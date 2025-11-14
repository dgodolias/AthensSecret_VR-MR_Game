import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';

// Custom metrics
const signupSuccess = new Counter('signup_success');
const verifySuccess = new Counter('verify_success');
const sessionStartSuccess = new Counter('session_start_success');
const sessionEndSuccess = new Counter('session_end_success');
const adminCallsSuccess = new Counter('admin_calls_success');
const sessionDuration = new Trend('session_duration');
const totalFlowSuccess = new Rate('total_flow_success');

// Configuration
const BASE_URL = 'https://athens-secret-api.onrender.com';
const ADMIN_KEY = 'DIOIKITO_VR_2025';

// Greek names data
const firstNames = [
    'Δημήτρης', 'Γιώργος', 'Νίκος', 'Κώστας', 'Μαρία', 'Ελένη', 
    'Αννα', 'Σοφία', 'Παναγιώτης', 'Ιωάννης', 'Βασίλης', 'Αθανάσιος',
    'Χρήστος', 'Κωνσταντίνος', 'Ευαγγελία', 'Κατερίνα', 'Δέσποινα',
    'Αλέξανδρος', 'Στέφανος', 'Θανάσης', 'Πέτρος', 'Μιχάλης'
];

const lastNames = [
    'Παπαδόπουλος', 'Γεωργίου', 'Νικολάου', 'Κωνσταντίνου', 'Δημητρίου',
    'Οικονόμου', 'Αντωνίου', 'Παπαντωνίου', 'Γκοντόλιας', 'Παναγιωτόπουλος',
    'Αθανασίου', 'Χριστοδούλου', 'Βασιλείου', 'Μιχαηλίδης', 'Πέτρου',
    'Σταματίου', 'Ανδρέου', 'Ελευθερίου', 'Κολοκοτρώνης', 'Μαυρογιάννης'
];

// Test configuration
export const options = {
    // Simulate 30 users over 10 days doing 3000 total requests
    // 3000 requests / 10 days = 300 requests/day
    // 300 requests/day / 30 users = 10 requests/user/day
    // 10 days = 240 hours = 14400 minutes
    
    stages: [
        { duration: '2m', target: 5 },      // Ramp up to 5 users (warm up)
        { duration: '5m', target: 30 },     // Ramp up to 30 users
        { duration: '20m', target: 30 },    // Stay at 30 users (main test - shortened for demo)
        { duration: '2m', target: 0 },      // Ramp down
    ],
    
    // For REAL 10-day test, uncomment this instead:
    /*
    stages: [
        { duration: '30m', target: 5 },     // Slow ramp up
        { duration: '1h', target: 30 },     // Reach 30 users
        { duration: '238h', target: 30 },   // Stay at 30 users for ~10 days
        { duration: '30m', target: 0 },     // Ramp down
    ],
    */
    
    thresholds: {
        http_req_duration: ['p(95)<2000'],  // 95% requests under 2s
        http_req_failed: ['rate<0.05'],     // Less than 5% errors
        'total_flow_success': ['rate>0.95'], // 95% successful flows
    },
};

// Generate random Greek user data
function generateUserData() {
    const firstName = firstNames[Math.floor(Math.random() * firstNames.length)];
    const lastName = lastNames[Math.floor(Math.random() * lastNames.length)];
    const randomNum = Math.floor(Math.random() * 10000);
    const email = `${firstName.toLowerCase()}.${lastName.toLowerCase()}${randomNum}@vrpark.test`;
    const age = Math.floor(Math.random() * 55) + 18; // Age between 18-72
    
    return { firstName, lastName, email, age };
}

// Generate realistic eyetracking sequence (500 chars, mostly 1s with some 0s)
function generateEyetrackingSequence() {
    let sequence = '';
    for (let i = 0; i < 500; i++) {
        // 85% chance of 1, 15% chance of 0 (mostly focused)
        sequence += Math.random() < 0.85 ? '1' : '0';
    }
    return sequence;
}

// 2.5% chance function
function shouldMakeAdminCall() {
    return Math.random() < 0.025; // 2.5% probability
}

