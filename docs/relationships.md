## Entity Relationship Diagram

```mermaid
erDiagram
    PLAYER {
      int id PK
      string first_name
      string last_name
      string email
    }

    RESPONSES {
      int player_id PK, FK
      string Q1
      string Q2
      string Q3
    }

    GAME_SESSIONS {
      int id PK
      int player_id FK
      datetime started_at
      datetime ended_at
    }

    MIRRORS_TRIAL {
      int id PK
      int game_session_id FK
      datetime start_time
      datetime end_time
      int total_gained_wisdom
    }

    OIL_TREE_TRIAL {
      int id PK
      int game_session_id FK
      datetime start_time
      datetime end_time
      int total_gained_wisdom
      datetime investment_start_time  "nullable"
    }

    PATH_TRIAL {
      int id PK
      int game_session_id FK
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

    %% Relationships
    PLAYER ||--|| RESPONSES : "fills"
    PLAYER ||--o{ GAME_SESSIONS : "plays"
    GAME_SESSIONS ||--o{ MIRRORS_TRIAL : "has"
    GAME_SESSIONS ||--o{ OIL_TREE_TRIAL : "has"
    GAME_SESSIONS ||--o{ PATH_TRIAL : "has"
```

### Relationship notes

- PLAYER ↔ RESPONSES: 1–1, το `player_id` είναι PK & FK (όπως πριν).
- PLAYER ↔ GAME_SESSIONS: 1–N (ένας παίκτης παίζει πολλές συνεδρίες).
- GAME_SESSIONS ↔ Trials: 1–N (κάθε συνεδρία έχει πολλά trials από κάθε τύπο).
- `OIL_TREE_TRIAL.investment_start_time`: επιτρέπεται NULL.
  - Αν μάζεψε αμέσως → NULL.
  - Αν επέλεξε Investment → set όταν πάτησε το κουμπί.
  - Χρόνος αναμονής = end_time - investment_start_time.
- `PATH_TRIAL.safe_path`: boolean (αν ακολούθησε τον ασφαλή δρόμο).