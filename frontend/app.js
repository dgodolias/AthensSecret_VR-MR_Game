// Global variables
let currentSessionId = null;
let currentPlayerData = null;
let chartInstances = {};
let authToken = localStorage.getItem('authToken');
let currentUser = localStorage.getItem('currentUser');

// API Configuration - DEPRECATED: Now loaded from backend API
// Unity clients should use: GET /api/config/game and GET /api/config/server
const API_BASE_URL = 'http://localhost:5182/api'; // Fallback for dummy frontend only

// Console logging helper for API calls
function logAPICall(method, url, requestData, responseData, status) {
    console.group(`🔄 API ${method.toUpperCase()} ${url}`);
    console.log(`🌐 Full URL: ${url}`);
    console.log(`📋 Status: ${status}`);
    
    if (requestData) {
        console.log(`📤 REQUEST BODY:`, requestData);
    }
    
    if (responseData) {
        console.log(`📥 RESPONSE DATA:`, responseData);
    }
    
    // Log headers if available
    const token = localStorage.getItem('authToken');
    if (token) {
        console.log(`🔐 Authorization: Bearer ${token.substring(0, 20)}...`);
    }
    
    console.log(`🕐 Timestamp: ${new Date().toISOString()}`);
    console.groupEnd();
}

// Helper function for authenticated API requests
async function authenticatedFetch(url, options = {}) {
    const token = localStorage.getItem('authToken');
    if (!token) {
        console.error('❌ AUTHENTICATED FETCH FAILED - No token found');
        throw new Error('No authentication token found');
    }
    
    const headers = {
        ...options.headers,
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };
    
    console.log(`🔐 AUTHENTICATED FETCH - URL: ${url}`);
    console.log(`🔑 Using token: ${token.substring(0, 20)}...`);
    
    return fetch(url, {
        ...options,
        headers: headers
    });
}

// Check authentication on page load
document.addEventListener('DOMContentLoaded', function() {
    // Console logging banner
    console.log(`
    🎮 VR GAME API DEBUGGING ENABLED 🎮
    ===================================
    📊 All API calls will be logged with detailed information:
    📤 Request data, 📥 Response data, 🌐 URLs, 🔐 Auth tokens
    
    🎯 Expected API Flow:
    1. POST /api/auth/register OR /api/auth/login
    2. GET /api/config/game (Unity should fetch game configuration)
    3. GET /api/config/server (Unity should fetch server info)
    4. POST /api/game/start
    5. POST /api/game/submit-complete
    6. POST /api/auth/logout
    
    📋 Watch for:
    - Request/Response schemas
    - HTTP status codes
    - JWT token usage
    - API route paths
    
    ⚠️  NOTE: This is a DUMMY FRONTEND for testing only!
    🎮 Unity clients should:
    - Fetch configuration from /api/config/game
    - Use standardized API responses
    - Implement proper error handling
    
    ⚙️  CONFIGURATION NOW SERVER-SIDE:
    - API Base URL: ${API_BASE_URL}
    - Game Config: Available via API endpoint
    - Server Info: Available via API endpoint  
    - Console Logging: Always enabled for debugging
    ===================================
    `);
    
    if (authToken && currentUser) {
        console.log(`🔄 EXISTING SESSION FOUND - User: ${currentUser}, Token: ${authToken.substring(0, 20)}...`);
        showAuthenticatedView();
    }
});

// Authentication Functions
function switchTab(tab) {
    // Switch tab styling
    document.querySelectorAll('.auth-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.auth-form').forEach(f => f.classList.remove('active'));
    
    document.querySelector(`[onclick="switchTab('${tab}')"]`).classList.add('active');
    document.getElementById(`${tab}Form`).classList.add('active');
}

// NEW: Configuration fetching functions for Unity demonstration
async function fetchGameConfig() {
    try {
        console.log('🎮 FETCHING GAME CONFIG - Unity clients should call this on startup');
        
        const response = await fetch(`${API_BASE_URL}/config/game`);
        const configData = await response.json();
        
        logAPICall('GET', `${API_BASE_URL}/config/game`, null, configData, response.status);
        
        if (configData.success && configData.data) {
            console.log('✅ GAME CONFIG LOADED - Unity can now apply these settings:', configData.data);
            
            // Display config in UI for testing purposes
            displayGameConfig(configData.data);
            return configData.data;
        } else {
            throw new Error('Failed to load game configuration');
        }
    } catch (error) {
        console.error('❌ GAME CONFIG FETCH FAILED:', error);
        throw error;
    }
}

async function fetchServerInfo() {
    try {
        console.log('🌐 FETCHING SERVER INFO - Unity clients should call this to get connection info');
        
        const response = await fetch(`${API_BASE_URL}/config/server`);
        const serverData = await response.json();
        
        logAPICall('GET', `${API_BASE_URL}/config/server`, null, serverData, response.status);
        
        if (serverData.success && serverData.data) {
            console.log('✅ SERVER INFO LOADED - Unity can use this for connection management:', serverData.data);
            
            // Display server info in UI for testing purposes
            displayServerInfo(serverData.data);
            return serverData.data;
        } else {
            throw new Error('Failed to load server information');
        }
    } catch (error) {
        console.error('❌ SERVER INFO FETCH FAILED:', error);
        throw error;
    }
}

function displayGameConfig(config) {
    const configDiv = document.createElement('div');
    configDiv.id = 'gameConfigDisplay';
    configDiv.innerHTML = `
        <h3>🎮 Game Configuration (Loaded from Backend)</h3>
        <div class="config-section">
            <h4>Energy Settings:</h4>
            <ul>
                <li>Starting Energy: ${config.energy.startingEnergy}</li>
                <li>Max Energy: ${config.energy.maxEnergy}</li>
                <li>Warning Threshold: ${config.energy.energyCapWarningThreshold}</li>
            </ul>
        </div>
        <div class="config-section">
            <h4>Patience Trial:</h4>
            <ul>
                <li>Timer Duration: ${config.trials.patience.timerDurationSeconds}s</li>
                <li>Bonus Threshold: ${config.trials.patience.bonusThresholdSeconds}s</li>
                <li>Energy Bonus: ${config.trials.patience.bonusEnergyAmount}</li>
                <li>Number of Mirrors: ${config.trials.patience.numberOfMirrors}</li>
            </ul>
        </div>
        <div class="config-section">
            <h4>Olive Investment:</h4>
            <ul>
                <li>Formula: ${config.trials.resource.oliveInvestment.formula}</li>
                <li>Max Bonus: ${config.trials.resource.oliveInvestment.maxBonusCap}</li>
                <li>Update Interval: ${config.trials.resource.oliveInvestment.updateIntervalMs}ms</li>
            </ul>
        </div>
    `;
    
    // Remove existing config display if present
    const existing = document.getElementById('gameConfigDisplay');
    if (existing) existing.remove();
    
    // Add to page
    document.querySelector('.container').appendChild(configDiv);
}

