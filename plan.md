# Σχέδιο Υλοποίησης: Backend και Dummy Frontend για το "Μυστικό της Αθήνας"

Αυτό το έγγραφο περιγράφει τα βήματα για τη δημιουργία του backend και ενός απλού frontend για το VR παιχνίδι "Το μυστικό της Αθήνας".

**Τεχνολογίες:**
*   **Backend:** ASP.NET Core Web API (C#)
*   **Βάση Δεδομένων:** PostgreSQL
*   **ORM:** Entity Framework Core
*   **Dummy Frontend:** HTML, JavaScript (με χρήση `fetch` API)
*   **Game Client (Τελικός):** Unity

---

## Μέρος 1: Υλοποίηση Backend (ASP.NET Core Web API)

### Βήμα 1: Δημιουργία Project και Ρύθμιση Περιβάλλοντος
1.  **Εγκατάσταση .NET SDK:** Βεβαιωθείτε ότι έχετε εγκαταστήσει την τελευταία έκδοση του .NET SDK.
2.  **Δημιουργία Web API Project:**
    ```bash
    dotnet new webapi -n AthensSecret.Api
    cd AthensSecret.Api
    ```
3.  **Εγκατάσταση Πακέτων NuGet:**
    *   **Entity Framework Core (EF Core):** Για την επικοινωνία με τη βάση δεδομένων.
        ```bash
        dotnet add package Microsoft.EntityFrameworkCore
        dotnet add package Microsoft.EntityFrameworkCore.Design
        ```
    *   **PostgreSQL Provider για EF Core:**
        ```bash
        dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
        ```
    *   **(Προαιρετικά) Swagger/Swashbuckle:** Για τεκμηρίωση και δοκιμή των API endpoints (συνήθως περιλαμβάνεται στα νέα templates).

### Βήμα 2: Σχεδιασμός Βάσης Δεδομένων και Δημιουργία Μοντέλων
1.  **Σχεδιασμός Σχημάτων:** Θα χρειαστούμε πίνακες για:
    *   `Players`: Αποθήκευση της κατάστασης κάθε παίκτη (π.χ., ID, όνομα, wisdom_energy).
    *   `GameSessions`: Παρακολούθηση κάθε playthrough (π.χ., ID, player_id, start_time, end_time, score, current_state).
    *   `Trials`: Στατικός πίνακας που ορίζει τις δοκιμασίες (π.χ., id, name, description).
2.  **Δημιουργία C# Models (Entities):**
    *   `Player.cs`: `int Id`, `string Username`
    *   `GameSession.cs`: `int Id`, `int PlayerId`, `int WisdomEnergy`, `DateTime StartTime`, `string CurrentTrial`
3.  **Δημιουργία DbContext:**
    *   `ApiDbContext.cs`: Κλάση που κληρονομεί από `DbContext` και ορίζει τα `DbSet<T>` για τα παραπάνω μοντέλα.
    *   Ρύθμιση του connection string για την PostgreSQL στο `appsettings.json`.

### Βήμα 3: Υλοποίηση API Endpoints (Controllers)
1.  **`GameController.cs`:**
    *   `POST /api/game/start`: Ξεκινά ένα νέο παιχνίδι για έναν παίκτη. Δημιουργεί μια νέα εγγραφή `GameSession` με αρχικές τιμές (π.χ., 50 wisdom energy) και επιστρέφει το `sessionId`.
    *   `GET /api/game/{sessionId}/state`: Επιστρέφει την τρέχουσα κατάσταση του παιχνιδιού (ενέργεια, τρέχουσα δοκιμασία κ.λπ.).
2.  **`TrialsController.cs`:**
    *   `POST /api/trials/patience`: Ο παίκτης υποβάλλει την επιλογή του για τη "Δοκιμασία Υπομονής". Το backend υπολογίζει το αποτέλεσμα (σωστό/λάθος), ενημερώνει την ενέργεια του παίκτη στη βάση δεδομένων και επιστρέφει το νέο state.
    *   `POST /api/trials/resource`: Ο παίκτης επιλέγει να φυτέψει την ελιά. Το backend χειρίζεται τη λογική (π.χ. άμεσο κέρδος ή επένδυση).
    *   `POST /api/trials/risk`: Ο παίκτης επιλέγει μονοπάτι (ασφαλές/αβέβαιο). Το backend καθορίζει το αποτέλεσμα, ενημερώνει την ενέργεια και επιστρέφει το αποτέλεσμα.

### Βήμα 4: Δημιουργία και Εφαρμογή της Βάσης Δεδομένων (Migrations)
1.  **Δημιουργία Migration:**
    ```bash
    dotnet ef migrations add InitialCreate
    ```
2.  **Εφαρμογή Migration στη Βάση:**
    ```bash
    dotnet ef database update
    ```
    Αυτό θα δημιουργήσει τους πίνακες στην PostgreSQL βάση δεδομένων σας.

---

## Μέρος 2: Υλοποίηση Dummy Frontend (HTML & JavaScript)

### Βήμα 1: Δημιουργία Βασικής Σελίδας HTML
*   Δημιουργήστε ένα αρχείο `index.html` στον φάκελο του project ή σε ξεχωριστό φάκελο `wwwroot`.
*   Προσθέστε βασικά στοιχεία UI:
    *   Ένα κουμπί "Start Game".
    *   Ένα πεδίο για την εμφάνιση του `Session ID`.
    *   Ένα πεδίο για την εμφάνιση της "Wisdom Energy".
    *   Κουμπιά για κάθε επιλογή σε κάθε δοκιμασία (π.χ., "Mirror 1", "Mirror 2", "Safe Path", "Uncertain Path").

### Βήμα 2: Υλοποίηση Λογικής με JavaScript
*   Δημιουργήστε ένα αρχείο `app.js` και συνδέστε το με το `index.html`.
*   **`startGame()` function:**
    *   Καλεί το `POST /api/game/start` endpoint.
    *   Αποθηκεύει το `sessionId` που επιστρέφεται.
    *   Ενημερώνει το UI.
*   **`getGameState()` function:**
    *   Καλεί το `GET /api/game/{sessionId}/state` για να ανανεώνει τις πληροφορίες στο UI.
*   **Functions για τις δοκιμασίες (π.χ., `submitPatienceChoice(choice)`):**
    *   Καλεί το αντίστοιχο `POST` endpoint του `TrialsController`, στέλνοντας την επιλογή του χρήστη.
    *   Μετά την απάντηση, καλεί το `getGameState()` για να ενημερώσει το UI με τη νέα τιμή ενέργειας.
*   **Event Listeners:**
    *   Συνδέστε τις παραπάνω συναρτήσεις με τα `onclick` events των κουμπιών.

### Βήμα 3: CORS Configuration
*   Στο backend (ASP.NET Core), ρυθμίστε το CORS (Cross-Origin Resource Sharing) για να επιτρέψετε στο frontend (που τρέχει σε διαφορετικό origin) να επικοινωνεί με το API σας. Αυτό γίνεται συνήθως στο `Program.cs` ή `Startup.cs`.

---

## Μέρος 3: Δοκιμές και Επόμενα Βήματα

1.  **Εκκίνηση Backend:** Τρέξτε την εντολή `dotnet run` από τον φάκελο του API project.
2.  **Άνοιγμα Frontend:** Ανοίξτε το `index.html` σε έναν browser.
3.  **End-to-End Testing:**
    *   Πατήστε "Start Game".
    *   Ελέγξτε αν το UI ενημερώνεται με το session ID και την αρχική ενέργεια.
    *   Παίξτε τις δοκιμασίες και παρακολουθήστε τις αλλαγές στην ενέργεια.
    *   Ελέγξτε την κονσόλα του browser και το output του backend για τυχόν σφάλματα.
4.  **Επόμενα Βήματα:**
    *   Υλοποίηση της λογικής για τις παθητικές ανταμοιβές (π.χ., αύξηση ενέργειας ανά λεπτό).
    *   Προσθήκη authentication/authorization αν απαιτείται.
    *   Έναρξη της ενοποίησης με το Unity client, αντικαθιστώντας τις κλήσεις του dummy frontend με κλήσεις από C# scripts στη Unity (π.χ., με τη χρήση `UnityWebRequest`).
