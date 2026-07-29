<#
=====================================================================
EF/NSwag update script
Example usage:
	1. Open PowerShell in solution root
	2. Run: ./Reset-EfNswag.ps1
	   or: ./Reset-EfNswag.ps1 -Products $customProducts
	For each product
		3. Script will delete EF migrations (default)
		4. Script will recreate InitialCreate migration (default)
		5. Script will update database with migrations (default)
		6. Script will run NSwag client code generation script for that 1 product

	Recommended usage:
		- Default full reset+migrate flow: ./Reset-EfNswag.ps1
		- Push migrations only (keep existing migration files): ./Reset-EfNswag.ps1 -SkipResetMigrations
		- Clear tables once per database before push (default behavior): ./Reset-EfNswag.ps1 -DropTables
		- Drop database once per database before push: ./Reset-EfNswag.ps1 -DropDatabase
=====================================================================
#>

param (
	[Parameter(Mandatory = $false)]
	[array]$Products = @(
		@{ Name = "AgentFramework"; Root = ".\src"; Database = "AgentFramework"; ApiProject = "Presentation.Api" }
	),
	[switch]$SkipResetMigrations,
	[switch]$DropDatabase,
	[switch]$DropTables,
	[string]$dropTablesPath = ".\data\Admin\Drop Tables.sql"
)

function Install-SqlCmd {
	$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
	if (-not $sqlcmd) {
		Write-Host "sqlcmd not found. Installing via winget..." -ForegroundColor Yellow
		winget install --id Microsoft.SQLServerCommandLineTools -e --silent
		$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
		if (-not $sqlcmd) {
			throw "sqlcmd installation failed. Please install manually."
			$dropCreateResult = sqlcmd -Q $dropCreateCmd -b 2>&1
			if ($LASTEXITCODE -ne 0) {
				Write-Host "[ERROR] sqlcmd failed for $name - $dropCreateResult" -ForegroundColor Red
				Pop-Location
				throw "[FAIL-FAST] sqlcmd failed for $name. Stopping script."
			}
		}
		else {
			Write-Host "sqlcmd is already installed." -ForegroundColor Green
		}
	}
}

Install-SqlCmd

function Test-DatabaseExists {
	param (
		[string]$DatabaseName
	)
	$query = "IF DB_ID('$DatabaseName') IS NOT NULL SELECT 1 ELSE SELECT 0"
	$result = sqlcmd -S "(localdb)\MSSQLLocalDB" -Q $query -h -1 -W 2>$null
	return ($result -eq '1')
}

if ($DropDatabase -and $DropTables) {
	throw "Choose either -DropDatabase or -DropTables, not both."
}

# Preserve existing behavior for this script: drop tables unless explicitly overridden.
$useDropTables = $DropTables
$useDropDatabase = $DropDatabase
$useResetMigrations = -not $SkipResetMigrations
if (-not $DropDatabase -and -not $DropTables) {
	$useDropTables = $true
}

