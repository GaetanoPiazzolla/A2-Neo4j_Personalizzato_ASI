#!/bin/bash

# ==============================================================================
# Script di ripristino e inizializzazione per il database Neo4j (asi-neo4j)
# ==============================================================================
# Questo script si occupa di:
# 1. Fermare i container attuali e rimuoverli per evitare lock sul volume.
# 2. Eliminare il volume persistente 'asineo4j_data' per ripartire da zero.
# 3. Scaricare il dataset ufficiale di test "Recommendations" (dump 5.x).
# 4. Inizializzare offline il volume usando un container temporaneo:
#    - Carica i dati dal dump.
#    - Esegue la migrazione al formato moderno ad alte prestazioni 'aligned' (CalVer).
# 5. Avviare il database tramite docker-compose in background.
# 6. Attendere (--wait) che la readiness probe assicuri che i dati siano online.
# ==============================================================================
set -e

if ! docker info >/dev/null 2>&1; then
  echo "Errore: il daemon Docker non è in esecuzione o non è raggiungibile." >&2
  exit 1
fi

echo "Sto fermando i container Neo4j (se attivi)..."
docker-compose down 2>/dev/null || true
docker rm -f neo4j-demo 2>/dev/null || true

echo "Rimuovo i volumi persistenti per ripartire da zero..."
docker volume rm asineo4j_data || true
docker volume rm asineo4j_plugins || true
docker volume create asineo4j_plugins

echo "Scarico il database dump (Recommendations dataset per Neo4j 5)..."
mkdir -p backups
curl -L -s -o backups/neo4j.dump https://raw.githubusercontent.com/neo4j-graph-examples/recommendations/main/data/recommendations-50.dump

echo "Carico il dump nel volume Docker (tramite container temporaneo)..."
docker run --rm \
  -u root \
  --volume=asineo4j_data:/data \
  --volume="$(pwd)/backups:/backups" \
  neo4j:2026.07.1 \
  bash -c "neo4j-admin database load neo4j --from-path=/backups --overwrite-destination=true && neo4j-admin database migrate neo4j --to-format=aligned"

echo "Avvio di nuovo Neo4j in modalità detached con i dati pre-caricati..."
docker-compose up -d --wait

echo "Fatto! Neo4j sta ripartendo con i dati importati dal dump."
echo "Attendi qualche secondo affinché il database sia pronto (verifica l'healthcheck)."
echo ""
echo "=== Accesso all'interfaccia Neo4j ==="
echo "- Neo4j Workspace / Browser (Locale): http://localhost:7474"
echo "- Web Browser Ufficiale: https://browser.neo4j.io/ (collegati a neo4j://localhost:7687)"
echo "- Credenziali: neo4j / password"
echo "====================================="