-- ====================================================================================
-- 1. PLAYER TABLE
-- ====================================================================================
CREATE TABLE PLAYER (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    age INTEGER NOT NULL
);

-- ====================================================================================
-- 2. RESPONSES TABLE (One-to-One with PLAYER)
-- ====================================================================================
CREATE TABLE RESPONSES (
    player_id INTEGER PRIMARY KEY,
    Q1 INTEGER NOT NULL,
    Q2 INTEGER NOT NULL,
    Q3 INTEGER NOT NULL,
    
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

-- ====================================================================================
-- 8. RESPONSES_STATISTICS TABLE (Age-based patience and risk values)
-- ====================================================================================
CREATE TABLE RESPONSES_STATISTICS (
    age INTEGER PRIMARY KEY,
    patience INTEGER NOT NULL,
    risk INTEGER NOT NULL
);

-- Insert age-based statistics data
INSERT INTO RESPONSES_STATISTICS (age, patience, risk) VALUES
(18, 6, 6), (19, 6, 6), (20, 7, 7), (21, 7, 7), (22, 8, 8), (23, 8, 8), (24, 9, 9), (25, 9, 9),
(26, 6, 6), (27, 7, 7), (28, 8, 8), (29, 6, 6), (30, 5, 5), (31, 5, 5), (32, 7, 7), (33, 8, 8),
(34, 5, 5), (35, 4, 4), (36, 8, 8), (37, 6, 6), (38, 6, 6), (39, 7, 7), (40, 7, 7), (41, 8, 8),
(42, 8, 8), (43, 9, 9), (44, 9, 9), (45, 6, 6), (46, 7, 7), (47, 8, 8), (48, 6, 6), (49, 6, 6),
(50, 7, 7), (51, 7, 7), (52, 8, 8), (53, 8, 8), (54, 9, 9), (55, 9, 9), (56, 6, 6), (57, 7, 7),
(58, 8, 8), (59, 6, 6), (60, 6, 6), (61, 7, 7), (62, 7, 7), (63, 8, 8), (64, 8, 8), (65, 9, 9),
(66, 9, 9), (67, 6, 6), (68, 7, 7), (69, 8, 8), (70, 6, 6), (71, 6, 6), (72, 7, 7), (73, 7, 7),
(74, 8, 8), (75, 8, 8), (76, 9, 9), (77, 9, 9), (78, 6, 6), (79, 7, 7), (80, 8, 8);

--helpful commands--

--delete all data in tables--
DELETE FROM RESPONSES;
DELETE FROM PATH_TRIAL;
DELETE FROM OIL_TREE_TRIAL;
DELETE FROM MIRRORS_TRIAL;
DELETE FROM GAME_SESSIONS;
DELETE FROM PLAYER;
DELETE FROM GAME_CONFIGURATION;

