# VR Park Database Viewer

## Επισκόπηση
Το VR Park Database Viewer είναι μια ασφαλής διεπαφή για την προβολή των δεδομένων χρηστών και game sessions του VR Park.

## Πρόσβαση

### URLs
- **Local**: `http://localhost:5182/vrpark/database/`
- **Render**: `https://athens-secret-api.onrender.com/vrpark/database/`

Η σελίδα ανιχνεύει αυτόματα το περιβάλλον και χρησιμοποιεί το σωστό API endpoint.

### Έλεγχος Ταυτότητας
Η σελίδα χρησιμοποιεί **την ίδια ακριβώς λογική** με το `/admin`:
- Admin key που αποθηκεύεται στο localStorage ως `vrparkAdminKey`
- Μόνο ASCII χαρακτήρες (αγγλικά γράμματα, αριθμοί, `_`, `-`)
- Το admin key ελέγχεται από τη μεταβλητή περιβάλλοντος `ADMIN_KEY` (default: `admin123`)

## Χαρακτηριστικά

### 1. VR Park Users Table
Εμφανίζει όλους τους εγγεγραμμένους χρήστες με:
- User ID
- First Name
- Last Name
- Email
- Age
- Video (1-4)
- Created At (ημερομηνία/ώρα εγγραφής)

### 2. VR Park Game Sessions Table
Εμφανίζει όλα τα game sessions με:
- Session ID
- User ID
- Started At (ημερομηνία/ώρα έναρξης)
- Ended At (ημερομηνία/ώρα λήξης)
- Duration (διάρκεια σε h/m/s)
- Eye Tracking (μήκος χαρακτήρων του eyetracking sequence)

### Λειτουργίες Πίνακα
Κάθε πίνακας διαθέτει:
- **Refresh**: Ανανέωση δεδομένων από τη βάση
- **Download CSV**: Εξαγωγή δεδομένων σε CSV αρχείο με UTF-8 encoding (υποστήριξη ελληνικών)

### Fixed Height & Scrolling
- Μέγιστο ύψος πίνακα: 500px
- Αυτόματο scrolling όταν τα δεδομένα ξεπερνούν το ύψος
- Sticky header που παραμένει ορατό κατά το scrolling
- Custom scrollbar styling

## API Endpoints

### GET /api/vrpark/database/users
Επιστρέφει όλους τους VR Park users.

**Headers:**
```
X-Admin-Key: <your-admin-key>
```

**Response:**
```json
[
  {
    "UserId": 1,
    "FirstName": "Όνομα",
    "LastName": "Επίθετο",
    "Email": "email@example.com",
    "Age": 25,
    "Video": 2,
    "CreatedAt": "2025-11-13T10:30:00Z"
  }
]
```

### GET /api/vrpark/database/sessions
Επιστρέφει όλα τα VR Park game sessions.

**Headers:**
```
X-Admin-Key: <your-admin-key>
```

**Response:**
```json
[
  {
    "SessionId": 1,
    "UserId": 1,
    "StartedAt": "2025-11-13T10:35:00Z",
    "EndedAt": "2025-11-13T10:45:00Z",
    "EyetrackingSequence": "LRLRLRLR..."
  }
]
```

## Ασφάλεια

### Admin Key Validation
- Το admin key ελέγχεται στο backend μέσω του header `X-Admin-Key`
- Η τιμή συγκρίνεται με τη μεταβλητή περιβάλλοντος `ADMIN_KEY`
- Default τιμή: `admin123` (πρέπει να αλλάξει σε production!)

### Error Handling
- **401 Unauthorized**: Λείπει το admin key
- **403 Forbidden**: Λάθος admin key
- Αυτόματη αποσύνδεση και redirect στην οθόνη login σε περίπτωση λάθους

### Rate Limiting
- Όλα τα endpoints προστατεύονται από rate limiting (`[EnableRateLimiting("ApiPolicy")]`)

## Design

### Καθαρό & Απλό UI
- Χωρίς gradients και emojis
- Λευκό background με subtle shadows
- Καθαρά tables με hover effects
- Professional color palette:
  - Blue (#4a90e2): Primary actions
  - Green (#27ae60): Download buttons
  - Red (#e74c3c): Logout
  - Gray scale: Text και borders

### Responsive Design
- Προσαρμοσμένο για desktop και mobile
- Auto-scrolling tables για μεγάλα datasets
- Readable font sizes (14px body, 16px headings)

## UTF-8 Encoding

### CSV Export
- BOM (Byte Order Mark) prefix για σωστό άνοιγμα σε Excel
- Proper escaping για double quotes
- Πλήρης υποστήριξη ελληνικών χαρακτήρων

### Database
- Connection string με `Encoding=UTF8;Client Encoding=UTF8;`
- Σωστή αποθήκευση και ανάκτηση ελληνικών χαρακτήρων

## Παράδειγμα Χρήσης

1. Άνοιξε το `/vrpark/database/` στον browser
2. Εισήγαγε το admin key (π.χ. `admin123`)
3. Δες τα δεδομένα στους δύο πίνακες
4. Χρησιμοποίησε **Refresh** για νέα δεδομένα
5. Χρησιμοποίησε **Download CSV** για εξαγωγή δεδομένων
6. Κάνε **Αποσύνδεση** όταν τελειώσεις

## Διαφορές από το /admin

Παρόμοια λογική αλλά:
- Ξεχωριστό localStorage key: `vrparkAdminKey` vs `adminKey`
- Διαφορετικά API endpoints: `/api/vrpark/database/*` vs `/api/admin/*`
- Ανάγνωση μόνο (read-only) - δεν επιτρέπονται επεξεργασίες
- Καθαρότερο UI χωρίς animations και gradients
- Focus στην προβολή και εξαγωγή δεδομένων

## Σημειώσεις

- Η σελίδα είναι **100% read-only** - δεν μπορείς να αλλάξεις δεδομένα
- Το admin key πρέπει να αλλάξει σε production (environment variable `ADMIN_KEY`)
- Τα δεδομένα ταξινομούνται από νεότερα προς παλαιότερα
- Το eyetracking sequence εμφανίζεται ως μήκος χαρακτήρων στον πίνακα, αλλά εξάγεται πλήρες στο CSV
