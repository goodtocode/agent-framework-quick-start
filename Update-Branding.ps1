<#
.SYNOPSIS
Rebrands this repository (or a staged template copy) using branding.map.json.

.DESCRIPTION
-Target Values    : replaces GoodToCode branding with the supplied customer values (clone scenario).
-Target DotnetNew : injects dotnet new tokens into a staged copy of src (used by the template repo).
-Target Vsix      : injects VSIX tokens / neutral placeholders into a staged copy of src (used by the template repo).

Dry run by default. Use -Apply to write changes. Nothing is committed.

.EXAMPLE
pwsh ./Update-Branding.ps1 -NewCompany Contoso -NewProduct "Contoso Assist" -Namespace Contoso.Assist
pwsh ./Update-Branding.ps1 -NewCompany Contoso -NewProduct "Contoso Assist" -Namespace Contoso.Assist -Apply
#>
[CmdletBinding()]
param(
    [string] $Path = $PSScriptRoot,
    [ValidateSet('Values', 'DotnetNew', 'Vsix')]
    [string] $Target = 'Values',
    [string] $MapFile = (Join-Path $PSScriptRoot 'branding.map.json'),
    [string] $ConfigFile,

    [string] $NewCompany,
    [string] $NewProduct,
    [string] $Namespace,
    [string] $CompanySlug,
    [string] $CompanyShort,
    [string] $Domain,
    [string] $ContactEmail,
    [string] $RepoOwner,
    [string] $RepoName,
    [string] $TenantHost = 'your-tenant-name.ciamlogin.com',

    [switch] $Apply,
    [switch] $Force,
    [switch] $FailOnLeftover,
    [switch] $KeepUserSecretsIds,
    [switch] $IncludeLicense
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Path = (Resolve-Path $Path).Path
$map = Get-Content -Path $MapFile -Raw | ConvertFrom-Json
$isValues = $Target -eq 'Values'

function ConvertTo-Slug([string] $Text) {
    return ($Text.ToLowerInvariant() -replace '[^a-z0-9]+', '-').Trim('-')
}

# ---------- Resolve customer values (Values target only) ----------
$values = @{}
if ($isValues) {
    if ($ConfigFile) {
        $cfg = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json
        foreach ($name in 'NewCompany', 'NewProduct', 'Namespace', 'CompanySlug', 'CompanyShort', 'Domain', 'ContactEmail', 'RepoOwner', 'RepoName', 'TenantHost') {
            if (-not $PSBoundParameters.ContainsKey($name) -and $cfg.PSObject.Properties.Name -contains $name) {
                Set-Variable -Name $name -Value $cfg.$name
            }
        }
    }
    foreach ($required in 'NewCompany', 'NewProduct', 'Namespace') {
        if ([string]::IsNullOrWhiteSpace((Get-Variable -Name $required -ValueOnly))) {
            throw "-$required is required for -Target Values (or supply it in -ConfigFile)."
        }
    }
    if (-not $CompanySlug)  { $CompanySlug = (ConvertTo-Slug $NewCompany).Replace('-', '') }
    if (-not $CompanyShort) { $CompanyShort = $CompanySlug }
    if (-not $Domain)       { $Domain = "$CompanySlug.com" }
    if (-not $ContactEmail) { $ContactEmail = "developers@$Domain" }
    if (-not $RepoOwner)    { $RepoOwner = $CompanySlug }
    if (-not $RepoName)     { $RepoName = ConvertTo-Slug $NewProduct }

    $values = @{
        NewCompany = $NewCompany; NewProduct = $NewProduct; Namespace = $Namespace
        CompanySlug = $CompanySlug; CompanyShort = $CompanyShort; Domain = $Domain
        ContactEmail = $ContactEmail; RepoOwner = $RepoOwner; RepoName = $RepoName; TenantHost = $TenantHost
    }

    if ($Apply -and -not $Force -and (Get-Command git -ErrorAction SilentlyContinue)) {
        $dirty = git -C $Path status --porcelain 2>$null
        if ($LASTEXITCODE -eq 0 -and $dirty) {
            throw 'Working tree has uncommitted changes. Commit or stash them, or pass -Force.'
        }
    }
}

# ---------- Build the effective rule list for this target ----------
$newGuids = @{}
$rules = foreach ($rule in $map.rules) {
    $replacement = $null
    if ($isValues) {
        if ($rule.value -eq '@guid') {
            if ($KeepUserSecretsIds) { continue }
            $replacement = [guid]::NewGuid().ToString()
        }
        else {
            $replacement = $rule.value
            foreach ($key in $values.Keys) { $replacement = $replacement.Replace("{$key}", $values[$key]) }
        }
    }
    else {
        $replacement = $rule.tokens.$Target
        if ($null -eq $replacement) { continue }
    }
    $pattern = if ($rule.kind -eq 'regex') { $rule.find } else { [regex]::Escape($rule.find) }
    [pscustomobject]@{
        Id = $rule.id
        Regex = [regex]::new($pattern)
        Replacement = $replacement.Replace('$', '$$')
    }
}

$protect = @($map.protect)
if (-not $isValues) { $protect += @($map.protectForTokens) }
$protect = $protect | Sort-Object Length -Descending
$maskOpen = [string][char]0xE000
$maskClose = [string][char]0xE001

function Protect-Text([string] $Text) {
    for ($i = 0; $i -lt $protect.Count; $i++) { $Text = $Text.Replace($protect[$i], "$maskOpen$i$maskClose") }
    return $Text
}
function Restore-Text([string] $Text) {
    for ($i = 0; $i -lt $protect.Count; $i++) { $Text = $Text.Replace("$maskOpen$i$maskClose", $protect[$i]) }
    return $Text
}

$leftoverRegexes = @($map.leftover.$Target | ForEach-Object { [regex]::new($_) })

# ---------- Collect files ----------
$excludeDirs = @($map.excludeDirectories)
$excludeFiles = @($map.excludeFiles)
if ($IncludeLicense) { $excludeFiles = $excludeFiles | Where-Object { $_ -ne 'LICENSE' } }
$textExt = @($map.textExtensions)

$rootLen = $Path.TrimEnd('\', '/').Length + 1
$files = Get-ChildItem -Path $Path -Recurse -File -Force | Where-Object {
    $relative = $_.FullName.Substring($rootLen)
    $segments = $relative -split '[\\/]'
    foreach ($segment in $segments[0..($segments.Count - 2)]) {
        if ($segments.Count -gt 1 -and $excludeDirs -contains $segment) { return $false }
    }
    foreach ($pattern in $excludeFiles) { if ($_.Name -like $pattern) { return $false } }
    return ($textExt -contains $_.Extension.ToLowerInvariant()) -or ($textExt -contains $_.Name.ToLowerInvariant())
}

# ---------- Content pass ----------
$utf8 = [System.Text.UTF8Encoding]::new($false)
$bom = [byte[]](0xEF, 0xBB, 0xBF)
$changed = [System.Collections.Generic.List[object]]::new()
$leftovers = [System.Collections.Generic.List[object]]::new()
$guidMap = @{}

foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    if ($bytes.Length -eq 0) { continue }
    if (($bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) -or ($bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF)) {
        Write-Warning "Skipping UTF-16 file: $($file.FullName)"
        continue
    }
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $offset = if ($hasBom) { 3 } else { 0 }
    $original = $utf8.GetString($bytes, $offset, $bytes.Length - $offset)

    $text = Protect-Text $original
    $hits = [System.Collections.Generic.List[string]]::new()
    foreach ($rule in $rules) {
        $count = $rule.Regex.Matches($text).Count
        if ($count -eq 0) { continue }
        $replacement = $rule.Replacement
        if ($isValues -and $replacement -match '^[0-9a-f]{8}-') {
            if (-not $guidMap.ContainsKey($rule.Id)) { $guidMap[$rule.Id] = $replacement }
            $replacement = $guidMap[$rule.Id]
        }
        $text = $rule.Regex.Replace($text, $replacement)
        $hits.Add("$($rule.Id) x$count")
    }

    $lineNo = 0
    foreach ($line in ($text -split "`n")) {
        $lineNo++
        foreach ($rx in $leftoverRegexes) {
            if ($rx.IsMatch($line)) {
                $snippet = (Restore-Text $line).Trim()
                if ($snippet.Length -gt 120) { $snippet = $snippet.Substring(0, 120) }
                $leftovers.Add([pscustomobject]@{ File = $file.FullName.Substring($rootLen); Line = $lineNo; Text = $snippet })
                break
            }
        }
    }

    $updated = Restore-Text $text
    if ($updated -ne $original) {
        $changed.Add([pscustomobject]@{ File = $file.FullName.Substring($rootLen); Rules = ($hits -join ', ') })
        if ($Apply) {
            $body = $utf8.GetBytes($updated)
            $out = if ($hasBom) { $bom + $body } else { $body }
            [System.IO.File]::WriteAllBytes($file.FullName, $out)
        }
    }
}

# ---------- Rename pass (Values target only) ----------
$renames = [System.Collections.Generic.List[object]]::new()
if ($isValues) {
    $items = Get-ChildItem -Path $Path -Recurse -Force | Where-Object {
        $relative = $_.FullName.Substring($rootLen)
        $segments = $relative -split '[\\/]'
        -not ($segments | Where-Object { $excludeDirs -contains $_ })
    } | Sort-Object { $_.FullName.Length } -Descending

    foreach ($item in $items) {
        $newName = $item.Name
        foreach ($r in $map.renames) {
            $to = $r.value
            foreach ($key in $values.Keys) { $to = $to.Replace("{$key}", $values[$key]) }
            $newName = $newName.Replace($r.find, $to)
        }
        if ($newName -ne $item.Name) {
            $renames.Add([pscustomobject]@{ From = $item.FullName.Substring($rootLen); To = $newName })
            if ($Apply) { Rename-Item -LiteralPath $item.FullName -NewName $newName }
        }
    }
}

# ---------- Report ----------
$mode = if ($Apply) { 'APPLIED' } else { 'DRY RUN (use -Apply to write)' }
Write-Host "Update-Branding [$Target] $mode"
Write-Host "Files scanned: $($files.Count); files changed: $($changed.Count); renames: $($renames.Count)"
if (-not $isValues) { Write-Host "Tokens injected into: $Path" }
$changed | ForEach-Object { Write-Host "  ~ $($_.File)  [$($_.Rules)]" }
$renames | ForEach-Object { Write-Host "  > $($_.From) -> $($_.To)" }

if ($leftovers.Count -gt 0) {
    Write-Warning "Leftover branding in $($leftovers.Count) line(s):"
    $leftovers | Select-Object -First 50 | ForEach-Object { Write-Host "  ! $($_.File):$($_.Line)  $($_.Text)" }
    if ($leftovers.Count -gt 50) { Write-Host "  ... $($leftovers.Count - 50) more" }
}

if ($Apply -and $isValues) {
    Write-Host ''
    Write-Host 'Next steps:'
    Write-Host '  1. Create Entra app registrations and user-secrets for the new product (see README: New-EntraAppRegistrations.ps1).'
    if (-not $KeepUserSecretsIds) { Write-Host '  2. UserSecretsId values were regenerated; previously stored secrets do not carry over.' }
    Write-Host '  3. Review the diff, build and test before committing.'
}

if ($leftovers.Count -gt 0 -and ($FailOnLeftover -or ($Apply -and $isValues))) { exit 1 }
exit 0
