#!/usr/bin/env bash
# Sobe Mongo (opcional), backend e frontend para desenvolvimento local.
# Uso:
#   ./dev.sh              # tenta Mongo via Docker; se falhar, usa stores em memória
#   ./dev.sh --in-memory  # força stores em memória (sem Docker)
#   ./dev.sh --no-mongo   # alias de --in-memory

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_PID=""
FRONTEND_PID=""
USE_IN_MEMORY=0

for arg in "$@"; do
  case "$arg" in
    --in-memory|--no-mongo)
      USE_IN_MEMORY=1
      ;;
    -h|--help)
      echo "Uso: ./dev.sh [--in-memory|--no-mongo]"
      exit 0
      ;;
    *)
      echo "Argumento desconhecido: $arg" >&2
      exit 1
      ;;
  esac
done

cleanup() {
  echo ""
  echo "Encerrando processos..."
  if [[ -n "${FRONTEND_PID}" ]] && kill -0 "${FRONTEND_PID}" 2>/dev/null; then
    kill "${FRONTEND_PID}" 2>/dev/null || true
  fi
  if [[ -n "${BACKEND_PID}" ]] && kill -0 "${BACKEND_PID}" 2>/dev/null; then
    kill "${BACKEND_PID}" 2>/dev/null || true
  fi
  wait 2>/dev/null || true
  echo "Finalizado."
}

trap cleanup EXIT INT TERM

need_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Comando obrigatório não encontrado: $1" >&2
    exit 1
  fi
}

need_cmd dotnet
need_cmd npm

echo "==> Preparando frontend (.env.local)"
if [[ ! -f "${ROOT}/frontend/.env.local" ]]; then
  cp "${ROOT}/frontend/.env.example" "${ROOT}/frontend/.env.local"
  echo "Criado frontend/.env.local a partir de .env.example"
fi

if [[ ! -d "${ROOT}/frontend/node_modules" ]]; then
  echo "==> npm install"
  (cd "${ROOT}/frontend" && npm install)
fi

if [[ "${USE_IN_MEMORY}" -eq 0 ]]; then
  echo "==> Subindo Mongo (docker compose)"
  if command -v docker >/dev/null 2>&1 && docker info >/dev/null 2>&1; then
    (cd "${ROOT}" && docker compose up -d)
  else
    echo "Docker indisponível — usando stores em memória."
    USE_IN_MEMORY=1
  fi
else
  echo "==> Modo in-memory (sem Mongo)"
fi

export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5080

if [[ "${USE_IN_MEMORY}" -eq 1 ]]; then
  export Testing__UseInMemoryStores=true
  export GooglePlaces__UseFakeSource=true
fi

echo "==> Backend em http://localhost:5080"
(
  cd "${ROOT}/backend"
  dotnet run --project WebScraping.Api --no-launch-profile
) &
BACKEND_PID=$!

echo "==> Aguardando health do backend..."
for _ in $(seq 1 60); do
  if curl -sf "http://localhost:5080/api/health" >/dev/null 2>&1; then
    echo "Backend ok."
    break
  fi
  if ! kill -0 "${BACKEND_PID}" 2>/dev/null; then
    echo "Backend encerrou antes de ficar saudável." >&2
    exit 1
  fi
  sleep 1
done

echo "==> Frontend em http://localhost:3000"
(
  cd "${ROOT}/frontend"
  npm run dev
) &
FRONTEND_PID=$!

echo ""
echo "Pronto."
echo "  API:  http://localhost:5080"
echo "  UI:   http://localhost:3000"
echo "  Ctrl+C para parar."
echo ""

wait
