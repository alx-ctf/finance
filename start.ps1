# Одна команда (PowerShell):
#   irm https://raw.githubusercontent.com/alx-ctf/finance/main/start.ps1 | iex
$ErrorActionPreference = "Stop"

$ComposeUrl = "https://raw.githubusercontent.com/alx-ctf/finance/main/docker-compose.hub.yml"
$Dir = if ($env:FINTRACKER_DIR) { $env:FINTRACKER_DIR } else { Join-Path $env:TEMP "fintracker" }

New-Item -ItemType Directory -Force -Path $Dir | Out-Null
Set-Location $Dir

Invoke-WebRequest -Uri $ComposeUrl -OutFile "docker-compose.hub.yml" -UseBasicParsing

# Apple Silicon (если когда-нибудь запускают PowerShell на Mac)
if (-not $env:FINTRACKER_PLATFORM -and (docker info -f '{{.Architecture}}' 2>$null) -match 'aarch64|arm64') {
    $env:FINTRACKER_PLATFORM = 'linux/amd64'
}

docker compose -f docker-compose.hub.yml pull
docker compose -f docker-compose.hub.yml up -d

$port = if ($env:APP_PORT) { $env:APP_PORT } else { "8080" }
Write-Host ""
Write-Host "FinTracker: http://localhost:$port"
Write-Host "Демо: demo@fintracker.local / Demo123!"
Write-Host "Остановка: docker compose -f $Dir\docker-compose.hub.yml down"
