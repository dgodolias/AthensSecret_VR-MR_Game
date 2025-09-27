# Database Relationships

```mermaid
erDiagram
  PLAYER {
    int id PK
    string first_name
    string last_name
    string email
    int age
  }

  RESPONSES {
    int player_id PK, FK
    int Q1
    int Q2
    int Q3
  }

  GAME_SESSIONS {
    int id PK
    int player_id FK
    datetime started_at
    datetime ended_at
  }

  MIRRORS_TRIAL {
    int game_session_id PK, FK
    datetime start_time
    datetime end_time
    int total_gained_wisdom
  }

  OIL_TREE_TRIAL {
    int game_session_id PK, FK
    datetime start_time
    datetime end_time
    int total_gained_wisdom
    datetime investment_start_time "nullable"
  }

  PATH_TRIAL {
    int game_session_id PK, FK
    datetime start_time
    datetime end_time
    int total_gained_wisdom
    boolean safe_path
  }

  GAME_CONFIGURATION {
    int id PK
    int StartingWisdom
    int MirrorWisdomIfWaits
    int MirrorWisdomIfRisksCorrectly
    int MirrorWisdomIfRisksFalsely
    int OilTreeWisdomNotInvestment
    string OilTreeWisdomInvestmentFunction
    int SafePathWisdom
    int UncertainPathWisdom
    int UncertainPathWisdomSmallPlank
    int UncertainPathWisdomMediumPlank
    int UncertainPathWisdomBigPlank
  }

  PLAYER ||--|| RESPONSES : "fills"
  PLAYER ||--o{ GAME_SESSIONS : "plays"
  GAME_SESSIONS ||--|| MIRRORS_TRIAL : "may have (0..1)"
  GAME_SESSIONS ||--|| OIL_TREE_TRIAL : "may have (0..1)"
  GAME_SESSIONS ||--|| PATH_TRIAL : "may have (0..1)"
```

## Περιγραφές Σχέσεων
- PLAYER → RESPONSES: One-to-One. Κάθε παίκτης έχει ακριβώς μία εγγραφή στο `RESPONSES`. Το `player_id` είναι PK και FK προς `PLAYER.id`.
- PLAYER → GAME_SESSIONS: One-to-Many. Ένας παίκτης μπορεί να έχει πολλές συνεδρίες.
- GAME_SESSIONS → Trials (MIRRORS/OIL_TREE/PATH): Each GAME_SESSION μπορεί να έχει το πολύ ένα trial ανά τύπο (0..1). Αυτό υλοποιείται με `*_TRIAL.game_session_id` ως PK+FK προς `GAME_SESSIONS.id`.
- `OIL_TREE_TRIAL.investment_start_time`: nullable. Αν είναι NULL → άμεση συγκομιδή. Χρόνος αναμονής = `end_time - investment_start_time` (όταν δεν είναι NULL).
- `PATH_TRIAL.safe_path`: boolean που δηλώνει αν επιλέχθηκε ο ασφαλής δρόμος.

## Περίληψη Σχέδων (Table Schemas)
- PLAYER
  - PK: `id`
  - Fields: `first_name`, `last_name`, `email`
- RESPONSES
  - PK/FK: `player_id` → `PLAYER.id`
  - Fields: `Q1`, `Q2`, `Q3`
- GAME_SESSIONS
  - PK: `id`
  - FK: `player_id` → `PLAYER.id`
  - Fields: `started_at`, `ended_at`
- MIRRORS_TRIAL
  - PK/FK: `game_session_id` → `GAME_SESSIONS.id` (0..1)
  - Fields: `start_time`, `end_time`, `total_gained_wisdom`
- OIL_TREE_TRIAL
  - PK/FK: `game_session_id` → `GAME_SESSIONS.id` (0..1)
  - Fields: `start_time`, `end_time`, `total_gained_wisdom`, `investment_start_time (nullable)`
- PATH_TRIAL
  - PK/FK: `game_session_id` → `GAME_SESSIONS.id` (0..1)
  - Fields: `start_time`, `end_time`, `total_gained_wisdom`, `safe_path (boolean)`
- GAME_CONFIGURATION
  - PK: `id`
  - Fields: tuning params (see schema above). Συνιστάται versioning και `is_active` flag.

## Χρήση Πινάκων (σύντομη)
- PLAYER
  - Webform: create/update παίκτη.
  - In-Game: ελεγχος υπαρξης του δωσμενου id
  - Admin: -
- RESPONSES
  - Webform: αποθήκευση Q1–Q3
  - Admin: -
  - In-Game: -
- GAME_SESSIONS
  - In-Game: INSERT `started_at` στην έναρξη, UPDATE `ended_at` στο τέλος.
  - Admin: -
  - Webform: -
- MIRRORS_TRIAL / OIL_TREE_TRIAL / PATH_TRIAL
  - In-Game: INSERT row per trial type only when that trial occurs (game_session_id = session.id). UPDATE with `end_time`, `total_gained_wisdom`, (για OIL_TREE: `investment_start_time` όταν επιλέγεται).
  - Admin: -
  - Webform: -
- GAME_CONFIGURATION
  - Admin: σεταρισμα παρααμετρων.
  - In-Game: read-only ανά session boot - χρησιμοποιείται για υπολογισμό ανταμοιβών.

