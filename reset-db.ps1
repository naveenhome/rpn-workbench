# Deletes the local SQLite database. It is recreated, empty, on the next run.
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "src/Rpn.Web")
Remove-Item -Force -ErrorAction SilentlyContinue rpn.db, rpn.db-shm, rpn.db-wal
Write-Host "Database removed. It will be recreated on the next 'dotnet run'."