// Admin endpoint call
function callAdminEndpoint(endpoint) {
    const headers = {
        'X-Admin-Key': ADMIN_KEY,
        'Content-Type': 'application/json',
    };
    
    const res = http.get(`${BASE_URL}/api/vrpark/database/${endpoint}`, { headers });
    
    const success = check(res, {
        [`admin ${endpoint} status is 200`]: (r) => r.status === 200,
        [`admin ${endpoint} has data`]: (r) => r.json().data !== undefined,
    });
    
    if (success) {
        adminCallsSuccess.add(1);
    }
    
    return res;
}

// Main test scenario
export default function () {
    let flowSuccess = true;
    const sessionStartTime = Date.now();
    
    // Step 1: Signup
    const userData = generateUserData();
    console.log(`[${__VU}] Starting flow for user: ${userData.firstName} ${userData.lastName}`);
    
    const signupPayload = JSON.stringify(userData);
    const signupHeaders = {
        'Content-Type': 'application/json; charset=utf-8',
    };
    
    let signupRes = http.post(
        `${BASE_URL}/api/vrpark/signup`,
        signupPayload,
        { headers: signupHeaders }
    );
    
    let userId;
    const signupOk = check(signupRes, {
        'signup status is 200': (r) => r.status === 200,
        'signup returns userId': (r) => {
            try {
                const body = r.json();
                userId = body.userId || body.UserId;
                return userId !== undefined;
            } catch (e) {
                return false;
            }
        },
    });
    
    if (signupOk) {
        signupSuccess.add(1);
        console.log(`[${__VU}] ✓ Signup successful - UserId: ${userId}`);
    } else {
        console.log(`[${__VU}] ✗ Signup failed - Status: ${signupRes.status}`);
        flowSuccess = false;
        totalFlowSuccess.add(flowSuccess);
        sleep(1);
        return;
    }
    
    // Random delay between signup and verify (5-10 seconds)
    sleep(Math.random() * 5 + 5);
    
    // 2.5% chance to call admin endpoint between step 1 and 2
    if (shouldMakeAdminCall()) {
        console.log(`[${__VU}] → Making admin call (users) after signup`);
        callAdminEndpoint('users');
        sleep(1);
    }
    
    // Step 2: Verify
    const verifyRes = http.get(`${BASE_URL}/api/vrpark/verify/${userId}`);
    
    const verifyOk = check(verifyRes, {
        'verify status is 200': (r) => r.status === 200,
        'verify returns success': (r) => {
            try {
                const body = r.json();
                return body.message !== undefined;
            } catch (e) {
                return false;
            }
        },
    });
    
    if (verifyOk) {
        verifySuccess.add(1);
        console.log(`[${__VU}] ✓ Verify successful`);
    } else {
        console.log(`[${__VU}] ✗ Verify failed - Status: ${verifyRes.status}`);
        flowSuccess = false;
        totalFlowSuccess.add(flowSuccess);
        sleep(1);
        return;
    }
    
    // Random delay between verify and session start (10-30 seconds)
    sleep(Math.random() * 20 + 10);
    
    // 2.5% chance to call admin endpoint between step 2 and 3
    if (shouldMakeAdminCall()) {
        console.log(`[${__VU}] → Making admin call (sessions) after verify`);
        callAdminEndpoint('sessions');
        sleep(1);
    }
    
    // Step 3: Session Start
    const sessionStartPayload = JSON.stringify({ userId });
    const sessionStartRes = http.post(
        `${BASE_URL}/api/vrpark/session/start`,
        sessionStartPayload,
        { headers: signupHeaders }
    );
    
    let sessionId;
    const sessionStartOk = check(sessionStartRes, {
        'session start status is 200': (r) => r.status === 200,
        'session start returns sessionId': (r) => {
            try {
                const body = r.json();
                sessionId = body.sessionId || body.SessionId;
                return sessionId !== undefined;
            } catch (e) {
                return false;
            }
        },
    });
    
    if (sessionStartOk) {
        sessionStartSuccess.add(1);
        console.log(`[${__VU}] ✓ Session started - SessionId: ${sessionId}`);
    } else {
        console.log(`[${__VU}] ✗ Session start failed - Status: ${sessionStartRes.status}`);
        flowSuccess = false;
        totalFlowSuccess.add(flowSuccess);
        sleep(1);
        return;
    }
    
    // Simulate VR game session duration (3-10 minutes)
    const gamePlayDuration = Math.random() * 7 + 3; // 3-10 minutes
    console.log(`[${__VU}] ⏱ Playing game for ${gamePlayDuration.toFixed(1)} minutes...`);
    sleep(gamePlayDuration * 60);
    
    // 2.5% chance to call admin endpoint between step 3 and 4
    if (shouldMakeAdminCall()) {
        console.log(`[${__VU}] → Making admin call (users) during session`);
        callAdminEndpoint('users');
        sleep(1);
    }
    
    // Step 4: Session End
    const eyetrackingSequence = generateEyetrackingSequence();
    const sessionEndPayload = JSON.stringify({
        sessionId,
        userId,
        eyetrackingSequence
    });
    
    const sessionEndRes = http.post(
        `${BASE_URL}/api/vrpark/session/end`,
        sessionEndPayload,
        { headers: signupHeaders }
    );
    
    const sessionEndOk = check(sessionEndRes, {
        'session end status is 200': (r) => r.status === 200,
        'session end returns duration': (r) => {
            try {
                const body = r.json();
                // API returns "Duration" as TimeSpan string (e.g., "00:05:30")
                return body.Duration !== undefined || body.duration !== undefined;
            } catch (e) {
                return false;
            }
        },
    });
    
    if (sessionEndOk) {
        sessionEndSuccess.add(1);
        const totalDuration = (Date.now() - sessionStartTime) / 1000 / 60; // in minutes
        sessionDuration.add(totalDuration);
        console.log(`[${__VU}] ✓ Session ended - Total duration: ${totalDuration.toFixed(2)} minutes`);
    } else {
        console.log(`[${__VU}] ✗ Session end failed - Status: ${sessionEndRes.status}`);
        flowSuccess = false;
    }
    
    totalFlowSuccess.add(flowSuccess);
    
    if (flowSuccess) {
        console.log(`[${__VU}] ✅ Complete flow successful!`);
    }
    
    // Random delay before next iteration (1-5 minutes to simulate user cooldown)
    sleep(Math.random() * 4 + 1);
}

