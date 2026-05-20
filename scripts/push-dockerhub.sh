#!/usr/bin/env sh
# Сборка и публикация multi-arch (Intel + Apple Silicon) на Docker Hub
set -eu
docker buildx create --name fintracker-builder --use 2>/dev/null || docker buildx use fintracker-builder 2>/dev/null || docker buildx use default
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t alxctf/fintracker:latest \
  -t alxctf/fintracker:main \
  --push \
  .
