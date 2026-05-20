# FinTracker — запуск с Docker Hub

Репозиторий образа: https://hub.docker.com/r/alxctf/fintracker  
Исходники и скрипты: https://github.com/alx-ctf/finance

## Одна команда

**Linux / macOS:**
```bash
curl -fsSL https://raw.githubusercontent.com/alx-ctf/finance/main/start.sh | sh
```

**Windows (PowerShell):**
```powershell
irm https://raw.githubusercontent.com/alx-ctf/finance/main/start.ps1 | iex
```

→ http://localhost:8080  
Демо: `demo@fintracker.local` / `Demo123!`

## Mac (Apple Silicon)

Образ на Hub — `linux/amd64`. Скрипт `start.sh` включает эмуляцию на M1/M2/M3 автоматически.

## Остановка

```bash
docker compose -f /tmp/fintracker/docker-compose.hub.yml down
```
(Windows: `%TEMP%\fintracker\docker-compose.hub.yml`)

## Не запускайте

Только контейнер `alxctf/fintracker` без PostgreSQL — будет ошибка `localhost:5432`.

## Теги на Hub

| Тег | Назначение |
|-----|------------|
| `latest` | последняя стабильная |
| `main` | синхрон с веткой main |

## Публикация образа (для автора)

```bash
docker build -t alxctf/fintracker:latest -t alxctf/fintracker:main .
docker push alxctf/fintracker:latest
docker push alxctf/fintracker:main
```

Multi-arch (Intel + Apple Silicon):
```bash
sh scripts/push-dockerhub.sh
```