function displayServerInfo(serverInfo) {
    const infoDiv = document.createElement('div');
    infoDiv.id = 'serverInfoDisplay';
    infoDiv.innerHTML = `
        <h3>🌐 Server Information (For Unity Connection)</h3>
        <div class="config-section">
            <h4>Server Status:</h4>
            <ul>
                <li>Version: ${serverInfo.serverVersion}</li>
                <li>API Version: ${serverInfo.apiVersion}</li>
                <li>Environment: ${serverInfo.environment}</li>
                <li>Health: ${serverInfo.status.isHealthy ? '✅ Healthy' : '❌ Unhealthy'}</li>
                <li>Active Sessions: ${serverInfo.status.activeSessions}</li>
            </ul>
        </div>
        <div class="config-section">
            <h4>API Endpoints:</h4>
            <ul>
                <li>Base URL: ${serverInfo.endpoints.baseUrl}</li>
                <li>Auth Login: ${serverInfo.endpoints.auth.login}</li>
                <li>Game Start: ${serverInfo.endpoints.game.start}</li>
                <li>Game Config: ${serverInfo.endpoints.config.gameConfig}</li>
            </ul>
        </div>
    `;
    
    // Remove existing info display if present
    const existing = document.getElementById('serverInfoDisplay');
    if (existing) existing.remove();
    
    // Add to page
    document.querySelector('.container').appendChild(infoDiv);
}
    


async function register() {
    const username = document.getElementById('registerUsername').value.trim();
    const password = document.getElementById('registerPassword').value;
    
    if (!username || !password) {
        showResult('Παρακαλώ συμπληρώστε όλα τα πεδία', 'error');
        return;
    }
    
    try {
        const requestData = { username, password };
        const url = `${API_BASE_URL}/auth/register`;
        
        console.log(`🚀 STARTING REGISTER API CALL`);
        logAPICall('POST', url, requestData, null, 'PENDING');
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });
        
        const data = await response.json();
        
        // Log the complete response
        logAPICall('POST', url, requestData, data, response.status);
        
        if (response.ok) {
            // Store authentication data
            authToken = data.token;
            currentUser = data.username;
            localStorage.setItem('authToken', authToken);
            localStorage.setItem('currentUser', currentUser);
            localStorage.setItem('playerId', data.playerId);
            
            console.log(`✅ REGISTER SUCCESS - Token saved, User: ${currentUser}, PlayerId: ${data.playerId}`);
            showResult(`Επιτυχής εγγραφή! Καλώς ήρθες, ${data.username}!`, 'success');
            showAuthenticatedView();
        } else {
            console.error(`❌ REGISTER FAILED - Status: ${response.status}, Message: ${data.message}`);
            showResult(data.message || 'Σφάλμα εγγραφής', 'error');
        }
    } catch (error) {
        console.error('🔥 REGISTER ERROR:', error);
        showResult('Σφάλμα σύνδεσης με τον server', 'error');
        console.error('Registration error:', error);
    }
}

async function login() {
    const username = document.getElementById('loginUsername').value.trim();
    const password = document.getElementById('loginPassword').value;
    
    if (!username || !password) {
        showResult('Παρακαλώ συμπληρώστε όλα τα πεδία', 'error');
        return;
    }
    
    try {
        const requestData = { username, password };
        const url = `${API_BASE_URL}/auth/login`;
        
        console.log(`🚀 STARTING LOGIN API CALL`);
        logAPICall('POST', url, requestData, null, 'PENDING');
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });
        
        const data = await response.json();
        
        // Log the complete response
        logAPICall('POST', url, requestData, data, response.status);
        
        if (response.ok) {
            // Store authentication data
            authToken = data.token;
            currentUser = data.username;
            localStorage.setItem('authToken', authToken);
            localStorage.setItem('currentUser', currentUser);
            localStorage.setItem('playerId', data.playerId);
            
            console.log(`✅ LOGIN SUCCESS - Token saved, User: ${currentUser}, PlayerId: ${data.playerId}`);
            showResult(`Επιτυχής είσοδος! Καλώς ήρθες πίσω, ${data.username}!`, 'success');
            showAuthenticatedView();
        } else {
            console.error(`❌ LOGIN FAILED - Status: ${response.status}, Message: ${data.message}`);
            showResult(data.message || 'Λάθος στοιχεία', 'error');
        }
    } catch (error) {
        console.error('🔥 LOGIN ERROR:', error);
        showResult('Σφάλμα σύνδεσης με τον server', 'error');
        console.error('Login error:', error);
    }
}

async function logout() {
    try {
        const url = `${API_BASE_URL}/auth/logout`;
        
        console.log(`🚀 STARTING LOGOUT API CALL`);
        logAPICall('POST', url, null, null, 'PENDING');
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });
        
        // Try to get response data if available
        let responseData = null;
        try {
            responseData = await response.text();
            if (responseData) {
                responseData = JSON.parse(responseData);
            }
        } catch (e) {
            // Response might be empty (204 No Content)
            responseData = { message: 'No content (204)' };
        }
        
        logAPICall('POST', url, null, responseData, response.status);
        console.log(`✅ LOGOUT COMPLETED - Status: ${response.status}`);
        
    } catch (error) {
        console.error('🔥 LOGOUT ERROR:', error);
        console.error('Logout error:', error);
    }
    
    // Clear stored data
    authToken = null;
    currentUser = null;
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    localStorage.removeItem('playerId');
    
    console.log(`🧹 LOGOUT CLEANUP - Cleared localStorage and tokens`);
    showResult('Επιτυχής έξοδος!', 'success');
    showUnauthenticatedView();
}

function showAuthenticatedView() {
    document.getElementById('authSection').style.display = 'none';
    document.getElementById('userInfo').style.display = 'block';
    document.getElementById('gameSection').style.display = 'block';
    document.getElementById('loggedInUser').textContent = currentUser;
    
    // Auto-load player dashboard
    loadPlayerSessions();
}

function showUnauthenticatedView() {
    document.getElementById('authSection').style.display = 'block';
    document.getElementById('userInfo').style.display = 'none';
    document.getElementById('gameSection').style.display = 'none';
    document.getElementById('playerDashboard').style.display = 'none';
    document.getElementById('gameState').style.display = 'none';
}

// Show result messages
function showResult(message, type = 'info') {
    displayResult(message, type);
}

