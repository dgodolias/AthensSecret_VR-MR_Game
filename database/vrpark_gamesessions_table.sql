-- VR Park Game Sessions Table
-- Tracks game sessions for VR Park users with eye tracking data

CREATE TABLE IF NOT EXISTS vrpark_gamesessions (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    eyetracking_sequence TEXT,
    started_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP,
    
    -- Foreign key constraint to vrpark_users
    CONSTRAINT fk_vrpark_user
        FOREIGN KEY (user_id)
        REFERENCES vrpark_users(id)
        ON DELETE CASCADE
);

-- Create index on user_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_vrpark_gamesessions_user_id ON vrpark_gamesessions(user_id);

-- Create index on started_at for sorting/filtering by date
CREATE INDEX IF NOT EXISTS idx_vrpark_gamesessions_started_at ON vrpark_gamesessions(started_at DESC);

-- Create index on ended_at for filtering active/completed sessions
CREATE INDEX IF NOT EXISTS idx_vrpark_gamesessions_ended_at ON vrpark_gamesessions(ended_at);

-- Add comment to table
COMMENT ON TABLE vrpark_gamesessions IS 'Game sessions for VR Park with eye tracking data. Supports concurrent sessions from multiple users.';
COMMENT ON COLUMN vrpark_gamesessions.id IS 'Auto-incrementing session ID';
COMMENT ON COLUMN vrpark_gamesessions.user_id IS 'Reference to vrpark_users table';
COMMENT ON COLUMN vrpark_gamesessions.eyetracking_sequence IS 'Eye tracking data sequence, populated when session ends';
COMMENT ON COLUMN vrpark_gamesessions.started_at IS 'Session start timestamp, auto-set on creation';
COMMENT ON COLUMN vrpark_gamesessions.ended_at IS 'Session end timestamp, null for active sessions';