// Summary at the end of the test
export function handleSummary(data) {
    console.log('\n' + '='.repeat(80));
    console.log('📊 VR PARK LOAD TEST SUMMARY');
    console.log('='.repeat(80));
    
    const metrics = data.metrics;
    
    console.log('\n🎯 Overall Performance:');
    console.log(`   Total Requests: ${metrics.http_reqs.values.count}`);
    console.log(`   Failed Requests: ${metrics.http_req_failed.values.passes} (${(metrics.http_req_failed.values.rate * 100).toFixed(2)}%)`);
    console.log(`   Avg Request Duration: ${metrics.http_req_duration.values.avg.toFixed(2)}ms`);
    console.log(`   P95 Duration: ${metrics.http_req_duration.values['p(95)'].toFixed(2)}ms`);
    console.log(`   P99 Duration: ${metrics.http_req_duration.values['p(99)'].toFixed(2)}ms`);
    
    console.log('\n📝 Step Success Counts:');
    console.log(`   Signups: ${metrics.signup_success.values.count}`);
    console.log(`   Verifies: ${metrics.verify_success.values.count}`);
    console.log(`   Session Starts: ${metrics.session_start_success.values.count}`);
    console.log(`   Session Ends: ${metrics.session_end_success.values.count}`);
    console.log(`   Admin Calls: ${metrics.admin_calls_success.values.count}`);
    
    console.log('\n⏱️  Session Statistics:');
    console.log(`   Avg Session Duration: ${metrics.session_duration.values.avg.toFixed(2)} minutes`);
    console.log(`   Min Session Duration: ${metrics.session_duration.values.min.toFixed(2)} minutes`);
    console.log(`   Max Session Duration: ${metrics.session_duration.values.max.toFixed(2)} minutes`);
    
    console.log('\n✅ Flow Success Rate:');
    console.log(`   Success Rate: ${(metrics.total_flow_success.values.rate * 100).toFixed(2)}%`);
    
    console.log('\n' + '='.repeat(80) + '\n');
    
    return {
        'stdout': JSON.stringify(data, null, 2),
        'summary.json': JSON.stringify(data, null, 2),
        'summary.html': htmlReport(data),
    };
}

