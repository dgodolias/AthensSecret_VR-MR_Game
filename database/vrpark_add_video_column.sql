-- Add video column to vrpark_users table
-- This column stores a random video ID (1-4) assigned during user creation

ALTER TABLE vrpark_users 
ADD COLUMN IF NOT EXISTS video INTEGER NOT NULL DEFAULT 1 
CHECK (video >= 1 AND video <= 4);

-- Add comment
COMMENT ON COLUMN vrpark_users.video IS 'Random video ID (1-4) assigned to user during registration';

-- Update existing records with random video values (1-4)
UPDATE vrpark_users 
SET video = floor(random() * 4 + 1)::INTEGER 
WHERE video IS NULL OR video = 1;

-- Verify the update
SELECT id, first_name, last_name, video FROM vrpark_users ORDER BY id;
