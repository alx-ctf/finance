#!/usr/bin/env sh
# Одна команда: curl -fsSL https://raw.githubusercontent.com/alx-ctf/finance/main/start.sh | sh
set -eu

COMPOSE_URL="https://raw.githubusercontent.com/alx-ctf/finance/main/docker-compose.hub.yml"
DIR="${FINTRACKER_DIR:-${TMPDIR:-/tmp}/fintracker}"

mkdir -p "$DIR"
cd "$DIR"

if command -v curl >/dev/null 2>&1; then
  curl -fsSL -o docker-compose.hub.yml "$COMPOSE_URL"
elif command -v wget >/dev/null 2>&1; then
  wget -qO docker-compose.hub.yml "$COMPOSE_URL"
else
  echo "Нужен curl или wget" >&2
  exit 1
fi

docker compose -f docker-compose.hub.yml pull
docker compose -f docker-compose.hub.yml up -d

PORT="${APP_PORT:-8080}"
echo ""
echo "FinTracker: http://localhost:${PORT}"
echo "Демо: demo@fintracker.local / Demo123!"
echo "Остановка: docker compose -f ${DIR}/docker-compose.hub.yml down"
