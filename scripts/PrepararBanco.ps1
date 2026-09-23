param(
    [string]$Servidor = '(localdb)\MSSQLLocalDB',
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]{0,63}$')][string]$NomeBanco = 'BalcaoServicos'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$connectionBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$connectionBuilder['Data Source'] = $Servidor
$connectionBuilder['Initial Catalog'] = 'master'
$connectionBuilder['Integrated Security'] = $true
$connectionBuilder['Connect Timeout'] = 15
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionBuilder.ConnectionString)
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = 'SELECT DB_ID(@nome)'
    $null = $command.Parameters.Add('@nome', [System.Data.SqlDbType]::NVarChar, 128)
    $command.Parameters['@nome'].Value = $NomeBanco
    if ($command.ExecuteScalar() -is [DBNull]) {
        $command.Parameters.Clear()
        $command.CommandText = "CREATE DATABASE [$NomeBanco]"
        $command.ExecuteNonQuery() | Out-Null
    }
    $connection.ChangeDatabase($NomeBanco)
    $schemaFile = Join-Path $PSScriptRoot '../database/001_schema.sql'
    $schema = Get-Content -LiteralPath $schemaFile -Raw -Encoding UTF8
    foreach ($batch in [regex]::Split($schema, '(?im)^\s*GO\s*;?\s*$')) {
        if ([string]::IsNullOrWhiteSpace($batch)) { continue }
        $command.Parameters.Clear()
        $command.CommandText = $batch
        $command.ExecuteNonQuery() | Out-Null
    }
    Write-Output "Banco $NomeBanco preparado. Nenhum registro existente foi removido."
} finally {
    if ($command) { $command.Dispose() }
    $connection.Dispose()
}
