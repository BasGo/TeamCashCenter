# Mannschaftskasse

Blazor Server Anwendung zur Verwaltung einer Mannschaftskasse. SQLite als Datenbank, EF Core für Migrations.

Quickstart:


1. .NET 10 SDK installieren

2. Abhängigkeiten wiederherstellen:

```bash
dotnet restore
```

3. Migrationen erstellen (erstmals):

```bash
dotnet tool restore
./scripts/create_migration.sh InitialCreate
dotnet ef database update
```

Falls `dotnet ef` nicht verfügbar ist, installiere das EF-Tool global:

```bash
dotnet tool install --global dotnet-ef
```

4. App lokal starten:

```bash
dotnet run
```

5. Docker build (runtime: .NET 10):

```bash
docker build -t mannschaftskasse:10 .
```

Alternatively use Docker Compose which maps `./data` and `./logs` for persistence:

```bash
docker compose up --build
```

Hinweise:
- The application uses SQLite. By default the DB file will be created in `./data/TeamCashCenter.db` when running in Docker or locally.
- Für Azure-Deployment: Docker-Image erstellen und in Azure Web App for Containers oder Azure Container Apps deployen.

UI Hinweise:
- Die Oberfläche nutzt Bootstrap 5 und Bootstrap Icons (über CDN eingebunden in `_Host.cshtml`).
- Formvalidierungen werden mit DataAnnotations angezeigt (clientseitig in Blazor Forms über `DataAnnotationsValidator` und `ValidationMessage`).


