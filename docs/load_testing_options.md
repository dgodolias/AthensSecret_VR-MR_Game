# Load Testing Options για VR Park API

## 📊 Απαιτήσεις Simulation

- **Συνολικά Requests**: 3,000 σε 10 ημέρες
- **Clients**: 30 διαφορετικοί
- **Requests per day**: ~300
- **Requests per client**: ~100 συνολικά
- **Επικαλυπτόμενα requests**: Ναι (concurrent)

### Flow Path (1→2→3→4 με πιθανότητες):
1. **Signup** (POST `/api/vrpark/signup`)
2. **Verify** (GET `/api/vrpark/verify/{userId}`)
3. **Session Start** (POST `/api/vrpark/session/start`)
4. **Session End** (POST `/api/vrpark/session/end`)

**Παράκαμψη με 2.5% πιθανότητα**:
- Μεταξύ 1→2: Μπορεί να γίνει 5 ή 6
- Μεταξύ 2→3: Μπορεί να γίνει 5 ή 6
- Μεταξύ 3→4: Μπορεί να γίνει 5 ή 6

**Admin Endpoints (5 & 6)**:
5. **Database Users** (GET `/api/vrpark/database/users` με X-Admin-Key header)
6. **Database Sessions** (GET `/api/vrpark/database/sessions` με X-Admin-Key header)

---

## 🎯 ΕΠΙΛΟΓΗ 1: k6 (Grafana Labs) - ΠΡΟΤΕΙΝΕΤΑΙ

### Πλεονεκτήματα:
- ✅ **Modern & Popular** - Industry standard για load testing
- ✅ **JavaScript-based** - Εύκολο scripting
- ✅ **Cloud & Local** - Μπορεί να τρέξει οπουδήποτε
- ✅ **Beautiful Reports** - Built-in HTML reports
- ✅ **Realistic Scenarios** - Virtual Users με think time
- ✅ **Free & Open Source**

### Χαρακτηριστικά:
- Virtual Users (VUs) simulation
- Staged ramp-up/ramp-down
- Custom metrics & thresholds
- HTTP/2 support
- WebSocket support
- JSON validation

### Installation:
```bash
# Windows (με Chocolatey)
choco install k6

# Ή download από https://k6.io/docs/getting-started/installation/
```

### Δομή Script:
```javascript
// load-test.js
import http from 'k6/http';
import { sleep, check } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 5 },   // Ramp up to 5 users
    { duration: '8d', target: 30 },  // Stay at 30 users for 8 days
    { duration: '1m', target: 0 },   // Ramp down
  ],
};

export default function () {
  // 1. Signup
  // 2. Verify
  // 3. Session Start
  // 4. Session End
  // Random 2.5% chance για admin endpoints
}
```

### Εκτέλεση:
```bash
k6 run load-test.js
```

### Αποτελέσματα:
- Console output με live stats
- HTML report με graphs
- JSON export για analysis

---

## 🎯 ΕΠΙΛΟΓΗ 2: Apache JMeter

### Πλεονεκτήματα:
- ✅ **Battle-tested** - Χρησιμοποιείται 20+ χρόνια
- ✅ **GUI-based** - Drag & drop test creation
- ✅ **Extensive Plugins** - Τεράστιο ecosystem
- ✅ **Detailed Reports** - Dashboards, graphs, tables
- ✅ **Record & Replay** - Μπορεί να record πραγματικά requests

### Χαρακτηριστικά:
- Thread Groups για users
- Timers για think time
- Assertions για validation
- Listeners για results
- CSV data sets για parameterization
- Distributed testing

### Installation:
```bash
# Download από https://jmeter.apache.org/download_jmeter.cgi
# Χρειάζεται Java JDK
```

### Δομή Test Plan:
```
Test Plan
├── Thread Group (30 users, 10 days)
│   ├── HTTP Request Defaults
│   ├── User Defined Variables (emails, ages, names)
│   ├── CSV Data Set (test data)
│   ├── Flow:
│   │   ├── Signup Request
│   │   ├── If Controller (2.5% chance admin)
│   │   ├── Verify Request
│   │   ├── Session Start Request
│   │   ├── Session End Request
│   └── Timers (random delays)
└── Listeners (graphs, tables, reports)
```

### Εκτέλεση:
```bash
# GUI mode για development
jmeter

# CLI mode για actual testing
jmeter -n -t test-plan.jmx -l results.jtl -e -o report-folder
```

---

## 🎯 ΕΠΙΛΟΓΗ 3: Locust (Python-based)

### Πλεονεκτήματα:
- ✅ **Python** - Αν ξέρεις Python
- ✅ **Web UI** - Real-time monitoring
- ✅ **Distributed** - Μπορεί να scale σε πολλά machines
- ✅ **Code-based** - Version control friendly
- ✅ **Easy to extend**

### Χαρακτηριστικά:
- User classes με behaviors
- Task decorators με weights
- Events & hooks
- CSV/JSON data feeding
- Real-time stats

### Installation:
```bash
pip install locust
```

