// Global variables
let currentSessionId = null;
let currentPlayerData = null;
let chartInstances = {};
let authToken = localStorage.getItem('authToken');
let currentUser = localStorage.getItem('currentUser');

// API Base URL
// API Configuration
const API_BASE_URL = 'http://localhost:5182/api';

// Helper function for authenticated API requests
async function authenticatedFetch(url, options = {}) {
    const token = localStorage.getItem('authToken');
    if (!token) {
        throw new Error('No authentication token found');
    }
    
    return fetch(url, {
        ...options,
        headers: {
            ...options.headers,
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
}

// Check authentication on page load
document.addEventListener('DOMContentLoaded', function() {
    if (authToken && currentUser) {
        showAuthenticatedView();
    }
});

// Authentication Functions
function switchTab(tab) {
    // Switch tab styling
    document.querySelectorAll('.auth-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.auth-form').forEach(f => f.classList.remove('active'));
    
    document.querySelector(`[onclick="switchTab('${tab}')"]`).classList.add('active');
    document.getElementById(tab + 'Form').classList.add('active');
}

async function register() {
    const username = document.getElementById('registerUsername').value.trim();
    const password = document.getElementById('registerPassword').value;
    
    if (!username || !password) {
        showResult('Παρακαλώ συμπληρώστε όλα τα πεδία', 'error');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, password })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            // Store authentication data
            authToken = data.token;
            currentUser = data.username;
            localStorage.setItem('authToken', authToken);
            localStorage.setItem('currentUser', currentUser);
            localStorage.setItem('playerId', data.playerId);
            
            showResult(`Επιτυχής εγγραφή! Καλώς ήρθες, ${data.username}!`, 'success');
            showAuthenticatedView();
        } else {
            showResult(data.message || 'Σφάλμα εγγραφής', 'error');
        }
    } catch (error) {
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
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, password })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            // Store authentication data
            authToken = data.token;
            currentUser = data.username;
            localStorage.setItem('authToken', authToken);
            localStorage.setItem('currentUser', currentUser);
            localStorage.setItem('playerId', data.playerId);
            
            showResult(`Επιτυχής είσοδος! Καλώς ήρθες πίσω, ${data.username}!`, 'success');
            showAuthenticatedView();
        } else {
            showResult(data.message || 'Λάθος στοιχεία', 'error');
        }
    } catch (error) {
        showResult('Σφάλμα σύνδεσης με τον server', 'error');
        console.error('Login error:', error);
    }
}

async function logout() {
    try {
        await fetch(`${API_BASE_URL}/auth/logout`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });
    } catch (error) {
        console.error('Logout error:', error);
    }
    
    // Clear stored data
    authToken = null;
    currentUser = null;
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    localStorage.removeItem('playerId');
    
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
        const response = await authenticatedFetch(`${API_BASE_URL}/game/player/${encodeURIComponent(username)}/sessions`);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        displayPlayerDashboard(data);

    } catch (error) {
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

// Global game state
let gameState = {
    sessionId: null,
    playerName: null,
    startTime: null,
    timerInterval: null,
    wisdomEnergy: 0,
    score: 0,
    currentTrial: null
};

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

// Start a new game
async function startGame() {
    if (!authToken) {
        showResult('Παρακαλώ κάντε login πρώτα!', 'error');
        return;
    }

    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/game/start`, {
            method: 'POST'
        });

        if (!response.ok) {
            if (response.status === 401) {
                showResult('Η συνεδρία σας έχει λήξει. Παρακαλώ κάντε login ξανά.', 'error');
                logout();
                return;
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        gameState.sessionId = data.sessionId;
        gameState.playerName = currentUser;

        showResult(`Παιχνίδι ξεκίνησε! Session ID: ${data.sessionId}`, 'success');
        
        // Start the game timer
        startGameTimer();
        
        updateGameStateDisplay(data);
        showCurrentTrialSection(data.currentTrial);

    } catch (error) {
        displayResult(`Σφάλμα έναρξης παιχνιδιού: ${error.message}`, 'error');
        console.error('Error starting game:', error);
    }
}

// Get current game state
async function getGameState() {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/game/${gameState.sessionId}/state`);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        // Update local gameState with server data, but preserve wisdom energy AND startTime
        const preserveWisdomEnergy = wisdomEnergyBonus !== null;
        const preserveStartTime = gameState.startTime; // Always preserve local startTime
        
        gameState = { 
            ...gameState, 
            ...data,
            // Keep local values
            startTime: preserveStartTime, // Never overwrite the local timer start time
            wisdomEnergy: preserveWisdomEnergy ? gameState.wisdomEnergy : data.wisdomEnergy
        };
        
        updateGameStateDisplay(gameState);
        showCurrentTrialSection(data.currentTrial);

    } catch (error) {
        displayResult(`Σφάλμα ανάκτησης κατάστασης: ${error.message}`, 'error');
        console.error('Error getting game state:', error);
    }
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

