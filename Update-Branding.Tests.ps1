BeforeAll {
    $script:Script = Join-Path $PSScriptRoot 'Update-Branding.ps1'
    $script:Map = Join-Path $PSScriptRoot 'branding.map.json'

    function New-Fixture {
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("brand-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Path "$root\src\App", "$root\.github\workflows" -Force | Out-Null
        Set-Content "$root\Goodtocode.AgentFramework.Web.slnx" '<Solution />'
        Set-Content "$root\.github\workflows\gtc-agent-standalone-iac.yml" 'name: gtc-agent-standalone-iac'
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
        Test-Path "$Root\.github\workflows\contoso-agent-standalone-iac.yml" | Should -BeTrue
        $csproj = Get-Content "$Root\src\App\App.csproj" -Raw
        $csproj | Should -Match 'Contoso\.Assist\.App'
        $csproj | Should -Match 'Goodtocode\.Mediator'
        $csproj | Should -Match 'Goodtocode\.Agents\.Playbook'
        $csproj | Should -Not -Match 'bf5b92ab-f3fe-4af6-b299-583240397247'
        Get-Content "$Root\src\App\Page.razor" -Raw | Should -Match 'Contoso Assist.*Contoso \| developers@contoso\.com'
        Get-Content "$Root\src\App\Auth.cs" -Raw | Should -Match 'access_as_user'
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