### Δομή Script:
```python
# locustfile.py
from locust import HttpUser, task, between
import random

class VRParkUser(HttpUser):
    wait_time = between(5, 15)  # Think time
    
    def on_start(self):
        # Signup
        pass
    
    @task(1)
    def verify_and_session(self):
        # Verify
        # 2.5% chance admin call
        # Session start
        # Session end
        pass
```

### Εκτέλεση:
```bash
locust -f locustfile.py --users 30 --spawn-rate 1 --run-time 10d
```

### Web UI:
- Open browser: `http://localhost:8089`
- Live charts & stats
- Start/stop control

---

## 🎯 ΕΠΙΛΟΓΗ 4: Artillery

### Πλεονεκτήματα:
- ✅ **YAML-based** - Απλό configuration
- ✅ **NPM package** - Easy installation
- ✅ **Scenarios** - Complex user flows
- ✅ **Built-in metrics** - No setup needed
- ✅ **CI/CD friendly**

### Χαρακτηριστικά:
- Phases για ramp-up
- Scenarios με flows
- Variables & functions
- Plugins για extended functionality
- Socket.io support

### Installation:
```bash
npm install -g artillery
```

### Δομή Config:
```yaml
# load-test.yml
config:
  target: 'https://athens-secret-api.onrender.com'
  phases:
    - duration: 864000  # 10 days in seconds
      arrivalRate: 1
      maxVusers: 30

scenarios:
  - name: "VR Park Full Flow"
    flow:
      - post:
          url: "/api/vrpark/signup"
          json:
            firstName: "{{ $randomString() }}"
      - get:
          url: "/api/vrpark/verify/{{ userId }}"
      # etc...
```

### Εκτέλεση:
```bash
artillery run load-test.yml
```

---

## 🎯 ΕΠΙΛΟΓΗ 5: Custom C# Console App

### Πλεονεκτήματα:
- ✅ **Full Control** - Κάνεις ό,τι θες
- ✅ **Same Stack** - C# όπως το API
- ✅ **HttpClient** - Native support
- ✅ **Async/Await** - True parallelism
- ✅ **Entity Framework** - Μπορείς να validate DB

### Χαρακτηριστικά:
- Task-based concurrency
- Rate limiting με SemaphoreSlim
- Progress reporting
- CSV export
- Custom metrics

### Δομή:
```
VRParkLoadTester/
├── Program.cs (main orchestration)
├── VRParkClient.cs (API wrapper)
├── UserScenario.cs (flow logic)
├── DataGenerator.cs (fake data)
├── MetricsCollector.cs (stats)
└── Report.cs (results output)
```

### Εκτέλεση:
```bash
dotnet run --configuration Release
```

---

## 🎯 ΕΠΙΛΟΓΗ 6: Postman Collection Runner

### Πλεονεκτήματα:
- ✅ **No coding** - GUI-based
- ✅ **Quick setup** - Αν έχεις ήδη collection
- ✅ **Newman CLI** - For automation
- ✅ **Data files** - CSV/JSON iteration
- ✅ **Cloud option** - Postman Cloud testing

### Χαρακτηριστικά:
- Collection με requests
- Pre-request scripts
- Tests για validation
- Environment variables
- Iteration με data files

### Setup:
1. Import VR Park API collection
2. Add test data CSV
3. Configure runner
4. Run iterations

### Εκτέλεση:
```bash
# CLI με Newman
newman run collection.json -d test-data.csv --iteration-count 3000
```

---

## 📊 Σύγκριση Επιλογών

| Tool | Difficulty | Flexibility | Reporting | Realistic | Free |
|------|-----------|-------------|-----------|-----------|------|
| **k6** | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ✅ |
| **JMeter** | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ✅ |
| **Locust** | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ✅ |
| **Artillery** | ⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ✅ |
| **Custom C#** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ✅ |
| **Postman** | ⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ✅ |

---

## 🎯 Η Πρόταση μου

### Για το Use Case σου (3000 requests, 10 days, 30 users):

**🥇 #1: k6**
- Πιο σύγχρονο & popular
- Άψογα reports
- JavaScript (εύκολο)
- Perfect για το scenario σου

**🥈 #2: Locust**
- Python (αν το προτιμάς)
- Web UI για live monitoring
- Πολύ εύκολο scaling

**🥉 #3: Custom C# App**
- Αν θες full control
- Same stack με το API
- Μπορείς να κάνεις & DB validation

---

## 📋 Επιπλέον Considerations

### Test Data Generation:
- Faker libraries (Bogus για C#, faker.js, Faker για Python)
- Ρεαλιστικά ονόματα, emails, ηλικίες
- Διαφορετικά data per user

### Metrics να Track:
- ✅ Response times (p50, p95, p99)
- ✅ Success rate
- ✅ Error rate by endpoint
- ✅ Concurrent users
- ✅ Requests per second
- ✅ Database query times
- ✅ Memory usage
- ✅ CPU usage

### Think Time:
- Signup → Verify: 5-10 seconds
- Verify → Session Start: 10-30 seconds
- Session Start → Session End: 3-10 minutes (διάρκεια παιχνιδιού)

### Validation:
- Status codes (200, 400, 401, 403)
- Response body structure
- Database state (userId exists, session created)
- Rate limiting (429 responses)

---

Πες μου ποια επιλογή σε ενδιαφέρει και φτιάχνω το complete implementation! 🚀
