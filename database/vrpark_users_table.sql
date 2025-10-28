-- VR Park Users Table
-- Separate table for VR Park signups (different from Athens Secret VR game)

CREATE TABLE IF NOT EXISTS vrpark_users (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255),
    age INTEGER NOT NULL CHECK (age >= 1 AND age <= 120),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Optional: Create index on email for faster lookups
CREATE INDEX IF NOT EXISTS idx_vrpark_users_email ON vrpark_users(email);

-- Optional: Create index on created_at for sorting
CREATE INDEX IF NOT EXISTS idx_vrpark_users_created_at ON vrpark_users(created_at DESC);

-- Add comment to table
COMMENT ON TABLE vrpark_users IS 'User registrations for VR Park service (separate from Athens Secret game)';
