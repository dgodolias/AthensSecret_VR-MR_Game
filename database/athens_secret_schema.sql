-- ====================================================================================
-- 1. PLAYER TABLE
-- ====================================================================================
CREATE TABLE PLAYER (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL
);

-- ====================================================================================
-- 2. RESPONSES TABLE (One-to-One with PLAYER)
-- ====================================================================================
CREATE TABLE RESPONSES (
    player_id INTEGER PRIMARY KEY,
    Q1 TEXT NOT NULL,
    Q2 TEXT NOT NULL,
    Q3 TEXT NOT NULL,
    
    FOREIGN KEY (player_id) REFERENCES PLAYER(id) ON DELETE CASCADE
);

-- ====================================================================================
-- 3. GAME_CONFIGURATION TABLE (Admin Configurable Settings - STANDALONE)
-- ====================================================================================
CREATE TABLE GAME_CONFIGURATION (
    id SERIAL PRIMARY KEY,
    
    -- Core wisdom values exactly as per relationships.md
    StartingWisdom INTEGER NOT NULL,
    MirrorWisdomIfWaits INTEGER NOT NULL,
    MirrorWisdomIfRisksCorrectly INTEGER NOT NULL,
    MirrorWisdomIfRisksFalsely INTEGER NOT NULL,
    OilTreeWisdomNotInvestment INTEGER NOT NULL,
    OilTreeWisdomInvestmentFunction VARCHAR(20) NOT NULL,
    SafePathWisdom INTEGER NOT NULL,
    UncertainPathWisdom INTEGER NOT NULL,
    UncertainPathWisdomSmallPlank INTEGER NOT NULL,
    UncertainPathWisdomMediumPlank INTEGER NOT NULL,
    UncertainPathWisdomBigPlank INTEGER NOT NULL
);

-- ====================================================================================
-- 4. GAME_SESSIONS TABLE (Exactly as per relationships.md)
-- ====================================================================================
CREATE TABLE GAME_SESSIONS (
    id SERIAL PRIMARY KEY,
    player_id INTEGER NOT NULL,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP NULL,
    
    FOREIGN KEY (player_id) REFERENCES PLAYER(id) ON DELETE CASCADE
);

-- ====================================================================================
-- 5. MIRRORS_TRIAL TABLE (Exactly as per relationships.md)
-- ====================================================================================
CREATE TABLE MIRRORS_TRIAL (
    game_session_id INTEGER PRIMARY KEY,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP NULL,
    total_gained_wisdom INTEGER NOT NULL,
    
    FOREIGN KEY (game_session_id) REFERENCES GAME_SESSIONS(id) ON DELETE CASCADE
);

-- ====================================================================================
-- 6. OIL_TREE_TRIAL TABLE (Exactly as per relationships.md)
-- ====================================================================================
CREATE TABLE OIL_TREE_TRIAL (
    game_session_id INTEGER PRIMARY KEY,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP NULL,
    total_gained_wisdom INTEGER NOT NULL,
    investment_start_time TIMESTAMP NULL, -- NULL = immediate harvest, NOT NULL = invested
    
    FOREIGN KEY (game_session_id) REFERENCES GAME_SESSIONS(id) ON DELETE CASCADE
);

-- ====================================================================================
-- 7. PATH_TRIAL TABLE (Exactly as per relationships.md)
-- ====================================================================================
CREATE TABLE PATH_TRIAL (
    game_session_id INTEGER PRIMARY KEY,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP NULL,
    total_gained_wisdom INTEGER NOT NULL,
    safe_path BOOLEAN NOT NULL, -- TRUE = safe path, FALSE = uncertain path
    
    FOREIGN KEY (game_session_id) REFERENCES GAME_SESSIONS(id) ON DELETE CASCADE
);

-- ====================================================================================
-- INDEXES FOR PERFORMANCE
-- ====================================================================================
CREATE INDEX idx_game_sessions_player_id ON GAME_SESSIONS(player_id);
CREATE INDEX idx_game_sessions_started_at ON GAME_SESSIONS(started_at);
CREATE INDEX idx_player_email ON PLAYER(email);

-- default data
INSERT INTO GAME_CONFIGURATION (
    StartingWisdom, MirrorWisdomIfWaits, MirrorWisdomIfRisksCorrectly,
    MirrorWisdomIfRisksFalsely, OilTreeWisdomNotInvestment, OilTreeWisdomInvestmentFunction,
    SafePathWisdom, UncertainPathWisdom, UncertainPathWisdomSmallPlank,
    UncertainPathWisdomMediumPlank, UncertainPathWisdomBigPlank
) VALUES (
    50, 50, 75,
    25, 50, 'sqrt',
    10, 25, 15,
    25, 40
);


