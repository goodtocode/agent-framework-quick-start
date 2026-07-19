####################################################################################
# To execute
#   1. In powershell, set security polilcy for this script: 
#      Set-ExecutionPolicy Unrestricted -Scope Process -Force
#   2. Change directory to the script folder:
#      CD C:\Scripts (wherever your script is)
#   3. In powershell, run script: 
#      .\Generate-NswagClientCode.ps1
# Imperva Swagger: https://docs.imperva.com/bundle/cloud-application-security/page/cloud-v1-api-definition.htm
####################################################################################

param (
    [string]$SwaggerJsonPath = 'swagger',
    [string]$ApiAssembly = 'Goodtocode.AgentFramework.Presentation.Api.dll',
    [string]$ApiVersion = 'v1',
    [string]$ClientPathFile = '../Goodtocode.AgentFramework.Web/Clients/BackendApiClient.g.cs',
    [string]$ClientNamespace = 'Goodtocode.AgentFramework.Web',
    [string]$Configuration = 'Debug',
    [string]$TargetFramework = 'net10.0',
    [switch]$SkipBuildRestore
)
####################################################################################
if ($IsWindows) {
    Set-ExecutionPolicy Unrestricted -Scope Process -Force
}
$VerbosePreference = 'SilentlyContinue' # 'Continue'
####################################################################################

$apiAssemblyName = [System.IO.Path]::GetFileName($ApiAssembly)
if ([string]::IsNullOrWhiteSpace($apiAssemblyName)) {
    throw "ApiAssembly is required and must be a file name (with or without path)."
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptRoot
try {

$swaggerJsonPathFile = "$SwaggerJsonPath/$ApiVersion/swagger.json"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:OpenAI__ApiKey = "123"
if (!(Test-Path -Path "$SwaggerJsonPath/$ApiVersion")) {
    New-Item -ItemType Directory -Path "$SwaggerJsonPath/$ApiVersion" | Out-Null
}

if (-not $SkipBuildRestore) {
    dotnet restore
    $buildArgs = @('build', '--configuration', $Configuration)
    if (-not [string]::IsNullOrWhiteSpace($TargetFramework)) {
        $buildArgs += @('--framework', $TargetFramework)
    }
    dotnet @buildArgs
}

$apiAssemblyPathSegments = @($scriptRoot, 'bin')
if (-not [string]::IsNullOrWhiteSpace($Configuration)) {
    $apiAssemblyPathSegments += $Configuration
}
if (-not [string]::IsNullOrWhiteSpace($TargetFramework)) {
    $apiAssemblyPathSegments += $TargetFramework
}
$apiAssemblyPathSegments += $apiAssemblyName
$apiAssemblyPath = [System.IO.Path]::Combine([string[]]$apiAssemblyPathSegments)

if ([string]::IsNullOrWhiteSpace($apiAssemblyPath) -or -not (Test-Path -Path $apiAssemblyPath)) {
    $binPath = Join-Path $scriptRoot 'bin'
    if (Test-Path -Path $binPath) {
        $allCandidateAssemblies = @(Get-ChildItem -Path $binPath -Filter $apiAssemblyName -File -Recurse)
        $candidateAssemblies = $allCandidateAssemblies

        if (-not [string]::IsNullOrWhiteSpace($Configuration)) {
            $configurationPattern = "[\\/]bin[\\/]$([regex]::Escape($Configuration))[\\/]"
            $configurationMatches = @($candidateAssemblies | Where-Object { $_.FullName -match $configurationPattern })
            if ($configurationMatches.Count -gt 0) {
                $candidateAssemblies = $configurationMatches
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($TargetFramework)) {
            $frameworkPattern = "[\\/]$([regex]::Escape($TargetFramework))[\\/]"
            $frameworkMatches = @($candidateAssemblies | Where-Object { $_.FullName -match $frameworkPattern })
            if ($frameworkMatches.Count -gt 0) {
                $candidateAssemblies = $frameworkMatches
            }
        }

        if ($candidateAssemblies.Count -eq 0) {
            $candidateAssemblies = $allCandidateAssemblies
        }

        $apiAssemblyPath = $candidateAssemblies |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1 -ExpandProperty FullName
    }
}

if ([string]::IsNullOrWhiteSpace($apiAssemblyPath) -or -not (Test-Path -Path $apiAssemblyPath)) {
    throw "Could not resolve API assembly path. Verify -ApiAssembly, -Configuration, and -TargetFramework, or run without -SkipBuildRestore to build first."
}

# Pinned tool versions — update these when upgrading
$swashVersion = '10.1.0'
$nswagVersion  = '14.6.3'

# Ensure tool manifest exists (required for local tool installs).
if (-not (Test-Path -Path 'dotnet-tools.json')) {
    dotnet new tool-manifest
}

# Restore tools from manifest first so local tool commands are available in CI.
dotnet tool restore

# Ensure local tools are installed at the pinned version (install if missing, update if wrong version)
$localTools = dotnet tool list --local 2>&1
if (-not ($localTools | Select-String 'swashbuckle.aspnetcore.cli' -Quiet)) {
    dotnet tool install swashbuckle.aspnetcore.cli --local --version $swashVersion
} elseif (-not ($localTools | Select-String ([regex]::Escape($swashVersion)) -Quiet)) {
    dotnet tool update swashbuckle.aspnetcore.cli --local --version $swashVersion
}
if (-not ($localTools | Select-String 'nswag.consolecore' -Quiet)) {
    dotnet tool install nswag.consolecore --local --version $nswagVersion
} elseif (-not ($localTools | Select-String ([regex]::Escape($nswagVersion)) -Quiet)) {
    dotnet tool update nswag.consolecore --local --version $nswagVersion
}
dotnet tool run swagger tofile --output $swaggerJsonPathFile $apiAssemblyPath $ApiVersion
if ($LASTEXITCODE -ne 0) {
    throw "Swagger generation command failed."
}

if (Test-Path -Path $swaggerJsonPathFile) {    
    Write-Host "swagger.json generated successfully with OpenAPI 3.0.0."
}
else {
    Write-Error "swagger.json was not generated. Please check for build errors or missing dependencies."
    throw "swagger.json generation failed."
}

dotnet tool run nswag run Generate-NswagClientCode.json
if ($LASTEXITCODE -ne 0) {
    throw "NSwag client generation command failed."
}
}
finally {
    Pop-Location
}