// Load player sessions (used after login)
async function loadPlayerSessions() {
    if (!authToken || !currentUser) return;
    
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/game/player/${encodeURIComponent(currentUser)}/sessions`);
        
        if (response.ok) {
            const data = await response.json();
            displayPlayerDashboard(data);
        }
    } catch (error) {
        console.error('Error loading player sessions:', error);
    }
}

// Fetch and display stats for a given session ID
async function fetchPlayerSessions(username) {
    try {
        const url = `${API_BASE_URL}/game/player/${encodeURIComponent(username)}/sessions`;
        
        console.log(`🚀 STARTING FETCH PLAYER SESSIONS API CALL`);
        logAPICall('GET', url, null, null, 'PENDING');
        
        const response = await authenticatedFetch(url);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        // Log the complete response
        logAPICall('GET', url, null, data, response.status);
        console.log(`✅ FETCH SESSIONS SUCCESS - Found ${data.totalSessions} sessions for ${username}`);
        
        displayPlayerDashboard(data);

    } catch (error) {
        console.error('🔥 FETCH SESSIONS ERROR:', error);
        displayResult(`Σφάλμα ανάκτησης στατιστικών: ${error.message}`, 'error');
        console.error('Error fetching player sessions:', error);
    }
}

// Display player dashboard with all sessions
function displayPlayerDashboard(data) {
    currentPlayerData = data; // Store for filtering
    const dashboardDiv = document.getElementById('playerDashboard');
    const statsDiv = document.getElementById('playerStats');
    const chartsDiv = document.getElementById('chartsContainer');
    const dropdownDiv = document.getElementById('sessionDropdown');

    // Player stats
    const totalScore = data.sessions.reduce((sum, session) => sum + session.score, 0);
    const avgScore = data.totalSessions > 0 ? Math.round(totalScore / data.totalSessions) : 0;
    const completedSessions = data.sessions.filter(s => !s.isActive).length;

    statsDiv.innerHTML = `
        <div class="info">
            <h4>📊 Στατιστικά για ${data.username}</h4>
            <p><strong>Συνολικά Sessions:</strong> ${data.totalSessions}</p>
            <p><strong>Ολοκληρωμένα:</strong> ${completedSessions}</p>
            <p><strong>Συνολική Βαθμολογία:</strong> ${totalScore}</p>
            <p><strong>Μέση Βαθμολογία:</strong> ${avgScore}</p>
        </div>
    `;

    // Populate dropdown
    populateSessionDropdown(data.sessions);

    // Generate charts for all sessions initially
    generateCharts(data.sessions, chartsDiv);

    // Show current session details by default
    const currentSession = data.sessions.find(s => s.isActive) || data.sessions[0];
    if (currentSession) {
        showSessionDetails(currentSession);
        dropdownDiv.value = currentSession.sessionId;
    }

    dashboardDiv.style.display = 'block';
}

// Populate session dropdown
function populateSessionDropdown(sessions) {
    const dropdown = document.getElementById('sessionDropdown');
    
    // Clear existing options except "All"
    dropdown.innerHTML = '<option value="all">Όλα τα Sessions</option>';
    
    // Add session options
    sessions.forEach(session => {
        const option = document.createElement('option');
        option.value = session.sessionId;
        const statusText = session.isActive ? '(Ενεργό)' : '(Ολοκληρωμένο)';
        option.textContent = `Session ${session.sessionId} - Score: ${session.score} ${statusText}`;
        dropdown.appendChild(option);
    });
}

// Filter session data based on dropdown selection
function filterSessionData() {
    const dropdown = document.getElementById('sessionDropdown');
    const selectedValue = dropdown.value;
    
    if (!currentPlayerData) return;
    
    if (selectedValue === 'all') {
        // Show all sessions data
        generateCharts(currentPlayerData.sessions, document.getElementById('chartsContainer'));
        document.getElementById('currentSessionDetails').innerHTML = '<p><em>Εμφάνιση δεδομένων όλων των sessions</em></p>';
    } else {
        // Show specific session
        const sessionId = parseInt(selectedValue);
        const selectedSession = currentPlayerData.sessions.find(s => s.sessionId === sessionId);
        
        if (selectedSession) {
            generateCharts([selectedSession], document.getElementById('chartsContainer'));
            showSessionDetails(selectedSession);
        }
    }
}

// Show details for a specific session
function showSessionDetails(session) {
    const detailsDiv = document.getElementById('currentSessionDetails');
    const statusText = session.isActive ? 'Ενεργό' : 'Ολοκληρωμένο';
    const endTimeText = session.endTime ? new Date(session.endTime).toLocaleString() : '-';
    
    detailsDiv.innerHTML = `
        <div class="session-detail-card">
            <h4>🎯 Λεπτομέρειες Session ${session.sessionId}</h4>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px;">
                <div>
                    <p><strong>Status:</strong> ${statusText}</p>
                    <p><strong>Βαθμολογία:</strong> ${session.score}</p>
                    <p><strong>Ενέργεια:</strong> ${session.wisdomEnergy}</p>
                </div>
                <div>
                    <p><strong>Τρέχουσα Δοκιμασία:</strong> ${session.currentTrial}</p>
                    <p><strong>Έναρξη:</strong> ${new Date(session.startTime).toLocaleString()}</p>
                    <p><strong>Λήξη:</strong> ${endTimeText}</p>
                </div>
            </div>
        </div>
    `;
}

// Generate charts using Chart.js
function generateCharts(sessions, container) {
    if (!window.Chart || sessions.length === 0) {
        container.innerHTML = '<p>Δεν υπάρχουν δεδομένα για charts.</p>';
        return;
    }

    // Destroy existing charts
    Object.values(chartInstances).forEach(chart => {
        if (chart) chart.destroy();
    });
    chartInstances = {};

    const completedSessions = sessions.filter(s => !s.isActive);
    
    // Create charts HTML structure
    container.innerHTML = `
        <h4>📈 ${sessions.length === 1 ? `Στατιστικά Session ${sessions[0].sessionId}` : 'Συνολικά Στατιστικά'}</h4>
        <div class="charts-grid">
            <div class="chart-container">
                <h5>Εξέλιξη Βαθμολογίας</h5>
                <canvas id="scoresChart" width="400" height="200"></canvas>
            </div>
            <div class="chart-container">
                <h5>Κατανομή Ενέργειας</h5>
                <canvas id="energyChart" width="400" height="200"></canvas>
            </div>
            <div class="chart-container">
                <h5>Score vs Energy</h5>
                <canvas id="comparisonChart" width="400" height="200"></canvas>
            </div>
            <div class="chart-container">
                <h5>Κατανομή Βαθμολογιών</h5>
                <canvas id="scoreDistChart" width="400" height="200"></canvas>
            </div>
        </div>
    `;

    setTimeout(() => {
        // 1. Scores Timeline
        if (completedSessions.length > 0) {
            const ctx1 = document.getElementById('scoresChart');
            if (ctx1) {
                chartInstances.scores = new Chart(ctx1, {
                    type: 'line',
                    data: {
                        labels: completedSessions.map(s => `Session ${s.sessionId}`),
                        datasets: [{
                            label: 'Βαθμολογία',
                            data: completedSessions.map(s => s.score),
                            borderColor: 'rgb(75, 192, 192)',
                            backgroundColor: 'rgba(75, 192, 192, 0.2)',
                            tension: 0.1
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            title: {
                                display: true,
                                text: 'Εξέλιξη Βαθμολογίας'
                            }
                        },
                        scales: {
                            x: {
                                ticks: {
                                    maxRotation: 45,
                                    minRotation: 45
                                }
                            }
                        }
                    }
                });
            }
        }

        // 2. Energy Distribution
        const energyRanges = { 'Χαμηλή (0-50)': 0, 'Μεσαία (51-100)': 0, 'Υψηλή (101+)': 0 };
        completedSessions.forEach(session => {
            if (session.wisdomEnergy <= 50) energyRanges['Χαμηλή (0-50)']++;
            else if (session.wisdomEnergy <= 100) energyRanges['Μεσαία (51-100)']++;
            else energyRanges['Υψηλή (101+)']++;
        });

        const ctx2 = document.getElementById('energyChart');
        if (ctx2) {
            chartInstances.energy = new Chart(ctx2, {
                type: 'pie',
                data: {
                    labels: Object.keys(energyRanges),
                    datasets: [{
                        data: Object.values(energyRanges),
                        backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56']
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Κατανομή Ενέργειας'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        }

        // 3. Score vs Energy Comparison
        const ctx3 = document.getElementById('comparisonChart');
        if (ctx3) {
            chartInstances.comparison = new Chart(ctx3, {
                type: 'bar',
                data: {
                    labels: completedSessions.map(s => `Session ${s.sessionId}`),
                    datasets: [{
                        label: 'Βαθμολογία',
                        data: completedSessions.map(s => s.score),
                        backgroundColor: 'rgba(54, 162, 235, 0.5)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    }, {
                        label: 'Ενέργεια',
                        data: completedSessions.map(s => s.wisdomEnergy),
                        backgroundColor: 'rgba(255, 99, 132, 0.5)',
                        borderColor: 'rgba(255, 99, 132, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Σύγκριση Score vs Energy'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        },
                        x: {
                            ticks: {
                                maxRotation: 45,
                                minRotation: 45
                            }
                        }
                    }
                }
            });
        }

        // 4. Score Distribution
        const scoreRanges = { 'Χαμηλό (0-100)': 0, 'Μεσαίο (101-200)': 0, 'Υψηλό (201+)': 0 };
        completedSessions.forEach(session => {
            if (session.score <= 100) scoreRanges['Χαμηλό (0-100)']++;
            else if (session.score <= 200) scoreRanges['Μεσαίο (101-200)']++;
            else scoreRanges['Υψηλό (201+)']++;
        });

        const ctx4 = document.getElementById('scoreDistChart');
        if (ctx4) {
            chartInstances.scoreDist = new Chart(ctx4, {
                type: 'doughnut',
                data: {
                    labels: Object.keys(scoreRanges),
                    datasets: [{
                        data: Object.values(scoreRanges),
                        backgroundColor: ['#FF6B6B', '#4ECDC4', '#45B7D1']
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Κατανομή Βαθμολογιών'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        }
    }, 100);
}

// Global game state with local storage for complete gameplay
let gameState = {
    sessionId: null,
    playerName: null,
    startTime: null,
    timerInterval: null,
    wisdomEnergy: 50, // Starting energy
    score: 0,
    currentTrial: 'start'
};

// Local game session storage - collects all data before final submission
let localGameSession = {
    sessionId: null,
    playerChoices: [],
    startTime: null,
    endTime: null,
    gameMetrics: {
        totalWisdomEnergyFromBonus: 0,
        patienceWaitTime: 0,
        totalGameDuration: 0
    }
};

// Helper function to record player choices locally
function recordPlayerChoice(trialType, choice, additionalData = {}) {
    localGameSession.playerChoices.push({
        trialType: trialType,
        choice: choice,
        timestamp: Date.now(),
        gameStateAtTime: {
            wisdomEnergy: gameState.wisdomEnergy,
            score: gameState.score,
            currentTrial: gameState.currentTrial
        },
        ...additionalData
    });
    
    displayResult(`📝 Επιλογή καταγράφηκε τοπικά: ${trialType}`, 'info');
}

// Timer functions
function startGameTimer() {
    // Ensure we have a valid start time
    if (!gameState.startTime) {
        gameState.startTime = Date.now();
    }
    document.getElementById('gameTimer').style.display = 'block';
    
    gameState.timerInterval = setInterval(() => {
        if (gameState.startTime) {
            const elapsed = Date.now() - gameState.startTime;
            const minutes = Math.floor(elapsed / 60000);
            const seconds = Math.floor((elapsed % 60000) / 1000);
            
            // Extra safety check for NaN
            const displayMinutes = isNaN(minutes) ? 0 : minutes;
            const displaySeconds = isNaN(seconds) ? 0 : seconds;
            
            document.getElementById('timerDisplay').textContent = 
                `${displayMinutes.toString().padStart(2, '0')}:${displaySeconds.toString().padStart(2, '0')}`;
        }
    }, 1000);
}

function stopGameTimer() {
    if (gameState.timerInterval) {
        clearInterval(gameState.timerInterval);
        gameState.timerInterval = null;
    }
    document.getElementById('gameTimer').style.display = 'none';
}

function getGameDuration() {
    if (!gameState.startTime) return 0;
    return Math.floor((Date.now() - gameState.startTime) / 1000);
}

// Utility function to display results
function displayResult(message, type = 'info') {
    const resultsDiv = document.getElementById('results');
    const resultElement = document.createElement('div');
    resultElement.className = `result ${type}`;
    resultElement.innerHTML = `<strong>${new Date().toLocaleTimeString()}:</strong> ${message}`;
    resultsDiv.appendChild(resultElement);
    resultsDiv.scrollTop = resultsDiv.scrollHeight;
}

// Start a new game with local session initialization
async function startGame() {
    if (!authToken) {
        showResult('Παρακαλώ κάντε login πρώτα!', 'error');
        return;
    }

    try {
        const url = `${API_BASE_URL}/game/start`;
        
        console.log(`🚀 STARTING GAME START API CALL`);
        logAPICall('POST', url, null, null, 'PENDING');
        
        const response = await authenticatedFetch(url, {
            method: 'POST'
        });

        if (!response.ok) {
            if (response.status === 401) {
                console.error(`❌ GAME START FAILED - 401 Unauthorized`);
                showResult('Η συνεδρία σας έχει λήξει. Παρακαλώ κάντε login ξανά.', 'error');
                logout();
                return;
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        // Log the complete response
        logAPICall('POST', url, null, data, response.status);
        
        // Initialize game state
        gameState.sessionId = data.sessionId;
        gameState.playerName = currentUser;
        gameState.startTime = Date.now(); // Use local time for precision
        gameState.wisdomEnergy = CONFIG.GAME.STARTING_ENERGY; // Starting energy
        gameState.score = 0;
        gameState.currentTrial = 'start';

        // Initialize local session tracking
        localGameSession = {
            sessionId: data.sessionId,
            playerChoices: [],
            startTime: Date.now(),
            endTime: null,
            gameMetrics: {
                totalWisdomEnergyFromBonus: 0,
                patienceWaitTime: 0,
                totalGameDuration: 0
            }
        };

        console.log(`✅ GAME START SUCCESS - SessionId: ${data.sessionId}, Local tracking initialized`);
        console.log(`📊 Initial Game State:`, gameState);
        console.log(`💾 Local Session:`, localGameSession);
        
        showResult(`🎮 Παιχνίδι ξεκίνησε! Session ID: ${data.sessionId}`, 'success');
        showResult(`💾 Τοπική καταγραφή ενεργοποιημένη - δεδομένα θα σταλούν στο τέλος`, 'info');
        
        // Start the game timer
        startGameTimer();
        
        updateGameStateDisplay(gameState);
        showCurrentTrialSection(gameState.currentTrial);

    } catch (error) {
        console.error('🔥 GAME START ERROR:', error);
        displayResult(`Σφάλμα έναρξης παιχνιδιού: ${error.message}`, 'error');
        console.error('Error starting game:', error);
    }
}

// Get current game state (LOCAL VERSION - NO SERVER CALL)
async function getGameState() {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    // Use local game state instead of server call
    updateGameStateDisplay(gameState);
    showCurrentTrialSection(gameState.currentTrial);
    
    displayResult(`🎯 Local State: ${gameState.currentTrial}, Score: ${gameState.score}, Ενέργεια: ${gameState.wisdomEnergy}`, 'info');
}

// Update the game state display
function updateGameStateDisplay(data) {
    document.getElementById('sessionId').textContent = data.sessionId || '-';
    document.getElementById('playerName').textContent = data.username || '-';
    document.getElementById('wisdomEnergy').textContent = data.wisdomEnergy || '-';
    document.getElementById('score').textContent = data.score || '-';
    document.getElementById('currentTrial').textContent = data.currentTrial || '-';
    
    document.getElementById('gameState').style.display = 'block';
}

// Show the appropriate trial section
function showCurrentTrialSection(currentTrial) {
    // Hide all sections first
    document.getElementById('patienceSection').style.display = 'none';
    document.getElementById('resourceSection').style.display = 'none';
    document.getElementById('riskSection').style.display = 'none';
    document.getElementById('endSection').style.display = 'none';

    // Show the appropriate section
    switch (currentTrial) {
        case 'start':
            document.getElementById('patienceSection').style.display = 'block';
            // Reset patience buttons to blurred state
            const btn1 = document.getElementById('patienceBtn1');
            const btn2 = document.getElementById('patienceBtn2');
            const btn3 = document.getElementById('patienceBtn3');
            
            [btn1, btn2, btn3].forEach((btn, index) => {
                if (btn) {
                    btn.classList.add('blurred');
                    btn.classList.remove('clear', 'correct', 'incorrect');
                    btn.style.filter = 'blur(8px)';
                    btn.style.opacity = '0.4';
                    
                    // Reset button content with hints
                    const hintSymbols = ['❌', '❌', '✓'];
                    btn.innerHTML = `Καθρέφτης ${index + 1} <span class="hint" id="hint${index + 1}" style="opacity: 0;">(${hintSymbols[index]})</span>`;
                }
            });
            
            // Start patience timer
            startPatienceTimer();
            break;
        case 'resource':
            document.getElementById('resourceSection').style.display = 'block';
            
            // Start directly with patience panel (no initial choice)
            startResourceTrialDirectly();
            break;
        case 'risk':
            // Skip the old riskSection with small buttons, go directly to completed
            // The pathChoicePhase handles the risk selection with beautiful UI
            displayResult('⏭️ Μετάβαση στο τέλος του παιχνιδιού...', 'info');
            showCurrentTrialSection('completed');
            break;
        case 'completed':
            document.getElementById('endSection').style.display = 'block';
            
            // Stop wisdom bonus when game is completed
            if (wisdomEnergyBonus) {
                clearInterval(wisdomEnergyBonus);
                wisdomEnergyBonus = null;
                displayResult('🏁 Παιχνίδι ολοκληρώθηκε! Το bonus σοφίας σταμάτησε.', 'info');
            }
            break;
    }
}

// Patience trial state
let patienceState = {
    startTime: null,
    timerInterval: null,
    buttonsCleared: false,
    hasWaitedEnough: false
};

// Start patience trial
function startPatienceTimer() {
    patienceState.startTime = Date.now();
    patienceState.buttonsCleared = false;
    patienceState.hasWaitedEnough = false;
    
    let timeLeft = 60;
    document.getElementById('patienceTimeLeft').textContent = timeLeft;
    
    // Initialize blur levels
    updateButtonBlur(timeLeft);
    
    patienceState.timerInterval = setInterval(() => {
        timeLeft--;
        document.getElementById('patienceTimeLeft').textContent = timeLeft;
        
        // Update blur every second (progressive clearing)
        updateButtonBlur(timeLeft);
        
        // After 30 seconds, mark as waited enough for bonus
        if (timeLeft === 30 && !patienceState.hasWaitedEnough) {
            patienceState.hasWaitedEnough = true;
            document.querySelector('.patience-timer').innerHTML += 
                '<br><span style="color: #27ae60; font-weight: bold;">✓ Μπόνους υπομονής ξεκλειδώθηκε! (+50 πόντοι)</span>';
        }
        
        // When timer reaches 0, clear completely and show symbols
        if (timeLeft <= 0 && !patienceState.buttonsCleared) {
            clearPatienceButtons();
        }
    }, 1000);
}

function updateButtonBlur(timeLeft) {
    const buttons = [
        document.getElementById('patienceBtn1'),
        document.getElementById('patienceBtn2'),
        document.getElementById('patienceBtn3')
    ];
    
    const hints = [
        document.getElementById('hint1'),
        document.getElementById('hint2'),
        document.getElementById('hint3')
    ];
    
    // Calculate blur level: starts at 8px, reduces gradually to 0
    const maxBlur = 8;
    const blurLevel = Math.max(0, (timeLeft / 60) * maxBlur);
    
    buttons.forEach(btn => {
        if (btn) {
            btn.style.filter = `blur(${blurLevel}px)`;
            btn.style.opacity = Math.max(0.4, 1 - (timeLeft / 60) * 0.4); // Increases from 0.6 to 1.0
        }
    });
    
    // Show hints progressively (start showing after 40 seconds, fully visible after 20 seconds)
    const hintOpacity = Math.max(0, Math.min(1, (40 - timeLeft) / 20));
    hints.forEach(hint => {
        if (hint) {
            hint.style.opacity = hintOpacity;
        }
    });
}

function clearPatienceButtons() {
    patienceState.buttonsCleared = true;
    
    // Clear interval
    if (patienceState.timerInterval) {
        clearInterval(patienceState.timerInterval);
    }
    
    // Set final symbols and styles
    const btn1 = document.getElementById('patienceBtn1');
    const btn2 = document.getElementById('patienceBtn2');
    const btn3 = document.getElementById('patienceBtn3');
    
    btn1.style.filter = 'none';
    btn1.classList.remove('blurred');
    btn1.classList.add('clear', 'incorrect');
    btn1.innerHTML = '❌';
    btn1.style.opacity = '1';
    
    btn2.style.filter = 'none';
    btn2.classList.remove('blurred');
    btn2.classList.add('clear', 'incorrect');
    btn2.innerHTML = '❌';
    btn2.style.opacity = '1';
    
    btn3.style.filter = 'none';
    btn3.classList.remove('blurred');
    btn3.classList.add('clear', 'correct');
    btn3.innerHTML = '✓';
    btn3.style.opacity = '1';
    
    document.getElementById('patienceTimer').innerHTML = 
        '<span style="color: #27ae60;">⏰ Τα κουμπιά είναι τώρα crystal clear! Επέλεξε σωστά.</span>';
}

// Submit patience trial choice (LOCAL STORAGE - NO SERVER CALL)
async function submitPatienceChoice(choice) {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    // Stop patience timer
    if (patienceState.timerInterval) {
        clearInterval(patienceState.timerInterval);
    }
    
    // Calculate waiting time
    const waitedTime = patienceState.startTime ? 
        Math.floor((Date.now() - patienceState.startTime) / 1000) : 0;
    
    // Store metrics
    localGameSession.gameMetrics.patienceWaitTime = waitedTime;
    
    // Local calculation of results (instead of server call)
    let isCorrect = false;
    let energyChange = 0;
    let scoreChange = 0;
    let result = '';
    
    if (choice === 3) { // Correct choice
        isCorrect = true;
        energyChange = 25;
        scoreChange = 100;
        result = 'Σωστή επιλογή! Η αντανάκλασή σας είναι καθαρή.';
        
        // Bonus for patience
        if (patienceState.hasWaitedEnough) {
            energyChange += 50;
            result += ' Μπόνους υπομονής: +50 ενέργεια!';
        }
    } else {
        isCorrect = false;
        energyChange = -25;
        scoreChange = 0;
        result = `Λάθος επιλογή (το σωστό ήταν το 3). Η αντανάκλαση παραμορφώθηκε.`;
    }
    
    // Update local game state
    gameState.wisdomEnergy += energyChange;
    gameState.score += scoreChange;
    gameState.currentTrial = 'resource';
    
    // Record choice locally instead of sending to server
    recordPlayerChoice('patience', choice, {
        waitedSeconds: waitedTime,
        hasWaitedEnough: patienceState.hasWaitedEnough,
        isCorrect: isCorrect,
        energyChange: energyChange,
        scoreChange: scoreChange,
        result: result
    });
    
    let message = `Επιλογή: Καθρέφτης ${choice}. ${result} Περιμένατε: ${waitedTime} δευτερόλεπτα.`;
    displayResult(message, isCorrect ? 'success' : 'error');
    
    // Update display with local state
    updateGameStateDisplay(gameState);
    
    // Continue to next trial
    showCurrentTrialSection(gameState.currentTrial);
    
    // Hide patience section
    document.getElementById('patienceSection').style.display = 'none';
    
    // Reset patience state
    patienceState = {
        startTime: null,
        timerInterval: null,
        buttonsCleared: false,
        hasWaitedEnough: false
    };
    
    // Reset button styles
    [1, 2, 3].forEach(i => {
        const btn = document.getElementById(`patienceBtn${i}`);
        const hint = document.getElementById(`hint${i}`);
        if (btn) {
            btn.style.filter = '';
            btn.style.opacity = '';
            btn.classList.remove('blurred', 'clear', 'correct', 'incorrect');
        }
        if (hint) {
            hint.style.opacity = '0';
        }
    });
}

// Resource Trial Variables  
let patienceTimer = null;
let wisdomEnergyBonus = null;

// Start resource trial directly with patience panel (after mirrors)
function startResourceTrialDirectly() {
    // Hide all phases initially
    document.getElementById('patiencePhase').style.display = 'none';
    document.getElementById('patiencePanel').style.display = 'block';
    document.getElementById('olivePhase').style.display = 'none';
    document.getElementById('pathChoicePhase').style.display = 'none';
    
    // Initialize patience grid
    initializePatienceGrid();
    
    // Start the patience sequence
    startPatienceSequence();
    
    // DON'T start wisdom bonus here - only after choosing "Επένδυση ενέργειας"
    displayResult('🧘‍♀️ Περίοδος αναμονής! Περιμένετε για να φτάσετε στην επιλογή της ελιάς.', 'success');
}

// Initialize the 5x5 patience grid
function initializePatienceGrid() {
    const grid = document.getElementById('patienceGrid');
    grid.innerHTML = '';
    
    for (let i = 0; i < 25; i++) {
        const square = document.createElement('div');
        square.className = 'patience-square';
        square.id = `square-${i}`;
        square.onclick = () => selectPatienceSquare(i);
        grid.appendChild(square);
    }
}

// Start the 5-second patience sequence
function startPatienceSequence() {
    let currentSquare = 0;
    const totalSquares = 25;
    const intervalMs = 5000 / totalSquares; // 5 seconds divided by 25 squares
    
    patienceTimer = setInterval(() => {
        if (currentSquare < totalSquares) {
            const square = document.getElementById(`square-${currentSquare}`);
            square.classList.add('active');
            square.innerHTML = '✨';
            currentSquare++;
            
            // Update status
            const statusEl = document.getElementById('patienceStatus');
            statusEl.textContent = `Πρόοδος: ${currentSquare}/${totalSquares}`;
            
            if (currentSquare === totalSquares) {
                // All squares are ready, show the special square to click
                const randomSquare = Math.floor(Math.random() * totalSquares);
                const specialSquare = document.getElementById(`square-${randomSquare}`);
                specialSquare.style.background = '#ffd700';
                specialSquare.innerHTML = '🌟';
                specialSquare.style.animation = 'pulse 0.5s infinite';
                
                statusEl.textContent = 'Κάντε κλικ στο χρυσό τετράγωνο για να συνεχίσετε!';
            }
        }
    }, intervalMs);
}

// Handle patience square selection
function selectPatienceSquare(index) {
    const square = document.getElementById(`square-${index}`);
    
    if (square.style.background === 'rgb(255, 215, 0)' || square.style.background === '#ffd700') {
        // Correct square clicked
        clearInterval(patienceTimer);
        
        // Hide patience panel, go directly to olive planting phase
        document.getElementById('patiencePanel').style.display = 'none';
        document.getElementById('olivePhase').style.display = 'block';
        
        displayResult('🌟 Τέλεια! Η υπομονή σας ανταμείφθηκε. Τώρα φυτεύετε την ελιά.', 'success');
    }
}

// Start wisdom energy bonus (configurable formula) with local tracking
function startWisdomEnergyBonus() {
    if (wisdomEnergyBonus) {
        clearInterval(wisdomEnergyBonus);
    }
    
    const formula = CONFIG.GAME.OLIVE_INVESTMENT_FORMULA;
    displayResult(`🧘‍♀️ Ξεκίνησε το bonus σοφίας με ${formula} formula!`, 'success');
    displayResult('⏱️ Τα βόνους δίνονται σε ακέραιες τιμές - περίμενε για milestones!', 'info');
    
    const startTime = Date.now();
    const initialWisdomEnergy = gameState.wisdomEnergy || CONFIG.GAME.STARTING_ENERGY;
    let lastBonusShown = 0;
    
    wisdomEnergyBonus = setInterval(() => {
        if (gameState.sessionId) {
            const elapsedSeconds = Math.floor((Date.now() - startTime) / 1000);
            
            // Calculate bonus using configurable formula
            const currentBonusInteger = GAME_UTILS.calculateEnergyBonus(elapsedSeconds);
            
            // Only update energy when we reach a new integer value
            if (currentBonusInteger > lastBonusShown) {
                const energyToAdd = currentBonusInteger - lastBonusShown;
                gameState.wisdomEnergy = initialWisdomEnergy + currentBonusInteger;
                
                // Ensure we don't exceed max energy
                if (gameState.wisdomEnergy > CONFIG.GAME.MAX_ENERGY) {
                    gameState.wisdomEnergy = CONFIG.GAME.MAX_ENERGY;
                }
                
                // Track total bonus for final submission
                localGameSession.gameMetrics.totalWisdomEnergyFromBonus = currentBonusInteger;
                
                // Update the display
                updateGameStateDisplay(gameState);
                
                // Show feedback when bonus increases
                displayResult(`🧘‍♀️ Bonus σοφίας: +${energyToAdd} ενέργεια! (Συνολικά: +${currentBonusInteger}) [${elapsedSeconds}δευτ]`, 'success');
                
                lastBonusShown = currentBonusInteger;
                
                // Show next milestone info
                const nextMilestone = GAME_UTILS.getNextMilestone(elapsedSeconds);
                if (nextMilestone) {
                    displayResult(`⏳ Επόμενο bonus σε ${nextMilestone.timeToWait} δευτερόλεπτα (+${nextMilestone.bonus - currentBonusInteger})`, 'info');
                }
            }
            
            // Update investment status every few seconds
            if (elapsedSeconds % 3 === 0) { // Update every 3 seconds
                const statusElement = document.getElementById('investmentStatus');
                if (statusElement) {
                    const nextMilestone = GAME_UTILS.getNextMilestone(elapsedSeconds);
                    const statusText = nextMilestone ? 
                        `⚡ Τρέχουσα ενέργεια: +${currentBonusInteger} | Επόμενο bonus σε ${nextMilestone.timeToWait}δευτ` :
                        `⚡ Τρέχουσα ενέργεια: +${currentBonusInteger} | Μπορείτε να προχωρήσετε`;
                    statusElement.innerHTML = `🧘‍♀️ <strong>Επένδυση σε εξέλιξη...</strong><br>${statusText}`;
                }
            }
            
        } else {
            // Stop bonus if no active game
            clearInterval(wisdomEnergyBonus);
            wisdomEnergyBonus = null;
            displayResult(`🏁 Bonus σοφίας σταμάτησε. Συνολικό bonus: +${lastBonusShown} ενέργεια`, 'info');
        }
    }, CONFIG.GAME.OLIVE_UPDATE_INTERVAL_MS);
}

// Select path (safe/risky)
function selectPath(pathType) {
    // Visual feedback
    document.querySelectorAll('.path-option').forEach(option => {
        option.classList.remove('selected');
    });
    
    const selectedOption = document.querySelector(`.path-option.${pathType}`);
    selectedOption.classList.add('selected');
    
    // After selection, complete resource trial and move to next
    setTimeout(() => {
        const pathName = pathType === 'safe' ? 'Ασφαλές Μονοπάτι' : 'Αβέβαιο Μονοπάτι';
        displayResult(`🛤️ Επιλέξατε: ${pathName}.`, 'success');
        
        // Handle path logic locally based on current wisdom energy
        setTimeout(() => {
            handlePathChoice(pathType);
        }, 1500);
    }, 1000);
}

// Handle path choice with local logic (LOCAL STORAGE - NO SERVER CALL)
function handlePathChoice(pathType) {
    const currentWisdom = gameState.wisdomEnergy || 0;
    let energyChange = 0;
    let result = '';
    let isSuccess = false;
    
    if (pathType === 'safe') {
        // Safe path: always +50
        energyChange = CONFIG.GAME.RISK_SAFE_BONUS;
        result = 'Ασφαλές μονοπάτι - Σταθερό κέρδος!';
        isSuccess = true;
        displayResult(`🛡️ ${result} | +${energyChange} ενέργεια`, 'success');
    } else {
        // Risky path: -100 or +100 randomly
        isSuccess = Math.random() > CONFIG.GAME.RISK_SUCCESS_CHANCE;
        energyChange = isSuccess ? CONFIG.GAME.RISK_RISKY_SUCCESS : CONFIG.GAME.RISK_RISKY_PENALTY;
        result = isSuccess ? 'Ριψοκίνδυνο μονοπάτι - Μεγάλη επιτυχία!' : 'Ριψοκίνδυνο μονοπάτι - Αποτυχία!';
        
        const resultType = isSuccess ? 'success' : 'error';
        displayResult(`⚡ ${result} | ${energyChange > 0 ? '+' : ''}${energyChange} ενέργεια`, resultType);
    }
    
    // Apply the energy change to current wisdom energy (including any bonuses)
    gameState.wisdomEnergy = currentWisdom + energyChange;
    gameState.score += isSuccess ? 150 : 25;
    gameState.currentTrial = 'completed';
    
    // Ensure energy doesn't go below 0
    if (gameState.wisdomEnergy < 0) {
        gameState.wisdomEnergy = 0;
    }
    
    // Record choice locally
    recordPlayerChoice('risk', pathType === 'safe', {
        pathType: pathType,
        energyChange: energyChange,
        scoreChange: isSuccess ? 150 : 25,
        isSuccess: isSuccess,
        result: result,
        finalWisdomEnergy: gameState.wisdomEnergy
    });
    
    // Update the display
    updateGameStateDisplay(gameState);
    
    // Show summary
    displayResult(`📊 Ενέργεια: ${currentWisdom} → ${gameState.wisdomEnergy}`, 'info');
    
    // Stop wisdom bonus when all trials are completed
    if (wisdomEnergyBonus) {
        clearInterval(wisdomEnergyBonus);
        wisdomEnergyBonus = null;
        displayResult('🏁 Όλες οι δοκιμασίες ολοκληρώθηκαν! Το bonus σοφίας σταμάτησε.', 'info');
    }
    
    // Hide path choice and go to end game
    setTimeout(() => {
        document.getElementById('pathChoicePhase').style.display = 'none';
        showCurrentTrialSection('completed');
        
        // Prepare for final submission
        displayResult('📤 Προετοιμασία για αποστολή όλων των δεδομένων στον server...', 'info');
    }, 3000);
}

// Submit resource trial choice (LOCAL STORAGE - NO SERVER CALL)
async function submitResourceChoice(plantOlive) {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    // Local calculation of results (instead of server call)
    const choiceText = plantOlive ? 'Επένδυση ενέργειας' : 'Συλλογή άμεσα';
    let result = '';
    let energyChange = 0;
    let scoreChange = 50; // Points for making any choice

    if (plantOlive) {
        // Investment choice - start wisdom bonus but DON'T advance to next trial
        result = 'Φυτέψατε την ελιά. Κερδίζετε ενέργεια μέχρι να προχωρήσετε!';
        energyChange = 0; // No immediate change, bonus will handle it
        displayResult('🌱💡 Επιλέξατε επένδυση! Το bonus σοφίας ξεκινάει τώρα!', 'success');
        
        // Show the continue section
        document.getElementById('investmentContinueSection').style.display = 'block';
        
        // Start the investment bonus
        startWisdomEnergyBonus();
        
        // Update local game state (but don't change trial yet)
        gameState.wisdomEnergy += energyChange;
        gameState.score += scoreChange;
        // DON'T set gameState.currentTrial = 'risk' yet
        
        // Record choice locally
        recordPlayerChoice('resource', plantOlive, {
            choiceText: choiceText,
            energyChange: energyChange,
            scoreChange: scoreChange,
            result: result,
            startedWisdomBonus: true
        });
        
        displayResult(`Επιλογή: ${choiceText} | ${result}`, 'info');
        updateGameStateDisplay(gameState);
        
        // No auto-advance - wait for user to click "Next"
        
    } else {
        // Immediate collection choice - proceed normally
        energyChange = CONFIG.GAME.OLIVE_IMMEDIATE_BONUS;
        result = 'Πήρατε άμεσο κέρδος ενέργειας.';
        displayResult(`💰 Άμεση εισπραξη! +${CONFIG.GAME.OLIVE_IMMEDIATE_BONUS} μονάδες ενέργειας!`, 'success');
        
        // Update local game state
        gameState.wisdomEnergy += energyChange;
        gameState.score += scoreChange;
        gameState.currentTrial = 'risk';
        
        // Record choice locally
        recordPlayerChoice('resource', plantOlive, {
            choiceText: choiceText,
            energyChange: energyChange,
            scoreChange: scoreChange,
            result: result,
            startedWisdomBonus: false
        });
        
        displayResult(`Επιλογή: ${choiceText} | ${result}`, 'info');
        updateGameStateDisplay(gameState);
        
        // Auto-advance to next trial for immediate choice
        setTimeout(() => {
            proceedToRiskTrial();
        }, 2000);
    }
}

// Proceed to Risk Trial (called when user clicks "Next Stage" or auto-advance)
function proceedToRiskTrial() {
    // Update trial state
    gameState.currentTrial = 'risk';
    
    // Hide olive phase and show path choice
    document.getElementById('olivePhase').style.display = 'none';
    document.getElementById('pathChoicePhase').style.display = 'block';
    displayResult('🛤️ Επιλέξτε το μονοπάτι σας για τη συνέχεια.', 'info');
}

// Proceed to Next Trial (button click handler)
function proceedToNextTrial() {
    displayResult('➡️ Προχωρώντας στο επόμενο στάδιο...', 'info');
    
    // Stop the investment bonus
    if (wisdomEnergyBonus) {
        clearInterval(wisdomEnergyBonus);
        wisdomEnergyBonus = null;
        displayResult('⏹️ Το bonus σοφίας σταμάτησε.', 'info');
    }
    
    // Hide the continue section
    document.getElementById('investmentContinueSection').style.display = 'none';
    
    // Proceed to risk trial
    setTimeout(() => {
        proceedToRiskTrial();
    }, 1000);
}

// Stop Investment (button click handler)
function stopInvestment() {
    if (wisdomEnergyBonus) {
        clearInterval(wisdomEnergyBonus);
        wisdomEnergyBonus = null;
        displayResult('⏹️ Σταματήσατε την επένδυση. Διατηρείτε την ενέργεια που έχετε κερδίσει.', 'success');
        
        // Show proceed button prominently
        document.getElementById('investmentStatus').innerHTML = 
            '<strong>✅ Επένδυση ολοκληρώθηκε.</strong><br>Πατήστε "Επόμενο Στάδιο" για να συνεχίσετε.';
    }
}

// Submit complete game session (NEW BATCH APPROACH)
async function submitCompleteGameSession() {
    if (!gameState.sessionId || !localGameSession.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι για αποστολή!', 'error');
        return false;
    }

    try {
        // Finalize game session data
        localGameSession.endTime = Date.now();
        localGameSession.gameMetrics.totalGameDuration = Math.floor((localGameSession.endTime - localGameSession.startTime) / 1000);
        
        if (wisdomEnergyBonus) {
            localGameSession.gameMetrics.totalWisdomEnergyFromBonus = gameState.wisdomEnergy - 50; // Subtract starting energy
        }

        const completeSessionData = {
            sessionId: gameState.sessionId,
            finalGameState: {
                wisdomEnergy: gameState.wisdomEnergy,
                score: gameState.score,
                currentTrial: gameState.currentTrial
            },
            playerChoices: localGameSession.playerChoices,
            gameMetrics: localGameSession.gameMetrics,
            clientStartTime: new Date(localGameSession.startTime).toISOString(),
            clientEndTime: new Date(localGameSession.endTime).toISOString(),
            totalGameDurationSeconds: localGameSession.gameMetrics.totalGameDuration
        };

        const url = `${API_BASE_URL}/game/submit-complete`;
        
        console.log(`🚀 STARTING GAME END (SUBMIT COMPLETE) API CALL`);
        console.log(`📦 Complete Session Data Prepared:`, completeSessionData);
        logAPICall('POST', url, completeSessionData, null, 'PENDING');
        
        displayResult('📤 Αποστολή πλήρους session στον server...', 'info');
        console.log('Complete session data to submit:', completeSessionData);

        // Use the new batch submission endpoint
        const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(completeSessionData)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        
        // Log the complete response
        logAPICall('POST', url, completeSessionData, result, response.status);
        
        console.log(`✅ GAME END SUCCESS - Final Score: ${result.score}`);
        console.log(`📊 Server Response:`, result);
        
        displayResult('✅ Όλα τα δεδομένα στάλθηκαν επιτυχώς!', 'success');
        displayResult(`📊 Τελικό score: ${result.score}`, 'info');
        
        return true;

    } catch (error) {
        console.error('🔥 GAME END ERROR:', error);
        displayResult(`❌ Σφάλμα αποστολής session: ${error.message}`, 'error');
        console.error('Error submitting complete session:', error);
        displayResult('💾 Δεδομένα διατηρούνται τοπικά για επανάληψη.', 'info');
        return false;
    }
}

// End the game with batch submission of all data
async function endGame() {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    try {
        // Stop the game timer and calculate total duration
        const totalDuration = getGameDuration();
        const minutes = Math.floor(totalDuration / 60);
        const seconds = totalDuration % 60;
        
        stopGameTimer();
        
        // Clear resource trial timers
        if (patienceTimer) {
            clearInterval(patienceTimer);
            patienceTimer = null;
        }
        
        if (wisdomEnergyBonus) {
            clearInterval(wisdomEnergyBonus);
            wisdomEnergyBonus = null;
            displayResult('⏹️ Bonus σοφίας σταμάτησε για τελικό υπολογισμό.', 'info');
        }

        displayResult(`⏱️ Συνολικός χρόνος παιχνιδιού: ${minutes}:${seconds.toString().padStart(2, '0')}`, 'info');
        
        // Submit all game data in a single batch
        const submissionSuccess = await submitCompleteGameSession();
        
        if (submissionSuccess) {
            displayResult(`🎉 Παιχνίδι τερματίστηκε επιτυχώς! Τελική βαθμολογία: ${gameState.score}`, 'success');
            displayResult(`⚡ Τελική ενέργεια σοφίας: ${gameState.wisdomEnergy}`, 'info');
            displayResult(`📊 Συνολικές επιλογές καταγεγραμμένες: ${localGameSession.playerChoices.length}`, 'info');
            
            // Show player dashboard with all sessions
            setTimeout(async () => {
                await loadPlayerSessions();
            }, 2000);
        } else {
            displayResult('⚠️ Σφάλμα στην αποστολή - δοκιμάστε ξανά ή τα δεδομένα θα αποθηκευτούν τοπικά.', 'error');
        }

        // Reset game state after submission attempt
        gameState = {
            sessionId: null,
            playerName: null,
            startTime: null,
            timerInterval: null,
            wisdomEnergy: 50,
            score: 0,
            currentTrial: 'start'
        };

        // Reset local session
        localGameSession = {
            sessionId: null,
            playerChoices: [],
            startTime: null,
            endTime: null,
            gameMetrics: {
                totalWisdomEnergyFromBonus: 0,
                patienceWaitTime: 0,
                totalGameDuration: 0
            }
        };

        // Hide all sections
        document.getElementById('gameState').style.display = 'none';
        showCurrentTrialSection('none');

    } catch (error) {
        displayResult(`❌ Σφάλμα τερματισμού παιχνιδιού: ${error.message}`, 'error');
        console.error('Error ending game:', error);
    }
}

// Initialize page with debug functions
document.addEventListener('DOMContentLoaded', function() {
    displayResult('Frontend έτοιμο! Βεβαιωθείτε ότι το backend τρέχει στο https://localhost:7299', 'info');
    displayResult('🆕 Ενεργοποιημένη νέα αρχιτεκτονική: Local storage + batch submission', 'success');
    
    // Add debug function to global scope
    window.debugGameSession = function() {
        console.log('=== DEBUG: Current Game State ===');
        console.log('gameState:', gameState);
        console.log('localGameSession:', localGameSession);
        console.log('Wisdom bonus active:', wisdomEnergyBonus !== null);
        console.log('Total choices recorded:', localGameSession.playerChoices?.length || 0);
        
        displayResult(`🔧 Debug: ${localGameSession.playerChoices?.length || 0} επιλογές καταγεγραμμένες`, 'info');
        return { gameState, localGameSession };
    };
    
    displayResult('💡 Tip: Χρησιμοποιήστε debugGameSession() στο console για debug info', 'info');
});
