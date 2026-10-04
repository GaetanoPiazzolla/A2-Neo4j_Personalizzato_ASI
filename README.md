# Corso Neo4j – Repository dei corsisti

Ambiente pratico per il corso Neo4j (Cypher 5 / 25) – Giorno 1 e Giorno 2.

## Prerequisiti

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installato e **in esecuzione**
- `git` e `curl`

Verifica da terminale:

```bash
docker --version
git --version
curl --version
```

## Avvio del database

Lo script ricrea da zero il database e carica il dataset **Recommendations**.

**Mac / Linux**
```bash
./start-docker.sh
```

**Windows (CMD)**
```cmd
start-docker.bat
```

Rieseguirlo in qualsiasi momento riporta il database allo stato iniziale.

## Accesso

- Neo4j Browser: http://localhost:7474
- Credenziali: `neo4j` / `password`
- Bolt: `neo4j://localhost:7687`

## Piano B (senza Docker)

Su [sandbox.neo4j.com](https://sandbox.neo4j.com) crea una Sandbox con il dataset **Recommendations**.

## Comandi utili

```bash
docker-compose down          # ferma Neo4j (i dati restano nel volume)
docker-compose up -d --wait  # riavvia senza ricaricare il dump
```
