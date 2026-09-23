param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
if (!$env:BALCAO_TEST_CONNECTION_STRING) { throw 'Defina BALCAO_TEST_CONNECTION_STRING apontando para um banco exclusivo de testes já preparado.' }
$projectRoot = Split-Path $PSScriptRoot -Parent
New-Item -ItemType Directory -Force (Join-Path $projectRoot 'artifacts') | Out-Null
$runner = Join-Path $projectRoot 'packages/NUnit.ConsoleRunner.3.22.0/tools/nunit3-console.exe'
& $runner (Join-Path $projectRoot "tests/Balcao.Integracao/bin/$Configuration/Balcao.Integracao.dll") --workers=1 "--result=$(Join-Path $projectRoot 'artifacts/integracao.xml')" --noheader
if ($LASTEXITCODE -ne 0) { throw 'A integração com SQL Server falhou.' }
