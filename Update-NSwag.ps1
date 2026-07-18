<#
=====================================================================
NSwag client code generation script
Example usage:
	1. Open PowerShell in solution root
	2. Run: ./Update-NSwag.ps1
	   or: ./Update-NSwag.ps1 -Products $customProducts
	For each product:
	3. Script will run NSwag client code generation script
=====================================================================
#>

param (
	[Parameter(Mandatory = $false)]
	[array]$Products = @(
		@{ Name = "AgentFramework"; Root = ".\src"; Database = "AgentFramework"; ApiProject = "Presentation.Api" }
	)
)

Push-Location
try {
	function Write-Diag($msg) { Write-Host "[DIAG] $msg" -ForegroundColor Yellow }
	function Run-Verbose($cmd) {
		Write-Host "[DIAG] Running: $cmd" -ForegroundColor Yellow
		try {
			Invoke-Expression $cmd
		}
		catch {
			Write-Error "[ERROR] Command failed: $cmd"
			Write-Error $_
		}
	}

	foreach ($product in $Products) {
		$name = $product.Name
		$root = $product.Root
		$apiProject = $product.ApiProject

		Write-Host "[STEP] Product - $name" -ForegroundColor Cyan

		$nswagScript = "$root\$apiProject\Generate-NswagClientCode.ps1"
		if (Test-Path $nswagScript) {
			Write-Host "[STEP] Running NSwag code generation for $name ..." -ForegroundColor Cyan
			Set-Location "$root\$apiProject"
			Run-Verbose ".\Generate-NswagClientCode.ps1 -SkipBuildRestore"
			Pop-Location
			Push-Location
		}
		else {
			Write-Host "[WARN] NSwag script not found for $name - $nswagScript" -ForegroundColor Yellow
		}
	}

	Write-Host "[DONE] NSwag client code generation complete." -ForegroundColor Green
}
finally {
	Pop-Location
}
