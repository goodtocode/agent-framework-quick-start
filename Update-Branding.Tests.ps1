BeforeAll {
    $script:Script = Join-Path $PSScriptRoot 'Update-Branding.ps1'
    $script:Map = Join-Path $PSScriptRoot 'branding.map.json'

    function New-Fixture {
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("brand-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Path "$root\src\App", "$root\.github\workflows", "$root\src\Infrastructure.AgentFramework", "$root\.github\scripts\setup", "$root\.github\scripts\cd", "$root\.azure\scripts", "$root\.azure\variables", "$root\.azuredevops\pipelines", "$root\src\Presentation.Api", "$root\src\Presentation.Web" -Force | Out-Null
        Set-Content "$root\Goodtocode.AgentFramework.Web.slnx" '<Solution />'
        Set-Content "$root\.github\workflows\agentframework-iac.yml" 'PRODUCT: "agentframework"'
        Set-Content "$root\.azure\variables\development.bicepparam" "var productIac = 'agentframework'"
        Set-Content "$root\.azuredevops\pipelines\gtc-agentframework-app.yml" 'group: gtc-agentframework-dev'
        $entryPoint = Get-Content "$PSScriptRoot\Initialize-Installation.ps1" -Raw
        $entryPoint = $entryPoint -replace '\$CompanyName = ''[^'']+''', '$CompanyName = ''Goodtocode'''
        $entryPoint = $entryPoint -replace '\$ProductName = ''[^'']+''', '$ProductName = ''AgentFramework'''
        Set-Content "$root\Initialize-Installation.ps1" $entryPoint
        Copy-Item "$PSScriptRoot\.github\scripts\setup\Initialize-Product.ps1" "$root\.github\scripts\setup"
        foreach ($file in @('src\Presentation.Api\Presentation.Api.csproj', 'src\Presentation.Web\Presentation.Web.csproj', '.azure\scripts\New-EntraDotnetSolution.ps1', '.github\scripts\cd\New-Github-Azure-Federation.ps1')) {
            Set-Content (Join-Path $root $file) 'synthetic setup fixture'
        }
        Set-Content "$root\src\App\IAgentFrameworkContext.cs" 'namespace Goodtocode.AgentFramework.App; interface IAgentFrameworkContext {}'
        Set-Content "$root\src\App\AgentFrameworkContextModelSnapshot.cs" 'class AgentFrameworkContextModelSnapshot {}'
        Set-Content "$root\src\App\20260101000000_InitialCreate-AgentFrameworkContext.Designer.cs" '[DbContext(typeof(AgentFrameworkContext))]'
        Set-Content "$root\Reset-EfDatabase.ps1" '$Products = @{ Name = "AgentFramework"; Database = "AgentFramework" }'
        Set-Content "$root\src\Infrastructure.AgentFramework\Infrastructure.AgentFramework.csproj" @'
<Project><PropertyGroup><RootNamespace>Goodtocode.AgentFramework.Infrastructure.AgentFramework</RootNamespace></PropertyGroup>
<ItemGroup><PackageReference Include="Microsoft.AgentFramework" Version="1.0.0" />
<PackageReference Include="Microsoft.Agents.AI" Version="1.0.0" /></ItemGroup></Project>
'@
        Set-Content "$root\src\App\Database.json" '{"Connection":"Initial Catalog=AgentFramework","Context":"AgentFrameworkContext"}'
        Set-Content "$root\src\App\App.csproj" @'
<Project><PropertyGroup>
<RootNamespace>Goodtocode.AgentFramework.App</RootNamespace>
<UserSecretsId>bf5b92ab-f3fe-4af6-b299-583240397247</UserSecretsId>
</PropertyGroup><ItemGroup>
<PackageReference Include="Goodtocode.Mediator" Version="1.0.0" />
<PackageReference Include="Goodtocode.Agents.Playbook" Version="1.0.0" />
</ItemGroup></Project>
'@
        Set-Content "$root\src\App\Page.razor" '<title>Quick-start for Microsoft Agent Framework</title><p>GoodToCode | developers@goodtocode.com</p>'
        Set-Content "$root\src\App\Auth.cs" 'var s = "access_as_user"; // Goodtocode.AgentFramework.App'
        return $root
    }
}

Describe 'Update-Branding Values' {
    BeforeEach { $script:Root = New-Fixture }
    AfterEach { Remove-Item $script:Root -Recurse -Force -ErrorAction SilentlyContinue }

    It 'is a dry run by default' {
        & $Script -Path $Root -NewCompany Contoso -NewProduct 'Contoso Assist' -Namespace Contoso.Assist | Out-Null
        Get-Content "$Root\src\App\Page.razor" -Raw | Should -Match 'GoodToCode'
        Test-Path "$Root\Goodtocode.AgentFramework.Web.slnx" | Should -BeTrue
    }

    It 'rebrands content and renames files while preserving dependencies and contracts' {
        & $Script -Path $Root -NewCompany Contoso -NewProduct 'Contoso Assist' -Namespace Contoso.Assist -Apply -Force | Out-Null
        $LASTEXITCODE | Should -Be 0
        Test-Path "$Root\Contoso.Assist.Web.slnx" | Should -BeTrue
        Test-Path "$Root\.github\workflows\assist-iac.yml" | Should -BeTrue
        $csproj = Get-Content "$Root\src\App\App.csproj" -Raw
        $csproj | Should -Match 'Contoso\.Assist\.App'
        $csproj | Should -Match 'Goodtocode\.Mediator'
        $csproj | Should -Match 'Goodtocode\.Agents\.Playbook'
        $csproj | Should -Not -Match 'bf5b92ab-f3fe-4af6-b299-583240397247'
        Get-Content "$Root\src\App\Page.razor" -Raw | Should -Match 'Contoso Assist.*Contoso \| developers@contoso\.com'
        Get-Content "$Root\src\App\Auth.cs" -Raw | Should -Match 'access_as_user'
        Test-Path "$Root\src\App\IAssistContext.cs" | Should -BeTrue
        Test-Path "$Root\src\App\AssistContextModelSnapshot.cs" | Should -BeTrue
        Test-Path "$Root\src\App\20260101000000_InitialCreate-AssistContext.Designer.cs" | Should -BeTrue
        Get-Content "$Root\src\App\Database.json" -Raw | Should -Match 'Initial Catalog=Assist.*AssistContext'
        Get-Content "$Root\Reset-EfDatabase.ps1" -Raw | Should -Match 'Name = "Assist"; Database = "Assist"'
        Get-Content "$Root\.github\workflows\assist-iac.yml" -Raw | Should -Match 'PRODUCT: "assist"'
        Get-Content "$Root\.azure\variables\development.bicepparam" -Raw | Should -Match "productIac = 'assist'"
        $infrastructure = Get-Content "$Root\src\Infrastructure.AgentFramework\Infrastructure.AgentFramework.csproj" -Raw
        $infrastructure | Should -Match 'Contoso.Assist.Infrastructure.AgentFramework'
        $infrastructure | Should -Match 'Include="Microsoft.AgentFramework"'
        $infrastructure | Should -Match 'Include="Microsoft.Agents.AI"'
        Test-Path "$Root\src\Infrastructure.Assist" | Should -BeFalse
        Get-Content "$Root\.azuredevops\pipelines\contoso-assist-app.yml" -Raw | Should -Match 'group: contoso-assist-dev'
    }

    It 'requires both brand values before writing anything' {
        { & $Script -Path $Root -Apply -Force } | Should -Throw '*NewCompany is required*'
        { & $Script -Path $Root -NewCompany Contoso -Apply -Force } | Should -Throw '*NewProduct is required*'
        { & $Script -Path $Root -NewCompany Contoso -NewProduct ' ' -Apply -Force } | Should -Throw '*NewProduct is required*'
        Test-Path "$Root\Goodtocode.AgentFramework.Web.slnx" | Should -BeTrue
    }

    It 'derives the namespace from two brand parameters and persists setup defaults' {
        & $Script -Path $Root -NewCompany Contoso -NewProduct Assist -Apply -Force | Out-Null
        $LASTEXITCODE | Should -Be 0
        Test-Path "$Root\Contoso.Assist.Web.slnx" | Should -BeTrue
        Mock Get-Command { [pscustomobject]@{ Name = 'Available' } } -ParameterFilter { $Name -in @('dotnet', 'git') }
        Mock Read-Host { throw 'Check must not prompt.' }
        $output = & "$Root\Initialize-Installation.ps1" -Stage Check 6>&1
        ($output | Out-String) | Should -Match 'verified for assist'
        Should -Invoke Read-Host -Times 0 -Exactly
        $setup = Get-Content "$Root\Initialize-Installation.ps1" -Raw
        $setup | Should -Match "CompanyName = 'Contoso'"
        $setup | Should -Match "ProductName = 'Assist'"
    }

    It 'reads both brands from a configuration file' {
        Set-Content "$Root\branding.json" '{"NewCompany":"Fabrikam","NewProduct":"Portal"}'
        & $Script -Path $Root -ConfigFile "$Root\branding.json" -Apply -Force | Out-Null
        $LASTEXITCODE | Should -Be 0
        Test-Path "$Root\Fabrikam.Portal.Web.slnx" | Should -BeTrue
    }

    It 'keeps explicit multi-part namespaces consistent with installation' {
        & $Script -Path $Root -NewCompany Contoso -NewProduct Assist -Namespace Contoso.Products.Assist -Apply -Force | Out-Null
        $LASTEXITCODE | Should -Be 0
        Test-Path "$Root\Contoso.Products.Assist.Web.slnx" | Should -BeTrue
        Mock Get-Command { [pscustomobject]@{ Name = 'Available' } } -ParameterFilter { $Name -in @('dotnet', 'git') }
        $output = & "$Root\Initialize-Installation.ps1" -Stage Check 6>&1
        ($output | Out-String) | Should -Match 'verified for assist'
    }

    It 'rejects invalid inferred identifiers before writing' {
        { & $Script -Path $Root -NewCompany Contoso -NewProduct 'Contoso Assist' -Apply -Force } | Should -Throw '*dotted .NET identifier*'
        Test-Path "$Root\Goodtocode.AgentFramework.Web.slnx" | Should -BeTrue
    }

    It 'keeps UserSecretsId values with -KeepUserSecretsIds' {
        & $Script -Path $Root -NewCompany Contoso -NewProduct 'Contoso Assist' -Namespace Contoso.Assist -Apply -Force -KeepUserSecretsIds | Out-Null
        Get-Content "$Root\src\App\App.csproj" -Raw | Should -Match 'bf5b92ab-f3fe-4af6-b299-583240397247'
    }
}

Describe 'Update-Branding token targets' {
    BeforeEach { $script:Root = New-Fixture }
    AfterEach { Remove-Item $script:Root -Recurse -Force -ErrorAction SilentlyContinue }

    It 'DotnetNew injects tokens and leaves the namespace prefix for sourceName' {
        & $Script -Path "$Root\src" -Target DotnetNew -Apply -FailOnLeftover | Out-Null
        $LASTEXITCODE | Should -Be 0
        Get-Content "$Root\src\App\Page.razor" -Raw | Should -Match '__PRODUCT_NAME__.*__COMPANY_NAME__'
        $csproj = Get-Content "$Root\src\App\App.csproj" -Raw
        $csproj | Should -Match 'Goodtocode\.AgentFramework\.App'
        $csproj | Should -Match '__USER_SECRETS_ID_API__'
    }

    It 'Vsix only injects guid tokens and neutral placeholders' {
        & $Script -Path "$Root\src" -Target Vsix -Apply -FailOnLeftover | Out-Null
        $LASTEXITCODE | Should -Be 0
        Get-Content "$Root\src\App\App.csproj" -Raw | Should -Match '\$guid1\$'
        Get-Content "$Root\src\App\Page.razor" -Raw | Should -Match 'GoodToCode'
    }
}