// Submit patience trial choice
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
    
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/trials/patience`, {
            method: 'POST',
            body: JSON.stringify({
                SessionId: gameState.sessionId,
                Choice: choice,
                WaitedSeconds: waitedTime,
                HasWaitedEnough: patienceState.hasWaitedEnough
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        
        let message = `Επιλογή: Καθρέφτης ${choice}. `;
        
        if (choice === 3) {
            message += '✓ Σωστή επιλογή! ';
        } else {
            message += `❌ Λάθος επιλογή (το σωστό ήταν το 3). `;
        }
        
        if (patienceState.hasWaitedEnough) {
            message += 'Μπόνους υπομονής: +50 πόντοι! ';
        }
        message += `Περιμένατε: ${waitedTime} δευτερόλεπτα.`;
        
        displayResult(message, choice === 3 ? 'success' : 'error');
        
        // Update game state and continue
        await getGameState();
        
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
        
    } catch (error) {
        console.error('Patience trial error:', error);
        displayResult('Σφάλμα στη δοκιμασία υπομονής: ' + error.message, 'error');
    }
}

// Submit resource trial choice
// Reset resource trial to initial state
function resetResourceTrial() {
    // Clear any running timers
    if (patienceTimer) {
        clearInterval(patienceTimer);
        patienceTimer = null;
    }
    
    if (wisdomEnergyBonus) {
        clearInterval(wisdomEnergyBonus);
        wisdomEnergyBonus = null;
    }
    
    // Reset variables
    patienceChosen = false;
    patienceSquareCount = 0;
    
    // Show ONLY initial choice phase, hide everything else
    document.getElementById('patiencePhase').style.display = 'block';
    document.getElementById('patiencePanel').style.display = 'none';
    document.getElementById('olivePhase').style.display = 'none';
    document.getElementById('pathChoicePhase').style.display = 'none';
    
    // Clear any selected path styling
    document.querySelectorAll('.path-option').forEach(option => {
        option.classList.remove('selected');
    });
}

// Resource Trial Variables
let patienceChosen = false;
let patienceTimer = null;
let wisdomEnergyBonus = null;
let patienceSquareCount = 0;

// Start resource trial directly with patience panel (after mirrors)
function startResourceTrialDirectly() {
    // Hide all phases initially
    document.getElementById('patiencePhase').style.display = 'none';
    document.getElementById('patiencePanel').style.display = 'block';
    document.getElementById('olivePhase').style.display = 'none';
    document.getElementById('pathChoicePhase').style.display = 'none';
    
    // Set patience as chosen (since we skip the choice)
    patienceChosen = true;
    
    // Initialize patience grid
    initializePatienceGrid();
    
    // Start the patience sequence
    startPatienceSequence();
    
    // DON'T start wisdom bonus here - only after choosing "Επένδυση ενέργειας"
    
    displayResult('🧘‍♀️ Περίοδος αναμονής! Περιμένετε για να φτάσετε στην επιλογή της ελιάς.', 'success');
}

// Choose patience path
function choosePatience() {
    patienceChosen = true;
    
    // Hide choice buttons, show patience panel
    document.getElementById('patiencePhase').style.display = 'none';
    document.getElementById('patiencePanel').style.display = 'block';
    
    // Initialize patience grid
    initializePatienceGrid();
    
    // Start the patience sequence
    startPatienceSequence();
    
    // Start wisdom energy bonus (+1 per second from now until game ends)
    startWisdomEnergyBonus();
    
    displayResult('🧘‍♀️ Επιλέξατε την υπομονή! Κερδίζετε +1 ενέργεια σοφίας κάθε δευτερόλεπτο μέχρι το τέλος του παιχνιδιού!', 'success');
}

// Skip patience, go directly to olive choice
function skipPatience() {
    patienceChosen = false;
    
    // Hide choice buttons, show olive phase
    document.getElementById('patiencePhase').style.display = 'none';
    document.getElementById('olivePhase').style.display = 'block';
    
    displayResult('⚡ Επιλέξατε άμεση δράση!', 'info');
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

// Start wisdom energy bonus (1 per second)
function startWisdomEnergyBonus() {
    if (wisdomEnergyBonus) {
        clearInterval(wisdomEnergyBonus);
    }
    
    displayResult('🧘‍♀️ Ξεκίνησε το bonus σοφίας: +1 ενέργεια κάθε δευτερόλεπτο!', 'success');
    
    wisdomEnergyBonus = setInterval(() => {
        if (gameState.sessionId) {
            // Add wisdom energy bonus
            const oldWisdom = gameState.wisdomEnergy || 0;
            gameState.wisdomEnergy = oldWisdom + 1;
            
            // Update the display with current gameState
            updateGameStateDisplay(gameState);
            
            // Show feedback every 10 seconds to avoid spam
            if (gameState.wisdomEnergy % 10 === 0) {
                displayResult(`🧘‍♀️ Bonus σοφίας: ${gameState.wisdomEnergy} (συνεχίζει +1/δευτερόλεπτο)`, 'info');
            }
        } else {
            // Stop bonus if no active game
            clearInterval(wisdomEnergyBonus);
            wisdomEnergyBonus = null;
        }
    }, 1000);
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

// Handle path choice with local logic
function handlePathChoice(pathType) {
    const currentWisdom = gameState.wisdomEnergy || 0;
    let energyChange = 0;
    let result = '';
    
    if (pathType === 'safe') {
        // Safe path: always +50
        energyChange = 50;
        result = 'Ασφαλές μονοπάτι - Σταθερό κέρδος!';
        displayResult(`🛡️ ${result} | +${energyChange} ενέργεια`, 'success');
    } else {
        // Risky path: -100 or +100 randomly
        const isSuccess = Math.random() > 0.5;
        energyChange = isSuccess ? 100 : -100;
        result = isSuccess ? 'Ριψοκίνδυνο μονοπάτι - Μεγάλη επιτυχία!' : 'Ριψοκίνδυνο μονοπάτι - Αποτυχία!';
        
        const resultType = isSuccess ? 'success' : 'error';
        displayResult(`⚡ ${result} | ${energyChange > 0 ? '+' : ''}${energyChange} ενέργεια`, resultType);
    }
    
    // Apply the energy change to current wisdom energy (including any bonuses)
    gameState.wisdomEnergy = currentWisdom + energyChange;
    
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
    }, 3000);
}

async function submitResourceChoice(plantOlive) {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/trials/resource`, {
            method: 'POST',
            body: JSON.stringify({
                SessionId: gameState.sessionId,
                PlantOlive: plantOlive
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        const choiceText = plantOlive ? 'Επένδυση ενέργειας' : 'Συλλογή άμεσα';
        
        displayResult(`Επιλογή: ${choiceText} | ${data.result}`, 'info');

        if (plantOlive) {
            // INVESTMENT CHOICE: Ignore server energy change, just start the +1/sec bonus
            displayResult('🌱💡 Επιλέξατε επένδυση! Κερδίζετε +1 ενέργεια σοφίας κάθε δευτερόλεπτο μέχρι το τέλος!', 'success');
            startWisdomEnergyBonus();
        } else {
            // IMMEDIATE COLLECTION: Override server value, give +50 directly
            gameState.wisdomEnergy += 50;
            updateGameStateDisplay(gameState);
            displayResult('💰 Άμεση εισπραξη! Κερδίζετε +50 μονάδες ενέργειας αμέσως!', 'success');
        }
        
        // After olive choice, show path selection
        setTimeout(() => {
            document.getElementById('olivePhase').style.display = 'none';
            document.getElementById('pathChoicePhase').style.display = 'block';
            displayResult('🛤️ Επιλέξτε το μονοπάτι σας για τη συνέχεια.', 'info');
        }, 2000);

    } catch (error) {
        displayResult(`Σφάλμα υποβολής επιλογής: ${error.message}`, 'error');
        console.error('Error submitting resource choice:', error);
    }
}

// Submit risk trial choice
async function submitRiskChoice(chooseSafePath) {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/trials/risk`, {
            method: 'POST',
            body: JSON.stringify({
                SessionId: gameState.sessionId,
                ChooseSafePath: chooseSafePath
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        const choiceText = chooseSafePath ? 'Ασφαλές μονοπάτι' : 'Αβέβαιο μονοπάτι';
        
        displayResult(`Επιλογή: ${choiceText} | ${data.result} | Αλλαγή Ενέργειας: ${data.energyChange > 0 ? '+' : ''}${data.energyChange}`, 
                     data.isSuccess ? 'success' : 'error');

        // Stop wisdom bonus when risk trial is completed
        if (wisdomEnergyBonus) {
            clearInterval(wisdomEnergyBonus);
            wisdomEnergyBonus = null;
            displayResult('🏁 Όλες οι δοκιμασίες ολοκληρώθηκαν! Το bonus σοφίας σταμάτησε.', 'info');
        }

        // Wait a bit then update game state to show end screen
        setTimeout(async () => {
            await getGameState();
            
            // If still on risk (server hasn't updated yet), manually show end section
            if (gameState.currentTrial === 'risk') {
                displayResult('🎯 Δοκιμασία ρίσκου ολοκληρώθηκε! Μετάβαση στην οθόνη ολοκλήρωσης...', 'success');
                showCurrentTrialSection('completed');
            }
        }, 2000);

    } catch (error) {
        displayResult(`Σφάλμα υποβολής επιλογής: ${error.message}`, 'error');
        console.error('Error submitting risk choice:', error);
    }
}

// End the game
async function endGame() {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/game/${gameState.sessionId}/end`, {
            method: 'POST'
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        // Stop the game timer and show total time
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
        }
        
        displayResult(`Παιχνίδι τερματίστηκε! Τελική βαθμολογία: ${data.score} | Συνολικός χρόνος: ${minutes}:${seconds.toString().padStart(2, '0')}`, 'success');

        // Show player dashboard with all sessions
        await loadPlayerSessions();

        // Reset game state after showing dashboard
        gameState.sessionId = null;
        gameState.playerName = null;
        gameState.startTime = null;

        // Hide all sections
        document.getElementById('gameState').style.display = 'none';
        showCurrentTrialSection('none');

    } catch (error) {
        displayResult(`Σφάλμα τερματισμού παιχνιδιού: ${error.message}`, 'error');
        console.error('Error ending game:', error);
    }
}

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
    displayResult('Frontend έτοιμο! Βεβαιωθείτε ότι το backend τρέχει στο https://localhost:7299', 'info');
});
