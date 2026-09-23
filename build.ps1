param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$NuGet,
    [string]$MSBuild,
    [switch]$SemTestes
)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
if (!$NuGet) {
    $command = Get-Command nuget.exe -ErrorAction SilentlyContinue
    if ($command) { $NuGet = $command.Source }
}
if (!$NuGet -or !(Test-Path -LiteralPath $NuGet)) { throw 'Informe -NuGet com o caminho do nuget.exe ou inclua-o no PATH.' }
if (!$MSBuild) {
    $command = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($command) { $MSBuild = $command.Source }
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    if (!$MSBuild -and (Test-Path $vswhere)) {
        $MSBuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
    if (!$MSBuild) { $MSBuild = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/MSBuild.exe' }
}
if (!(Test-Path -LiteralPath $MSBuild)) { throw 'Instale o MSBuild/Visual Studio com desenvolvimento para desktop .NET.' }
$packageFolder = Join-Path $projectRoot 'packages'
foreach ($file in @('packages.config', 'src/Balcao.Dados/packages.config', 'tests/Balcao.Tests/packages.config', 'tests/Balcao.Integracao/packages.config')) {
    & $NuGet restore (Join-Path $projectRoot $file) -PackagesDirectory $packageFolder -ConfigFile (Join-Path $projectRoot 'NuGet.Config') -NonInteractive -Verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Falha ao restaurar $file" }
}
& $MSBuild (Join-Path $projectRoot 'Balcao.Servicos.sln') /t:Build /m /nologo /verbosity:minimal "/p:Configuration=$Configuration"
if ($LASTEXITCODE -ne 0) { throw 'A compilação falhou.' }
if (!$SemTestes) {
    New-Item -ItemType Directory -Force (Join-Path $projectRoot 'artifacts') | Out-Null
    $runner = Join-Path $packageFolder 'NUnit.ConsoleRunner.3.22.0/tools/nunit3-console.exe'
    & $runner (Join-Path $projectRoot "tests/Balcao.Tests/bin/$Configuration/Balcao.Tests.dll") --workers=1 "--result=$(Join-Path $projectRoot 'artifacts/unitarios.xml')" --noheader
    if ($LASTEXITCODE -ne 0) { throw 'Os testes unitários falharam.' }
}