Push-Location
try {
	function Write-Diag($msg) { Write-Host "[DIAG] $msg" -ForegroundColor Yellow }
	function Invoke-DiagnosticCommand($cmd) {
		Write-Host "[DIAG] Running: " + $cmd -ForegroundColor Yellow
		try {
			Invoke-Expression $cmd
			if ($LASTEXITCODE -ne 0) {
				throw "Command exited with code $LASTEXITCODE"
			}
		}
		catch {
			Write-Error "[ERROR] Command failed: $cmd"
			Write-Error $_
			throw
		}
	}

	$databasesDroppedByTable = @{}
	$databasesDroppedByDrop = @{}

	# Ensure dotnet-ef tool is available (idempotent)
	$srcPath = Join-Path $PSScriptRoot 'src'
	$toolsManifest = Join-Path $srcPath 'dotnet-tools.json'
	Push-Location $srcPath
	try {
		if (!(Test-Path -Path $toolsManifest)) {
			Write-Host "[STEP] Creating dotnet tool manifest in src/..." -ForegroundColor Magenta
			Invoke-DiagnosticCommand "dotnet new tool-manifest --force"
		}
		$toolList = & dotnet tool list --local | Out-String
		if ($toolList -notmatch 'dotnet-ef') {
			Write-Host "[STEP] Installing dotnet-ef as a local tool in src/..." -ForegroundColor Magenta
			Invoke-DiagnosticCommand "dotnet tool install dotnet-ef --local"
		} else {
			Write-Host "[STEP] dotnet-ef already installed as a local tool." -ForegroundColor Green
		}
		Write-Host "[STEP] Restoring local dotnet tools (dotnet-ef required) in src/..." -ForegroundColor Magenta
		Invoke-DiagnosticCommand "dotnet tool restore"
	} finally {
		Pop-Location
	}

	foreach ($product in $Products) {
		$name = $product.Name
		$root = $product.Root
		$database = $product.Database

		$infraPath = "$root\Infrastructure.SqlServer\Migrations\*.cs"
		$infraProj = "$root\Infrastructure.SqlServer\Infrastructure.SqlServer.csproj"
		$apiProject = $product.ApiProject
		$webApiProj = "$root\$apiProject\$apiProject.csproj"
		$context = "${name}Context"
		$connection = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=$database;Min Pool Size=3;MultipleActiveResultSets=True;Trusted_Connection=Yes;TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;"

		Write-Host "[STEP] Product - " + $name -ForegroundColor Cyan
		Write-Host "[STEP] Context - " + $context -ForegroundColor Cyan
		Write-Host "[STEP] Connection string - " + $connection -ForegroundColor Cyan
		Write-Host "[STEP] Infra project - " + $infraProj -ForegroundColor Cyan
		Write-Host "[STEP] WebApi project - " + $webApiProj -ForegroundColor Cyan

		# Restore and build before migrations
		Invoke-DiagnosticCommand "dotnet clean $infraProj"
		Invoke-DiagnosticCommand "dotnet restore $infraProj"
		Invoke-DiagnosticCommand "dotnet build $infraProj --no-restore"
		Invoke-DiagnosticCommand "dotnet clean $webApiProj"
		Invoke-DiagnosticCommand "dotnet restore $webApiProj"
		Invoke-DiagnosticCommand "dotnet build $webApiProj --no-restore"

		if ($useResetMigrations) {
			Write-Host "[STEP] Removing migration files for $name..." -ForegroundColor Magenta
			Remove-Item $infraPath -ErrorAction SilentlyContinue
			Write-Host "[STEP] Creating new InitialCreate migration for $context..." -ForegroundColor Cyan
		}
		else {
			Write-Host "[STEP] SkipResetMigrations enabled. Using existing migrations for $name." -ForegroundColor DarkCyan
		}
		# Build absolute paths from $PSScriptRoot (immune to CWD changes; avoids [IO.Path]::GetFullPath which
		# resolves against [Environment]::CurrentDirectory, not $PWD, causing wrong paths after Push-Location).
		$infraProjAbs = Join-Path $PSScriptRoot ($infraProj -replace '^\.[\\/]', '')
		$webApiProjAbs = Join-Path $PSScriptRoot ($webApiProj -replace '^\.[\\/]', '')

		if ($useDropTables -and -not $databasesDroppedByTable.ContainsKey($database)) {
			if (Test-Path $dropTablesPath) {
				if (Test-DatabaseExists -DatabaseName $database) {
					Write-Host "[STEP] Dropping all tables for shared database '$database' via $dropTablesPath..." -ForegroundColor Magenta
					$dropResult = sqlcmd -S "(localdb)\MSSQLLocalDB" -d $database -b -i $dropTablesPath 2>&1
					if ($LASTEXITCODE -ne 0) {
						Write-Host "[ERROR] Drop tables script failed for database '${database}': $dropResult" -ForegroundColor Red
						throw "[FAIL-FAST] Table drop failed for database '$database'. Stopping script."
					}
					Write-Host "[SUCCESS] All tables dropped for shared database '$database'." -ForegroundColor Green
				}
				else {
					Write-Host "[WARN] Database '$database' does not exist. Skipping drop tables step." -ForegroundColor Yellow
				}
				$databasesDroppedByTable[$database] = $true
			}
			else {
				throw "Drop script not found at $dropTablesPath"
			}
		}
		Push-Location $srcPath
		try {
			if ($useDropDatabase -and -not $databasesDroppedByDrop.ContainsKey($database)) {
				Write-Host "[STEP] Dropping shared database '$database' before applying contexts..." -ForegroundColor Magenta
				Invoke-DiagnosticCommand "dotnet ef database drop --project `"$infraProjAbs`" --startup-project `"$webApiProjAbs`" --context $context --connection '$connection' --force --verbose"
				$databasesDroppedByDrop[$database] = $true
			}
			if ($useResetMigrations) {
				Invoke-DiagnosticCommand "dotnet ef migrations add InitialCreate-$context --project `"$infraProjAbs`" --startup-project `"$webApiProjAbs`" --context $context --verbose"
			}
			Invoke-DiagnosticCommand "dotnet ef database update --project `"$infraProjAbs`" --startup-project `"$webApiProjAbs`" --context $context --connection '$connection' --verbose"
		} finally {
			Pop-Location
		}

		$nswagScript = "$root\$apiProject\Generate-NswagClientCode.ps1"
		if (Test-Path $nswagScript) {
			Push-Location "$root\$apiProject"
			try {
				Invoke-DiagnosticCommand ".\Generate-NswagClientCode.ps1 -SkipBuildRestore"
			} finally {
				Pop-Location
			}
		}
		else {
			Write-Host "NSwag script not found for " + $name + " - " + $nswagScript -ForegroundColor Red
		}

	}
}
finally {
	Pop-Location
}