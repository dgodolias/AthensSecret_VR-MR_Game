# VR Park - Frontend Integration Guide

## Overview
VR Park signup system is hosted at: **https://athens-secret-api.onrender.com/vrpark/signup/**

The frontend makes API calls to the same server for user registration and verification.

---

## API Endpoints

### Base URL
```
https://athens-secret-api.onrender.com
```

---

### 1. User Signup (Registration)

**Endpoint:** `POST /api/vrpark/signup`

**Request Body:**
```json
{
  "firstName": "Γιάννης",
  "lastName": "Παπαδόπουλος",
  "email": "user@example.com",
  "age": 25
}
```

**Notes:**
- `firstName` and `lastName` are **required**
- `email` is **optional** (can be null or empty string)
- `age` must be between 1 and 120

**Success Response (200):**
```json
{
  "userId": 7,
  "firstName": "Γιάννης",
  "lastName": "Παπαδόπουλος",
  "email": "user@example.com",
  "age": 25,
  "video": 3,
  "message": "Η εγγραφή σας ολοκληρώθηκε επιτυχώς!"
}
```

**Notes:**
- `video` is a random integer between 1-4, assigned automatically during signup
- This video ID determines which video the user will watch

**Error Response (400):**
```json
{
  "message": "Το email υπάρχει ήδη"
}
```

---

### 2. User Verification

**Endpoint:** `GET /api/vrpark/verify/{userId}`

**Example:** `GET /api/vrpark/verify/7`

**Success Response (200):**
```json
{
  "userId": 7,
  "firstName": "Γιάννης",
  "lastName": "Παπαδόπουλος",
  "email": "user@example.com",
  "age": 25,
  "video": 3,
  "createdAt": "2025-10-28T10:30:00Z",
  "message": "Καλώς ήρθες, Γιάννης!"
}
```

**Notes:**
- `video` field indicates which video (1-4) was assigned to this user during signup

**Error Response (404):**
```json
{
  "message": "Ο χρήστης δεν βρέθηκε"
}
```

---

## Frontend Implementation

### Signup Example (JavaScript)
```javascript
async function signup(firstName, lastName, email, age) {
  const response = await fetch('https://athens-secret-api.onrender.com/api/vrpark/signup', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ firstName, lastName, email, age })
  });
  
  return await response.json();
}
```

### Verify Example (JavaScript)
```javascript
async function verifyUser(userId) {
  const response = await fetch(`https://athens-secret-api.onrender.com/api/vrpark/verify/${userId}`);
  
  if (response.ok) {
    return await response.json();
  } else {
    throw new Error('User not found');
  }
}
```

---

## Important Notes

- **Rate Limiting:** API has rate limiting enabled on signup and verify endpoints
- **CORS:** Server accepts requests from any origin
- **Email Uniqueness:** If email is provided, it must be unique (duplicate emails return 400 error)
- **User ID:** After successful signup, save the `userId` - you'll need it for verification

---

## Testing

### Test Signup (PowerShell)
```powershell
$body = @{
    firstName = "Test"
    lastName = "User"
    email = "test@example.com"
    age = 25
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://athens-secret-api.onrender.com/api/vrpark/signup" -Method POST -Body $body -ContentType "application/json"
```

### Test Verify (PowerShell)
```powershell
Invoke-WebRequest -Uri "https://athens-secret-api.onrender.com/api/vrpark/verify/1" -Method GET
```

---

## Reference Implementation

See the existing signup page for a complete example:
**https://athens-secret-api.onrender.com/vrpark/signup/**
