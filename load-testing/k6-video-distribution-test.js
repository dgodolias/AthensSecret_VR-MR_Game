import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

// Metrics to track video distribution
const video1 = new Counter('video_1_count');
const video2 = new Counter('video_2_count');
const video3 = new Counter('video_3_count');
const video4 = new Counter('video_4_count');
const totalSignups = new Counter('total_signups');

// Configuration
const BASE_URL = 'https://athens-secret-api.onrender.com'; // Αλλάξτε το σε http://localhost:5000 αν τρέχετε τοπικά

// Test configuration
export const options = {
    scenarios: {
        distribution_test: {
            executor: 'per-vu-iterations',
            vus: 50,              // 50 ταυτόχρονοι χρήστες
            iterations: 4,        // 4 επαναλήψεις ο καθένας (Σύνολο 200 εγγραφές)
            maxDuration: '30s',
        },
    },
    thresholds: {
        http_req_failed: ['rate<0.01'], // Λιγότερο από 1% αποτυχίες
    },
};

// Helper to generate random user data
function generateUserData() {
    const randomNum = Math.floor(Math.random() * 100000);
    return {
        firstName: `TestUser`,
        lastName: `DistCheck${randomNum}`,
        email: `dist.check.${randomNum}@test.com`,
        age: 25
    };
}

export default function () {
    const userData = generateUserData();
    
    const payload = JSON.stringify(userData);
    const headers = {
        'Content-Type': 'application/json; charset=utf-8',
    };
    
    const res = http.post(
        `${BASE_URL}/api/vrpark/signup`,
        payload,
        { headers: headers }
    );
    
    const success = check(res, {
        'status is 200': (r) => r.status === 200,
        'has video id': (r) => r.json('video') !== undefined,
    });

    if (success) {
        totalSignups.add(1);
        const videoId = res.json('video');
        
        // Καταμέτρηση των Video IDs
        if (videoId === 1) video1.add(1);
        else if (videoId === 2) video2.add(1);
        else if (videoId === 3) video3.add(1);
        else if (videoId === 4) video4.add(1);
    }
}

export function handleSummary(data) {
    const v1 = data.metrics.video_1_count.values.count;
    const v2 = data.metrics.video_2_count.values.count;
    const v3 = data.metrics.video_3_count.values.count;
    const v4 = data.metrics.video_4_count.values.count;
    const total = data.metrics.total_signups.values.count;

    console.log('\n' + '='.repeat(50));
    console.log('📊 VIDEO DISTRIBUTION REPORT');
    console.log('='.repeat(50));
    console.log(`Total Signups: ${total}`);
    console.log('-'.repeat(50));
    console.log(`Video 1: ${v1} (${((v1/total)*100).toFixed(1)}%)`);
    console.log(`Video 2: ${v2} (${((v2/total)*100).toFixed(1)}%)`);
    console.log(`Video 3: ${v3} (${((v3/total)*100).toFixed(1)}%)`);
    console.log(`Video 4: ${v4} (${((v4/total)*100).toFixed(1)}%)`);
    console.log('='.repeat(50));
    
    // Έλεγχος ομοιομορφίας (χονδρικά)
    const expected = total / 4;
    const tolerance = expected * 0.3; // 30% ανοχή λόγω τυχαιότητας
    
    const isUniform = 
        Math.abs(v1 - expected) < tolerance &&
        Math.abs(v2 - expected) < tolerance &&
        Math.abs(v3 - expected) < tolerance &&
        Math.abs(v4 - expected) < tolerance;

    if (isUniform) {
        console.log('✅ Η κατανομή φαίνεται ομοιόμορφη και τυχαία.');
    } else {
        console.log('⚠️ Η κατανομή δεν φαίνεται πολύ ομοιόμορφη (μπορεί να είναι τυχαίο σε μικρό δείγμα).');
    }
    console.log('\n');

    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
    };
}
