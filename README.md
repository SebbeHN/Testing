# 🧾 Testing – Testmiljö för WTP CRM-system

Detta repo innehåller testningen för CRM-systemet WTP.
---

## 🗂️ Struktur i repot

```bash
Testing/
├── WTP/                   # Själva CRM-systemet (frontend + backend)
│   ├── client/            # React frontend
│   └── server/            # .NET-backend
│       └── Tests/         # xUnit-enhetstester
├── N2NTest/               # End-to-end-tester (SpecFlow + Playwright)
├── Postman/               # API-testning (Postman-samlingar)
├── workflow/              # CI/CD workflows för automatiserad test & deploy
├── init.sql               # Init-script för PostgreSQL-databas
└── README.md              # Denna fil
```

---

## 🧪 Innehåll i testsviten

| Testtyp           | Plats            | Teknologi         | Innehåll |
|------------------|------------------|-------------------|----------|
| Enhetstestning    | `WTP/server/Tests/` | `xUnit`            | Login, ärenden, chattlogik |
| API-testning      | `Postman/`       | `Postman`         | Auth, ärenden, chatt-API  |
| End-to-end (E2E)  | `N2NTest/`       | `SpecFlow + Playwright` | Fulla användarflöden |
| CI/CD             | `workflow/`      | GitHub Actions    | Bygg, test och verifiera automatiskt |

---

## ⚙️ Komma igång lokalt

### 1. Klona repot
```bash
git clone https://github.com/SebbeHN/Testing.git
cd Testing
```

### 2. Initiera databas (PostgreSQL)
Kör `init.sql` mot din databas:
```bash
psql -U <user> -d wtp -f init.sql
```

### 3. Starta backend (.NET)
```bash
cd WTP/server
dotnet restore
dotnet run
```

### 4. Starta frontend (React)
```bash
cd WTP/client
npm install
npm run dev
```

---

## 🔍 Köra tester

### ✔️ Enhetstester
```bash
cd WTP/server
dotnet test
```

### ✔️ End-to-end-tester
```bash
cd N2NTest
# Körs via Playwright + SpecFlow i Rider
```

### ✔️ API-testning
Importera `Postman`-samlingen och kör mot lokal server.

---

## ⚒️ CI/CD

GitHub Actions-workflows i `workflow/`-mappen kör automatiskt tester vid push/PR.

---

## 🧠 Teknologier

- **Frontend:** React + Vite
- **Backend:** ASP.NET Core
- **Databas:** PostgreSQL
- **Test:** xUnit, Postman, Playwright, SpecFlow
- **CI/CD:** GitHub Actions

---

## 👤 Utvecklare

By **Sebastian Holmberg**  
📧 https://www.linkedin.com/in/sebastian-holmberg-nilsson-02a4161a1/  
🔗 [GitHub.com/SebbeHN](https://github.com/SebbeHN)
