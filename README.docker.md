Docker build & run (local)

Build image:

```bash
docker build -t mannschaftskasse:latest .
```

Run with host DB volume and port mapping (exposes app at http://localhost:5005):

```bash
docker run --rm -p 5005:80 -v $(pwd)/mannschaftskasse.db:/app/mannschaftskasse.db mannschaftskasse:latest
```

Or use docker-compose:

```bash
docker compose up --build
```

Notes:
- The SQLite DB file is mounted into the container so data persists on the host.
- If you run under Docker for Mac, ensure file permissions allow the container to write the DB file.
