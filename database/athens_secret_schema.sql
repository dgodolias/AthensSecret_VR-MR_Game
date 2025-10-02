-- ====================================================================================
-- 1. PLAYER TABLE
-- ====================================================================================
CREATE TABLE PLAYER (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE,  -- Email is now optional
    age INTEGER NOT NULL
);

-- ====================================================================================
-- 2. RESPONSES TABLE (One-to-One with PLAYER)
-- Q1 = Patience, Q2 = Risk (mapped to ResponsesStatistics table)
-- ====================================================================================
CREATE TABLE RESPONSES (
    player_id INTEGER PRIMARY KEY,
    Q1 INTEGER NOT NULL,    -- Patience (1-10)
    Q2 INTEGER NOT NULL,    -- Risk tolerance (1-10)
    
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
    OliveTreeWisdomNotInvestment INTEGER NOT NULL,
    OliveTreeWisdomInvestmentFunction VARCHAR(20) NOT NULL,
    SafePathWisdom INTEGER NOT NULL,
    UncertainPathWisdom INTEGER NOT NULL,
    UncertainPathWisdomSmallPlank INTEGER NOT NULL,
    UncertainPathWisdomMediumPlank INTEGER NOT NULL,
    UncertainPathWisdomBigPlank INTEGER NOT NULL,
    UnlockWisdomHiddenRoom INTEGER NOT NULL
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
-- 6. OLIVE_TREE_TRIAL TABLE (Exactly as per relationships.md)
-- ====================================================================================
CREATE TABLE OLIVE_TREE_TRIAL (
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
    MirrorWisdomIfRisksFalsely, OliveTreeWisdomNotInvestment, OliveTreeWisdomInvestmentFunction,
    SafePathWisdom, UncertainPathWisdom, UncertainPathWisdomSmallPlank,
    UncertainPathWisdomMediumPlank, UncertainPathWisdomBigPlank, UnlockWisdomHiddenRoom
) VALUES (
    50, 50, 75,
    25, 50, 'sqrt',
    10, 25, 15,
    25, 40, 100
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
DELETE FROM OLIVE_TREE_TRIAL;
DELETE FROM MIRRORS_TRIAL;
DELETE FROM GAME_SESSIONS;
DELETE FROM PLAYER;
DELETE FROM GAME_CONFIGURATION;
DELETE FROM RESPONSES_STATISTICS;

--delete all data from temp tables--
DELETE FROM public.game_sessions;
DELETE FROM public.mirrors_trial;
delete FROM public.olive_tree_trial;
delete FROM public.path_trial;
delete FROM public.player;
delete FROM public.responses;

-- Complete game session details with all related tables
SELECT 
    -- Game Session Info
    gs.id AS session_id,
    gs.started_at,
    gs.ended_at,
    
    -- Player Info
    p.id AS player_id,
    p.first_name,
    p.last_name,
    p.email,
    p.age,
    
    -- Player Responses
    r.q1 AS patience_response,
    r.q2 AS risk_response,
    
    -- Mirrors Trial Info
    mt.start_time AS mirrors_start,
    mt.end_time AS mirrors_end,
    mt.total_gained_wisdom AS mirrors_wisdom,
    
    -- Olive Tree Trial Info
    ot.start_time AS olive_start,
    ot.end_time AS olive_end,
    ot.total_gained_wisdom AS olive_wisdom,
    ot.investment_start_time AS olive_investment_time,
    
    -- Path Trial Info
    pt.start_time AS path_start,
    pt.end_time AS path_end,
    pt.total_gained_wisdom AS path_wisdom,
    pt.safe_path AS path_is_safe

FROM game_sessions gs
INNER JOIN player p ON gs.player_id = p.id
LEFT JOIN responses r ON r.player_id = p.id
LEFT JOIN mirrors_trial mt ON mt.game_session_id = gs.id
LEFT JOIN olive_tree_trial ot ON ot.game_session_id = gs.id
LEFT JOIN path_trial pt ON pt.game_session_id = gs.id

WHERE gs.id = 52;

