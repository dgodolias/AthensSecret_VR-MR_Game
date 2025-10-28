# VR Park Game Sessions API - Frontend Guide

## Overview
VR Park game sessions API allows tracking of gameplay sessions with eye tracking data. Supports concurrent sessions from multiple users.

---

## API Endpoints

### Base URL
```
https://athens-secret-api.onrender.com
```

---

## Game Session Endpoints

### 1. Start Game Session

**Endpoint:** `POST /api/vrpark/session/start`

**Description:** Creates a new game session when user starts playing.

**Request Body:**
```json
{
  "userId": 7
}
```

**Success Response (200):**
```json
{
  "sessionId": 42,
  "userId": 7,
  "startedAt": "2025-10-28T10:30:00Z",
  "message": "Το session ξεκίνησε επιτυχώς!"
}
```

**Error Response (404 - User not found):**
```json
{
  "message": "Ο χρήστης δεν βρέθηκε"
}
```

**Important:**
- Save the `sessionId` returned - you'll need it to end the session
- User must exist (must have signed up first via `/api/vrpark/signup`)

---

### 2. End Game Session

**Endpoint:** `POST /api/vrpark/session/end`

**Description:** Ends a game session and saves eye tracking data.

**Request Body:**
```json
{
  "sessionId": 42,
  "userId": 7,
  "eyetrackingSequence": "0.5,0.3,0.7,0.2,..."
}
```

**Notes:**
- `sessionId` and `userId` are **required**
- `eyetrackingSequence` is **optional** (can be null or empty string)

**Success Response (200):**
```json
{
  "sessionId": 42,
  "userId": 7,
  "eyetrackingSequence": "0.5,0.3,0.7,0.2,...",
  "startedAt": "2025-10-28T10:30:00Z",
  "endedAt": "2025-10-28T10:45:00Z",
  "duration": "00:15:00",
  "message": "Το session ολοκληρώθηκε επιτυχώς!"
}
```

**Error Response (404 - Session not found):**
```json
{
  "message": "Το session δεν βρέθηκε ή δεν ανήκει στον χρήστη"
}
```

**Error Response (400 - Already ended):**
```json
{
  "message": "Το session έχει ήδη ολοκληρωθεί"
}
```

---

## Concurrency Support

The API handles multiple concurrent sessions:

✅ **Supported Flow:**
```
User A: Start Session → SessionId = 1
User B: Start Session → SessionId = 2
User C: Start Session → SessionId = 3
User A: End Session (SessionId = 1)
User C: End Session (SessionId = 3)
User B: End Session (SessionId = 2)
```

- Each session has a **unique auto-incrementing ID**
- Sessions are independent and can be started/ended in any order
- User verification ensures sessions belong to the correct user

---

## Frontend Implementation

### Complete Game Flow Example (JavaScript)

```javascript
// Step 1: User signs up (if not already)
async function signup(firstName, lastName, email, age) {
  const response = await fetch('https://athens-secret-api.onrender.com/api/vrpark/signup', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ firstName, lastName, email, age })
  });
  
  const data = await response.json();
  return data.userId; // Save this!
}

// Step 2: Start game session
async function startGameSession(userId) {
  const response = await fetch('https://athens-secret-api.onrender.com/api/vrpark/session/start', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userId })
  });
  
  const data = await response.json();
  return data.sessionId; // Save this for ending the session!
}

// Step 3: End game session (when game finishes)
async function endGameSession(sessionId, userId, eyetrackingData) {
  const response = await fetch('https://athens-secret-api.onrender.com/api/vrpark/session/end', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ 
      sessionId, 
      userId, 
      eyetrackingSequence: eyetrackingData 
    })
  });
  
  const data = await response.json();
  console.log(`Session lasted: ${data.duration}`);
  return data;
}
```

### Complete Usage Flow

```javascript
// 1. User signs up (or uses existing userId)
const userId = await signup("Γιάννης", "Παπαδόπουλος", "test@example.com", 25);
console.log(`User created with ID: ${userId}`);

// 2. Start game session
const sessionId = await startGameSession(userId);
console.log(`Game session started with ID: ${sessionId}`);

// 3. User plays the game...
// Collect eye tracking data during gameplay
let eyeTrackingData = "";

// 4. When game ends, send the data
const result = await endGameSession(sessionId, userId, eyeTrackingData);
console.log(`Session completed! Duration: ${result.duration}`);
```

---

## Data Storage

**Database Table:** `vrpark_gamesessions`

| Column | Type | Description |
|--------|------|-------------|
| id | INTEGER | Auto-increment session ID (Primary Key) |
| user_id | INTEGER | Foreign key to vrpark_users |
| eyetracking_sequence | TEXT | Eye tracking data (nullable) |
| started_at | TIMESTAMP | Session start time |
| ended_at | TIMESTAMP | Session end time (null for active sessions) |

---

## Testing with PowerShell

### Start Session
```powershell
$body = @{
    userId = 7
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://athens-secret-api.onrender.com/api/vrpark/session/start" -Method POST -Body $body -ContentType "application/json"
```

### End Session
```powershell
$body = @{
    sessionId = 42
    userId = 7
    eyetrackingSequence = "0.5,0.3,0.7,0.2,0.8"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://athens-secret-api.onrender.com/api/vrpark/session/end" -Method POST -Body $body -ContentType "application/json"
```

---

## Important Notes

✅ **Session ID Management:** Always save the `sessionId` returned from start endpoint  
✅ **User Validation:** UserId must exist in database before starting session  
✅ **Concurrency:** Multiple users can have active sessions simultaneously  
✅ **Idempotency:** Cannot end the same session twice (returns 400 error)  
✅ **Data Integrity:** Foreign key constraint ensures user exists  
✅ **Rate Limiting:** API has rate limiting on all endpoints  

---

## Error Handling Best Practices

```javascript
async function safeStartSession(userId) {
  try {
    const response = await fetch('https://athens-secret-api.onrender.com/api/vrpark/session/start', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ userId })
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Failed to start session:', error);
    // Handle error (show message to user, retry, etc.)
    throw error;
  }
}
```

---

## Complete API Reference

All VR Park endpoints:

1. `POST /api/vrpark/signup` - User registration
2. `GET /api/vrpark/verify/{userId}` - User verification
3. `POST /api/vrpark/session/start` - Start game session
4. `POST /api/vrpark/session/end` - End game session
