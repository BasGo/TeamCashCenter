# Mannschaftskasse

Blazor Server Anwendung zur Verwaltung einer Mannschaftskasse. SQLite als Datenbank, EF Core für Migrations.

Quickstart:


1. .NET 8 SDK installieren

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

5. Docker build:

```bash
docker build -t mannschaftskasse:latest .
```

Hinweise:
- Die Anwendung verwendet SQLite (`mannschaftskasse.db`) im Projektordner.
- Für Azure-Deployment: Docker-Image erstellen und in Azure Web App for Containers oder Azure Container Apps deployen.

UI Hinweise:
- Die Oberfläche nutzt Bootstrap 5 und Bootstrap Icons (über CDN eingebunden in `_Host.cshtml`).
- Formvalidierungen werden mit DataAnnotations angezeigt (clientseitig in Blazor Forms über `DataAnnotationsValidator` und `ValidationMessage`).


