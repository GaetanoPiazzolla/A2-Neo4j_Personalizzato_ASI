@echo off
setlocal enabledelayedexpansion

:: Ricrea da zero il database Neo4j con il dataset "Recommendations".

:: Funziona da qualsiasi cartella
cd /d "%~dp0"

set "DUMP_URL=https://raw.githubusercontent.com/neo4j-graph-examples/recommendations/main/data/recommendations-50.dump"
set "IMAGE=neo4j:2026.07.1"

docker --version >nul 2>&1
if errorlevel 1 (
    echo [ERRORE] Docker non e' installato. Installa Docker Desktop.
    exit /b 1
)
docker info >nul 2>&1
if errorlevel 1 (
    echo [ERRORE] Docker non e' in esecuzione. Avvia Docker Desktop e riprova.
    exit /b 1
)
where curl >nul 2>&1
if errorlevel 1 (
    echo [ERRORE] curl non e' installato.
    exit /b 1
)

:: Compose v2 (plugin) oppure v1 (binario separato)
set "COMPOSE=docker compose"
docker compose version >nul 2>&1
if errorlevel 1 (
    set "COMPOSE=docker-compose"
    docker-compose version >nul 2>&1
    if errorlevel 1 (
        echo [ERRORE] Docker Compose non trovato.
        exit /b 1
    )
)

echo Sto fermando i container Neo4j (se attivi)...
%COMPOSE% down >nul 2>&1
docker rm -f neo4j-demo >nul 2>&1

:: Dopo lo stop: una porta occupata qui e' di un altro programma
call :check_port 7474
if errorlevel 1 exit /b 1
call :check_port 7687
if errorlevel 1 exit /b 1

echo Rimuovo i volumi persistenti per ripartire da zero...
docker volume rm asineo4j_data >nul 2>&1
docker volume rm asineo4j_plugins >nul 2>&1
docker volume create asineo4j_plugins >nul

echo Scarico il dataset Recommendations...
if not exist ".backups" mkdir .backups
curl -fL --retry 3 --connect-timeout 15 -# -o .backups\neo4j.dump.tmp "%DUMP_URL%"
if errorlevel 1 (
    del .backups\neo4j.dump.tmp >nul 2>&1
    if exist ".backups\neo4j.dump" (
        echo [ATTENZIONE] Download fallito: uso il dump gia' presente in .backups.
    ) else (
        echo [ERRORE] Impossibile scaricare il dump. Controlla la connessione a internet.
        exit /b 1
    )
) else (
    move /y .backups\neo4j.dump.tmp .backups\neo4j.dump >nul
)

echo Carico il dump nel volume Docker (la prima volta scarica anche l'immagine Neo4j)...
docker run --rm -u root --volume="asineo4j_data:/data" --volume="%CD%\.backups:/backups" %IMAGE% bash -c "neo4j-admin database load neo4j --from-path=/backups --overwrite-destination=true && neo4j-admin database migrate neo4j --to-format=aligned"
if errorlevel 1 (
    echo [ERRORE] Caricamento del dump fallito.
    exit /b 1
)

echo Avvio Neo4j...
%COMPOSE% up -d
if errorlevel 1 (
    echo [ERRORE] Avvio di Neo4j fallito ^(vedi il messaggio sopra^).
    exit /b 1
)

echo Attendo che Neo4j sia pronto...
set READY=0
for /L %%I in (1,1,60) do (
    if !READY! equ 0 (
        docker exec neo4j-demo cypher-shell -u neo4j -p password "RETURN 1" >nul 2>&1
        if not errorlevel 1 (
            set READY=1
        ) else (
            ping -n 4 127.0.0.1 >nul
        )
    )
)
if !READY! equ 0 (
    echo [ERRORE] Neo4j non risponde dopo 3 minuti. Controlla con: docker logs neo4j-demo
    exit /b 1
)

echo.
echo Fatto. Neo4j e' pronto.
echo - Neo4j Browser: http://localhost:7474
echo - Credenziali: neo4j / password
exit /b 0

:: Aspetta fino a 20 secondi che la porta si liberi (Docker la rilascia con ritardo)
:check_port
for /L %%T in (1,1,10) do (
    netstat -ano | findstr /R /C:":%1 .*LISTENING" >nul 2>&1
    if errorlevel 1 exit /b 0
    ping -n 3 127.0.0.1 >nul
)
echo [ERRORE] La porta %1 e' gia' usata da un altro programma ^(es. un altro Neo4j^). Chiudilo e riprova.
exit /b 1
