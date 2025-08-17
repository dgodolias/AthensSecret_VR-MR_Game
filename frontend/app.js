// Fetch and display stats for a given session ID
async function fetchPlayerSessions(username) {
    try {
        const response = await fetch(`${API_BASE_URL}/game/player/${encodeURIComponent(username)}/sessions`);
        
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
    const dashboardDiv = document.getElementById('playerDashboard');
    const statsDiv = document.getElementById('playerStats');
    const sessionsDiv = document.getElementById('sessionsList');

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

    // Sessions list
    let sessionsHtml = '<h4>🎮 Ιστορικό Παιχνιδιών</h4>';
    data.sessions.forEach(session => {
        const cardClass = session.isActive ? 'session-active' : 'session-completed';
        const statusText = session.isActive ? 'Ενεργό' : 'Ολοκληρωμένο';
        const endTimeText = session.endTime ? new Date(session.endTime).toLocaleString() : '-';
        
        sessionsHtml += `
            <div class="session-card ${cardClass}">
                <p><strong>Session ID:</strong> ${session.sessionId} <span style="float:right;"><strong>Status:</strong> ${statusText}</span></p>
                <p><strong>Βαθμολογία:</strong> ${session.score} | <strong>Ενέργεια:</strong> ${session.wisdomEnergy} | <strong>Δοκιμασία:</strong> ${session.currentTrial}</p>
                <p><strong>Έναρξη:</strong> ${new Date(session.startTime).toLocaleString()} | <strong>Λήξη:</strong> ${endTimeText}</p>
            </div>
        `;
    });

    sessionsDiv.innerHTML = sessionsHtml;
    dashboardDiv.style.display = 'block';
}
// Base API URL - αλλάξτε αν το backend τρέχει σε διαφορετικό port
const API_BASE_URL = 'http://localhost:5182/api';

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
    const username = document.getElementById('username').value.trim();
    if (!username) {
        displayResult('Παρακαλώ εισάγετε όνομα χρήστη!', 'error');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/game/start`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ Username: username })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        gameState.sessionId = data.sessionId;
        gameState.playerName = data.username;

        displayResult(`Παιχνίδι ξεκίνησε για τον παίκτη: ${username}`, 'success');
        
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
        const response = await fetch(`${API_BASE_URL}/game/${gameState.sessionId}/state`);
        
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
        const response = await fetch(`${API_BASE_URL}/trials/patience`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
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
        const response = await fetch(`${API_BASE_URL}/trials/resource`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
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
        const response = await fetch(`${API_BASE_URL}/trials/risk`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
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
        const response = await fetch(`${API_BASE_URL}/game/${gameState.sessionId}/end`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        displayResult(`Παιχνίδι τερματίστηκε! Τελική βαθμολογία: ${data.score}`, 'success');

        // Show player dashboard with all sessions
        await fetchPlayerSessions(gameState.playerName);

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
