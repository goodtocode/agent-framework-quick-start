<#
=====================================================================
EF/NSwag update script
Example usage:
	1. Open PowerShell in solution root
	2. Run: ./src/Reset-EfNswag.ps1
	   or: ./src/Reset-EfNswag.ps1 -Products $customProducts
	For each product
	3. Script will delete EF migration
	4. Script will recreate new migration with current model for that 1 product
	5. Script will update database with new migration for that 1 product
	6. Script will run NSwag client code generation script for that 1 product	
=====================================================================
#>

param (
	[Parameter(Mandatory = $false)]
	[array]$Products = @(
		@{ Name = "AgentFramework"; Root = ".\src"; Database = "AgentFramework"; ApiProject = "Presentation.Api" }
	),
	[string] $dropTablesPath = ".\data\Admin\Drop Tables.sql"
)

function Ensure-SqlCmd {
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

Ensure-SqlCmd

# STEP 1: Drop all tables using the provided SQL script, but only if the database exists
function Test-DatabaseExists {
	param (
		[string]$DatabaseName
	)
	$query = "IF DB_ID('$DatabaseName') IS NOT NULL SELECT 1 ELSE SELECT 0"
	$result = sqlcmd -S "(localdb)\MSSQLLocalDB" -Q $query -h -1 -W 2>$null
	return ($result -eq '1')
}

if (Test-Path $dropTablesPath) {
	if ($Products -and $Products.Count -gt 0) {
		$dbName = $Products[0].Database
	}
	if (Test-DatabaseExists -DatabaseName $dbName) {
		Write-Host "[STEP 1] Dropping all tables via $dropTablesPath on database '$dbName'..." -ForegroundColor Magenta
		$dropResult = sqlcmd -S "(localdb)\MSSQLLocalDB" -d $dbName -b -i $dropTablesPath 2>&1
		if ($LASTEXITCODE -ne 0) {
			Write-Host "[ERROR] Drop tables script failed: $dropResult" -ForegroundColor Red
			throw "[FAIL-FAST] Database drop failed. Stopping script."
		}
		Write-Host "[SUCCESS] All tables dropped." -ForegroundColor Green
	} else {
		Write-Host "[WARN] Database does not exist. Skipping drop tables step." -ForegroundColor Yellow
	}
} else {
	Write-Host "[WARN] Drop script not found at $dropTablesPath — skipping database reset. Ensure the database is clean before continuing." -ForegroundColor Yellow
}

Push-Location
try {
	function Write-Diag($msg) { Write-Host "[DIAG] $msg" -ForegroundColor Yellow }
	function Run-Verbose($cmd) {
		Write-Host "[DIAG] Running: " + $cmd -ForegroundColor Yellow
		try {
			Invoke-Expression $cmd
		}
		catch {
			Write-Error "[ERROR] Command failed: $cmd"
			Write-Error $_
		}
	}

	# Ensure dotnet-ef tool is available (idempotent)
	$srcPath = Join-Path $PSScriptRoot 'src'
	$toolsManifest = Join-Path $srcPath 'dotnet-tools.json'
	Push-Location $srcPath
	try {
		if (!(Test-Path -Path $toolsManifest)) {
			Write-Host "[STEP] Creating dotnet tool manifest in src/..." -ForegroundColor Magenta
			Run-Verbose "dotnet new tool-manifest --force"
		}
		$toolList = & dotnet tool list --local | Out-String
		if ($toolList -notmatch 'dotnet-ef') {
			Write-Host "[STEP] Installing dotnet-ef as a local tool in src/..." -ForegroundColor Magenta
			Run-Verbose "dotnet tool install dotnet-ef --local"
		} else {
			Write-Host "[STEP] dotnet-ef already installed as a local tool." -ForegroundColor Green
		}
		Write-Host "[STEP] Restoring local dotnet tools (dotnet-ef required) in src/..." -ForegroundColor Magenta
		Run-Verbose "dotnet tool restore"
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
		Run-Verbose "dotnet clean $infraProj"
		Run-Verbose "dotnet restore $infraProj"
		Run-Verbose "dotnet build $infraProj --no-restore"
		Run-Verbose "dotnet clean $webApiProj"
		Run-Verbose "dotnet restore $webApiProj"
		Run-Verbose "dotnet build $webApiProj --no-restore"

		Write-Host "Removing migration files"
		Remove-Item $infraPath -ErrorAction SilentlyContinue
		Write-Host "Creating new InitialCreate migration without dropping database..." -ForegroundColor Cyan
		# Build absolute paths from $PSScriptRoot (immune to CWD changes; avoids [IO.Path]::GetFullPath which
		# resolves against [Environment]::CurrentDirectory, not $PWD, causing wrong paths after Push-Location).
		$infraProjAbs = Join-Path $PSScriptRoot ($infraProj -replace '^\.[\\/]', '')
		$webApiProjAbs = Join-Path $PSScriptRoot ($webApiProj -replace '^\.[\\/]', '')
		Push-Location $srcPath
		try {
			Run-Verbose "dotnet ef migrations add InitialCreate-$context --project `"$infraProjAbs`" --startup-project `"$webApiProjAbs`" --context $context --verbose"
			Run-Verbose "dotnet ef database update --project `"$infraProjAbs`" --startup-project `"$webApiProjAbs`" --context $context --connection '$connection' --verbose"
		} finally {
			Pop-Location
		}

		$nswagScript = "$root\$apiProject\Generate-NswagClientCode.ps1"
		if (Test-Path $nswagScript) {
			Push-Location "$root\$apiProject"
			try {
				Run-Verbose ".\Generate-NswagClientCode.ps1 -SkipBuildRestore"
			} finally {
				Pop-Location
			}
		}
		else {
			Write-Host "NSwag script not found for " + $name + " - " + $nswagScript -ForegroundColor Red
		}

		Pop-Location
		Push-Location
	}
}
finally {
	Pop-Location
}