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
    playerName: null
};

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
        updateGameStateDisplay(data);
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
            break;
        case 'resource':
            document.getElementById('resourceSection').style.display = 'block';
            break;
        case 'risk':
            document.getElementById('riskSection').style.display = 'block';
            break;
        case 'completed':
            document.getElementById('endSection').style.display = 'block';
            break;
    }
}

// Submit patience trial choice
async function submitPatienceChoice(choice) {
    if (!gameState.sessionId) {
        displayResult('Δεν υπάρχει ενεργό παιχνίδι!', 'error');
        return;
    }

    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/trials/patience`, {
            method: 'POST',
            body: JSON.stringify({
                SessionId: gameState.sessionId,
                Choice: choice
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        const choiceText = choice === 1 ? 'Καθρέφτης 1' : 
                          choice === 2 ? 'Καθρέφτης 2' : 'Υπομονή';
        
        displayResult(`Επιλογή: ${choiceText} | ${data.result} | Αλλαγή Ενέργειας: ${data.energyChange > 0 ? '+' : ''}${data.energyChange}`, 
                     data.isCorrect ? 'success' : 'error');

        // Update game state
        await getGameState();

    } catch (error) {
        displayResult(`Σφάλμα υποβολής επιλογής: ${error.message}`, 'error');
        console.error('Error submitting patience choice:', error);
    }
}

// Submit resource trial choice
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
        
        const choiceText = plantOlive ? 'Φύτεψα την ελιά' : 'Άμεσο κέρδος';
        
        displayResult(`Επιλογή: ${choiceText} | ${data.result} | Αλλαγή Ενέργειας: ${data.energyChange > 0 ? '+' : ''}${data.energyChange}`, 'info');

        // Update game state
        await getGameState();

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

        // Update game state
        await getGameState();

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
        
        displayResult(`Παιχνίδι τερματίστηκε! Τελική βαθμολογία: ${data.score}`, 'success');

        // Show player dashboard with all sessions
        await loadPlayerSessions();

        // Reset game state after showing dashboard
        gameState.sessionId = null;
        gameState.playerName = null;

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
