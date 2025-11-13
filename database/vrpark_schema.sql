-- VR Park Users Table

CREATE TABLE IF NOT EXISTS vrpark_users (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255),
    age INTEGER NOT NULL CHECK (age >= 1 AND age <= 120),
    video INTEGER NOT NULL DEFAULT 1 CHECK (video >= 1 AND video <= 4),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_vrpark_users_email ON vrpark_users(email);
CREATE INDEX IF NOT EXISTS idx_vrpark_users_created_at ON vrpark_users(created_at DESC);
COMMENT ON TABLE vrpark_users IS 'User registrations for VR Park service (separate from Athens Secret game)';
COMMENT ON COLUMN vrpark_users.video IS 'Random video ID (1-4) assigned to user during registration';

-- VR Park Game Sessions Table

CREATE TABLE IF NOT EXISTS vrpark_gamesessions (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    eyetracking_sequence TEXT,
    started_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP,

    CONSTRAINT fk_vrpark_user
        FOREIGN KEY (user_id)
        REFERENCES vrpark_users(id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_vrpark_gamesessions_user_id ON vrpark_gamesessions(user_id);
CREATE INDEX IF NOT EXISTS idx_vrpark_gamesessions_started_at ON vrpark_gamesessions(started_at DESC);
CREATE INDEX IF NOT EXISTS idx_vrpark_gamesessions_ended_at ON vrpark_gamesessions(ended_at);
COMMENT ON TABLE vrpark_gamesessions IS 'Game sessions for VR Park with eye tracking data. Supports concurrent sessions from multiple users.';
COMMENT ON COLUMN vrpark_gamesessions.id IS 'Auto-incrementing session ID';
COMMENT ON COLUMN vrpark_gamesessions.user_id IS 'Reference to vrpark_users table';
COMMENT ON COLUMN vrpark_gamesessions.eyetracking_sequence IS 'Eye tracking data sequence, populated when session ends';
COMMENT ON COLUMN vrpark_gamesessions.started_at IS 'Session start timestamp, auto-set on creation';
COMMENT ON COLUMN vrpark_gamesessions.ended_at IS 'Session end timestamp, null for active sessions';

-- commands to delete all data from the tables
TRUNCATE TABLE vrpark_gamesessions RESTART IDENTITY CASCADE;
TRUNCATE TABLE vrpark_users RESTART IDENTITY CASCADE;