// Simple HTML report generator
function htmlReport(data) {
    return `
<!DOCTYPE html>
<html>
<head>
    <title>VR Park Load Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
        h2 { color: #34495e; margin-top: 30px; }
        .metric { background: #ecf0f1; padding: 15px; margin: 10px 0; border-radius: 5px; display: flex; justify-content: space-between; }
        .metric-name { font-weight: bold; color: #2c3e50; }
        .metric-value { color: #27ae60; font-size: 1.2em; }
        .success { color: #27ae60; }
        .warning { color: #f39c12; }
        .error { color: #e74c3c; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #3498db; color: white; }
        tr:hover { background: #f8f9fa; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🎮 VR Park Load Test Report</h1>
        <p><strong>Test Date:</strong> ${new Date().toLocaleString()}</p>
        
        <h2>📊 Overall Performance</h2>
        <div class="metric">
            <span class="metric-name">Total HTTP Requests</span>
            <span class="metric-value">${data.metrics.http_reqs.values.count}</span>
        </div>
        <div class="metric">
            <span class="metric-name">Failed Requests</span>
            <span class="metric-value ${data.metrics.http_req_failed.values.rate > 0.05 ? 'error' : 'success'}">
                ${data.metrics.http_req_failed.values.passes} (${(data.metrics.http_req_failed.values.rate * 100).toFixed(2)}%)
            </span>
        </div>
        <div class="metric">
            <span class="metric-name">Average Response Time</span>
            <span class="metric-value">${data.metrics.http_req_duration.values.avg.toFixed(2)}ms</span>
        </div>
        <div class="metric">
            <span class="metric-name">P95 Response Time</span>
            <span class="metric-value ${data.metrics.http_req_duration.values['p(95)'] > 2000 ? 'warning' : 'success'}">
                ${data.metrics.http_req_duration.values['p(95)'].toFixed(2)}ms
            </span>
        </div>
        
        <h2>✅ Flow Success Metrics</h2>
        <table>
            <tr>
                <th>Step</th>
                <th>Success Count</th>
            </tr>
            <tr>
                <td>1. Signups</td>
                <td class="success">${data.metrics.signup_success.values.count}</td>
            </tr>
            <tr>
                <td>2. Verifications</td>
                <td class="success">${data.metrics.verify_success.values.count}</td>
            </tr>
            <tr>
                <td>3. Session Starts</td>
                <td class="success">${data.metrics.session_start_success.values.count}</td>
            </tr>
            <tr>
                <td>4. Session Ends</td>
                <td class="success">${data.metrics.session_end_success.values.count}</td>
            </tr>
            <tr>
                <td>Admin API Calls (2.5% random)</td>
                <td class="success">${data.metrics.admin_calls_success.values.count}</td>
            </tr>
        </table>
        
        <h2>⏱️ Session Duration Statistics</h2>
        <div class="metric">
            <span class="metric-name">Average Session Duration</span>
            <span class="metric-value">${data.metrics.session_duration.values.avg.toFixed(2)} minutes</span>
        </div>
        <div class="metric">
            <span class="metric-name">Min Session Duration</span>
            <span class="metric-value">${data.metrics.session_duration.values.min.toFixed(2)} minutes</span>
        </div>
        <div class="metric">
            <span class="metric-name">Max Session Duration</span>
            <span class="metric-value">${data.metrics.session_duration.values.max.toFixed(2)} minutes</span>
        </div>
        
        <h2>🎯 Complete Flow Success Rate</h2>
        <div class="metric">
            <span class="metric-name">End-to-End Success Rate</span>
            <span class="metric-value ${data.metrics.total_flow_success.values.rate < 0.95 ? 'error' : 'success'}">
                ${(data.metrics.total_flow_success.values.rate * 100).toFixed(2)}%
            </span>
        </div>
    </div>
</body>
</html>
    `.trim();
}
