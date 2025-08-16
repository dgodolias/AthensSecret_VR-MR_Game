# Πρόοδος Υλοποίησης: "Το Μυστικό της Αθήνας"

## Μέρος 1: Υλοποίηση Backend (ASP.NET Core Web API)

-   [x] **Βήμα 1: Δημιουργία Project και Ρύθμιση Περιβάλλοντος**
    -   [x] Δημιουργία Web API Project (`dotnet new webapi`)
    -   [x] Εγκατάσταση πακέτων NuGet (EF Core, Npgsql)
-   [x] **Βήμα 2: Σχεδιασμός Βάσης Δεδομένων και Δημιουργία Μοντέλων**
    -   [x] Δημιουργία C# Models (Player, GameSession)
    -   [x] Δημιουργία ApiDbContext
    -   [x] Ρύθμιση Connection String
-   [x] **Βήμα 3: Υλοποίηση API Endpoints (Controllers)**
    -   [x] `GameController` (`/start`, `/state`)
    -   [x] `TrialsController` (`/patience`, `/resource`, `/risk`)
-   [x] **Βήμα 4: Δημιουργία και Εφαρμογή της Βάσης Δεδομένων (Migrations)**
    -   [x] Δημιουργία αρχικής migration
    -   [ ] Εφαρμογή migration στη βάση

## Μέρος 2: Υλοποίηση Dummy Frontend (HTML & JavaScript)

-   [ ] **Βήμα 1: Δημιουργία Βασικής Σελίδας HTML**
-   [ ] **Βήμα 2: Υλοποίηση Λογικής με JavaScript**
    -   [ ] `startGame()`
    -   [ ] `getGameState()`
    -   [ ] Functions για τις δοκιμασίες
-   [ ] **Βήμα 3: CORS Configuration**

## Μέρος 3: Δοκιμές

-   [ ] End-to-End Testing
