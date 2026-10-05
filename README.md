# Corso Neo4j – Repository dei corsisti

Ambiente pratico per il corso Neo4j (Cypher 5 / 25).

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
Alla fine lo script stampa "Fatto": a quel punto il database è pronto.

Se `./start-docker.sh` dà "permission denied" (succede dopo un download ZIP):

```bash
bash start-docker.sh
```

## Accesso

- Neo4j Browser: http://localhost:7474
- Credenziali: `neo4j` / `password`
- Bolt: `neo4j://localhost:7687`

## Dal Giorno 3: backend C#

Serve anche il [.NET 10 SDK](https://dotnet.microsoft.com/download) (`dotnet --version` → 10.x).

Ogni giorno trovi nuove cartelle con `git pull`:

| Cartella | A cosa serve |
|---|---|
| `giornoN-start/` | **la tua**: ci lavori tutto il giorno, completando i `// TODO LAB` |
| `giornoN-docente/` | il codice del docente, aggiornato prima di ogni LAB: **non modificarla** |
| `giornoN-sol/` | la soluzione completa, pubblicata a fine giornata |

```bash
git pull
cd giorno3-start/Neo4jBackend
dotnet run          # -> http://localhost:5234
```

Le richieste di prova sono in `Neo4jBackend.http` (Rider e Visual Studio le eseguono
direttamente; in VS Code serve l'estensione **REST Client**).

## Problemi comuni

- **"Docker non è in esecuzione"**: avvia Docker Desktop e aspetta che sia pronto
- **"La porta 7474 o 7687 è già usata"**: chiudi l'altro Neo4j (o altro programma) che le occupa
- **"Impossibile scaricare il dump"**: controlla la connessione a internet

## Comandi utili

```bash
docker compose down     # ferma Neo4j (i dati restano nel volume)
docker compose up -d    # riavvia senza ricaricare il dump
```

Se `docker compose` non funziona, usa `docker-compose` con il trattino.
