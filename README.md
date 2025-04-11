# WTP-projekt – CI/CD & Databas

## 🚀 Kom igång lokalt

1. Starta databas via Docker:

```bash
docker-compose up -d
```

2. Kör backend:

```bash
cd server
dotnet run
```

3. Seed testdata:

```bash
dotnet run --project ./server/server.csproj seed
```

## 🧪 CI/CD på GitHub

- `init.sql` körs automatiskt i Actions
- Testdata fylls med `DatabaseSeeder.cs`
- Backend byggs och testas