<#
=====================================================================
EF database reset script (drop + re-migrate)
Example usage:
	1. Open PowerShell in solution root
	2. Run: ./Reset-EfDatabase.ps1
	   or: ./Reset-EfDatabase.ps1 -Products $customProducts
	For each product:
	3. Script will drop the database
	4. Script will apply existing EF migrations to recreate schema

Notes:
	- This script does NOT create new migrations.
	- This script does NOT run NSwag client generation.
=====================================================================
#>

param (
	[Parameter(Mandatory = $false)]
	[array]$Products = @(
		@{ Name = "AgentFramework"; Root = ".\src"; Database = "AgentFramework"; ApiProject = "Presentation.Api" }
	),
	[switch]$DropDatabase,
	[switch]$DropTables,
	[string]$DropTablesPath = ".\data\Admin\Drop Tables.sql"
)

Push-Location
try {
	function Invoke-DiagnosticCommand($cmd) {
		Write-Host "[DIAG] Running: $cmd" -ForegroundColor Yellow
		try {
			Invoke-Expression $cmd
		}
		catch {
			Write-Error "[ERROR] Command failed: $cmd"
			Write-Error $_
			throw
		}
	}

	function Install-SqlCmd {
		$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
		if (-not $sqlcmd) {
			Write-Host "sqlcmd not found. Installing via winget..." -ForegroundColor Yellow
			winget install --id Microsoft.SQLServerCommandLineTools -e --silent
			$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
			if (-not $sqlcmd) {
				throw "sqlcmd installation failed. Please install manually."
			}
			Write-Host "sqlcmd installed." -ForegroundColor Green
		}
		else {
			Write-Host "sqlcmd is already installed." -ForegroundColor Green
		}
	}

	function Test-DatabaseExists {
		param (
			[string]$DatabaseName
		)

		$query = "IF DB_ID('$DatabaseName') IS NOT NULL SELECT 1 ELSE SELECT 0"
		$result = sqlcmd -S "(localdb)\MSSQLLocalDB" -Q $query -h -1 -W 2>$null
		return ($result -eq '1')
	}

	function Reset-DatabaseState {
		param (
			[string]$Name,
			[string]$Database,
			[string]$Context,
			[string]$Connection,
			[string]$InfraProjAbs,
			[string]$WebApiProjAbs
		)

		if ($DropDatabase -and $DropTables) {
			throw "Choose either -DropDatabase or -DropTables, not both."
		}

		$useDropDatabase = $DropDatabase
		$useDropTables = $DropTables
		if (-not $DropDatabase -and -not $DropTables) {
			# Preserve existing behavior for this script: drop and recreate database.
			$useDropDatabase = $true
		}

		if ($useDropTables) {
			Install-SqlCmd
			if (-not (Test-Path $DropTablesPath)) {
				throw "Drop tables script not found at $DropTablesPath"
			}
			if (Test-DatabaseExists -DatabaseName $Database) {
				Write-Host "[STEP] Dropping all tables for $Name via $DropTablesPath..." -ForegroundColor Magenta
				$dropResult = sqlcmd -S "(localdb)\MSSQLLocalDB" -d $Database -b -i $DropTablesPath 2>&1
				if ($LASTEXITCODE -ne 0) {
					throw "Drop tables script failed for ${Name}: $dropResult"
				}
				Write-Host "[SUCCESS] All tables dropped for $Name." -ForegroundColor Green
			}
			else {
				Write-Host "[WARN] Database '$Database' does not exist. Skipping drop tables step for $Name." -ForegroundColor Yellow
			}
		}

		if ($useDropDatabase) {
			Write-Host "[STEP] Dropping database for $Name..." -ForegroundColor Magenta
			Invoke-DiagnosticCommand "dotnet ef database drop --project `"$InfraProjAbs`" --startup-project `"$WebApiProjAbs`" --context $Context --connection '$Connection' --force --verbose"
		}
	}

	# Ensure dotnet-ef tool is available (idempotent).
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
		}
		else {
			Write-Host "[STEP] dotnet-ef already installed as a local tool." -ForegroundColor Green
		}

		Write-Host "[STEP] Restoring local dotnet tools..." -ForegroundColor Magenta
		Invoke-DiagnosticCommand "dotnet tool restore"
	}
	finally {
		Pop-Location
	}

	foreach ($product in $Products) {
		$name = $product.Name
		$root = $product.Root
		$database = $product.Database
		$apiProject = $product.ApiProject

		$infraProj = "$root\Infrastructure.SqlServer\Infrastructure.SqlServer.csproj"
		$webApiProj = "$root\$apiProject\$apiProject.csproj"
		$context = "${name}Context"
		$connection = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=$database;Min Pool Size=3;MultipleActiveResultSets=True;Trusted_Connection=Yes;TrustServerCertificate=True;Encrypt=True;Connection Timeout=30;"

		Write-Host "[STEP] Product - $name" -ForegroundColor Cyan
		Write-Host "[STEP] Context - $context" -ForegroundColor Cyan
		Write-Host "[STEP] Infra project - $infraProj" -ForegroundColor Cyan
		Write-Host "[STEP] WebApi project - $webApiProj" -ForegroundColor Cyan

		Invoke-DiagnosticCommand "dotnet clean $infraProj"
		Invoke-DiagnosticCommand "dotnet restore $infraProj"
		Invoke-DiagnosticCommand "dotnet build $infraProj --no-restore"
		Invoke-DiagnosticCommand "dotnet clean $webApiProj"
		Invoke-DiagnosticCommand "dotnet restore $webApiProj"
		Invoke-DiagnosticCommand "dotnet build $webApiProj --no-restore"

		# Build absolute paths from $PSScriptRoot to avoid CWD-related path resolution issues.
		$infraProjAbs = Join-Path $PSScriptRoot ($infraProj -replace '^\.[\\/]', '')
		$webApiProjAbs = Join-Path $PSScriptRoot ($webApiProj -replace '^\.[\\/]', '')

		Push-Location $srcPath
		try {
			Reset-DatabaseState -Name $name -Database $database -Context $context -Connection $connection -InfraProjAbs $infraProjAbs -WebApiProjAbs $webApiProjAbs

			Write-Host "[STEP] Applying existing migrations for $name..." -ForegroundColor Magenta
			Invoke-DiagnosticCommand "dotnet ef database update --project `"$infraProjAbs`" --startup-project `"$webApiProjAbs`" --context $context --connection '$connection' --verbose"
		}
		finally {
			Pop-Location
		}
	}

	Write-Host "[DONE] Database reset and migration update complete." -ForegroundColor Green
}
finally {
	Pop-Location
}
