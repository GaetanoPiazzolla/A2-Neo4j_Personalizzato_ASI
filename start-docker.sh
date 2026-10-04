#!/bin/bash

# Ricrea da zero il database Neo4j con il dataset "Recommendations".
set -euo pipefail

# Funziona da qualsiasi cartella
cd "$(dirname "$0")"

DUMP_URL="https://raw.githubusercontent.com/neo4j-graph-examples/recommendations/main/data/recommendations-50.dump"
IMAGE="neo4j:2026.07.1"

fail() {
  echo "Errore: $1" >&2
  exit 1
}

command -v docker >/dev/null 2>&1 || fail "Docker non è installato. Installa Docker Desktop."
docker info >/dev/null 2>&1 || fail "Docker non è in esecuzione. Avvia Docker Desktop e riprova."
command -v curl >/dev/null 2>&1 || fail "curl non è installato."

# Compose v2 (plugin) oppure v1 (binario separato)
if docker compose version >/dev/null 2>&1; then
  COMPOSE="docker compose"
elif command -v docker-compose >/dev/null 2>&1; then
  COMPOSE="docker-compose"
else
  fail "Docker Compose non trovato."
fi

echo "Sto fermando i container Neo4j (se attivi)..."
$COMPOSE down >/dev/null 2>&1 || true
docker rm -f neo4j-demo >/dev/null 2>&1 || true

# Docker rilascia le porte con ritardo: aspetta fino a 20 secondi
wait_port_free() {
  for _ in $(seq 1 10); do
    lsof -nP -iTCP:"$1" -sTCP:LISTEN >/dev/null 2>&1 || return 0
    sleep 2
  done
  fail "la porta $1 è già usata da un altro programma (es. un altro Neo4j). Chiudilo e riprova."
}

if command -v lsof >/dev/null 2>&1; then
  wait_port_free 7474
  wait_port_free 7687
fi

echo "Rimuovo i volumi persistenti per ripartire da zero..."
docker volume rm asineo4j_data >/dev/null 2>&1 || true
docker volume rm asineo4j_plugins >/dev/null 2>&1 || true
docker volume create asineo4j_plugins >/dev/null

echo "Scarico il dataset Recommendations..."
mkdir -p .backups
if curl -fL --retry 3 --connect-timeout 15 -# -o .backups/neo4j.dump.tmp "$DUMP_URL"; then
  mv .backups/neo4j.dump.tmp .backups/neo4j.dump
elif [ -s .backups/neo4j.dump ]; then
  rm -f .backups/neo4j.dump.tmp
  echo "Download fallito: uso il dump già presente in .backups/." >&2
else
  rm -f .backups/neo4j.dump.tmp
  fail "impossibile scaricare il dump. Controlla la connessione a internet."
fi

echo "Carico il dump nel volume Docker (la prima volta scarica anche l'immagine Neo4j)..."
docker run --rm \
  -u root \
  --volume=asineo4j_data:/data \
  --volume="$(pwd)/.backups:/backups" \
  "$IMAGE" \
  bash -c "neo4j-admin database load neo4j --from-path=/backups --overwrite-destination=true && neo4j-admin database migrate neo4j --to-format=aligned" \
  || fail "caricamento del dump fallito."

echo "Avvio Neo4j..."
$COMPOSE up -d || fail "avvio di Neo4j fallito (vedi il messaggio sopra)."

echo "Attendo che Neo4j sia pronto..."
ready=0
for _ in $(seq 1 60); do
  if docker exec neo4j-demo cypher-shell -u neo4j -p password "RETURN 1" >/dev/null 2>&1; then
    ready=1
    break
  fi
  sleep 3
done
[ "$ready" -eq 1 ] || fail "Neo4j non risponde dopo 3 minuti. Controlla con: docker logs neo4j-demo"

echo ""
echo "Fatto! Neo4j è pronto."
echo "- Neo4j Browser: http://localhost:7474"
echo "- Credenziali: neo4j / password"
