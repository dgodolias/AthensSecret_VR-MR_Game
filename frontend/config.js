// 🎮 VR Game Configuration
// Centralized configuration for easy server management

const CONFIG = {
    // 🌐 API Configuration
    API: {
        // Change this URL to point to your deployed server
        BASE_URL: 'http://localhost:5182/api',
        
        // Alternative URLs for different environments
        ENVIRONMENTS: {
            LOCAL: 'http://localhost:5182/api',
            RAILWAY: 'https://your-app-name.up.railway.app/api',
            AZURE: 'https://your-app-name.azurewebsites.net/api',
            HEROKU: 'https://your-app-name.herokuapp.com/api'
        },
        
        // Request timeouts (milliseconds)
        TIMEOUT: 30000, // 30 seconds
        
        // Retry configuration
        MAX_RETRIES: 3,
        RETRY_DELAY: 1000 // 1 second
    },
    
    // 🎲 Game Mechanics
    GAME: {
        // Energy system
        STARTING_ENERGY: 50,
        MIN_ENERGY: 0, // Minimum energy level
        MAX_ENERGY: 1000, // Maximum energy cap
        
        // Patience trial settings  
        PATIENCE_TIMER_SECONDS: 60,
        PATIENCE_BONUS_THRESHOLD: 30, // seconds to wait for bonus
        PATIENCE_BONUS_AMOUNT: 50,
        PATIENCE_BONUS_SCORE: 50,
        
        // Resource management (Olive Tree Investment)
        OLIVE_IMMEDIATE_BONUS: 50,
        OLIVE_INVESTMENT_FORMULA: 'sqrt', // Options: 'linear', 'sqrt', 'log', 'quadratic'
        OLIVE_INVESTMENT_MULTIPLIER: 1, // For formula scaling
        OLIVE_INVESTMENT_CAP: 100, // Maximum bonus from investment
        OLIVE_UPDATE_INTERVAL_MS: 1000, // How often to check for bonus updates
        
        // Alternative formulas for olive investment
        FORMULAS: {
            'linear': (seconds) => seconds, // 1 energy per second
            'sqrt': (seconds) => Math.floor(Math.sqrt(seconds)), // √x formula
            'log': (seconds) => Math.floor(Math.log2(seconds + 1)), // Log₂(x+1)
            'quadratic': (seconds) => Math.floor(Math.pow(seconds, 0.5) * 1.5) // Custom scaling
        },
        
        // Risk trial settings
        RISK_SAFE_BONUS: 50,
        RISK_RISKY_SUCCESS: 100,
        RISK_RISKY_PENALTY: -100,
        RISK_SUCCESS_CHANCE: 0.6, // 60% chance of success for risky path
        RISK_SAFE_SCORE: 100,
        RISK_RISKY_SUCCESS_SCORE: 150,
        RISK_RISKY_FAIL_SCORE: 25,
        
        // Scoring system
        SCORE_MULTIPLIERS: {
            PATIENCE_COMPLETED: 1.0,
            PATIENCE_WITH_BONUS: 1.5,
            RESOURCE_IMMEDIATE: 1.0,
            RESOURCE_INVESTMENT: 1.2,
            RISK_SAFE: 1.0,
            RISK_RISKY_SUCCESS: 2.0,
            RISK_RISKY_FAIL: 0.5
        },
        
        // Game flow timing
        TRIAL_TRANSITION_DELAY: 2000, // ms between trials
        RESULT_DISPLAY_DURATION: 3000, // ms to show results
        AUTO_ADVANCE_TRIALS: false, // Investment requires manual advance
        MANUAL_ADVANCE_FOR_INVESTMENT: true, // User must click "Next" for investment
        INVESTMENT_STATUS_UPDATE_INTERVAL: 3000, // ms between status updates
        
        // Performance and limits
        MAX_GAME_DURATION_MINUTES: 30,
        SESSION_TIMEOUT_MINUTES: 60,
        MAX_RETRIES_PER_TRIAL: 3
    },
    
    // 🎨 UI Settings
    UI: {
        ANIMATION_SPEED: 300, // milliseconds
        NOTIFICATION_DURATION: 3000, // 3 seconds
        ENABLE_CONSOLE_LOGGING: true,
        THEME: 'default', // Options: 'default', 'dark', 'vr', 'accessible'
        FONT_SIZE: 'medium', // Options: 'small', 'medium', 'large'
        ENABLE_SOUND_EFFECTS: true,
        ENABLE_HAPTIC_FEEDBACK: true, // For VR controllers
        LANGUAGE: 'el', // Greek by default
        
        // Progress indicators
        SHOW_PROGRESS_BAR: true,
        SHOW_TIMER_COUNTDOWN: true,
        SHOW_ENERGY_ANIMATIONS: true,
        SHOW_SCORE_ANIMATIONS: true,
        
        // Accessibility
        HIGH_CONTRAST_MODE: false,
        SCREEN_READER_SUPPORT: true,
        KEYBOARD_NAVIGATION: true
    },
    
    // 🔧 Debug Settings
    DEBUG: {
        ENABLE_API_LOGGING: true,
        ENABLE_GAME_STATE_LOGGING: true,
        MOCK_API_RESPONSES: false,
        SKIP_TRIALS: false, // For testing - skip directly to end
        FORCE_TRIAL_OUTCOMES: null, // For testing specific scenarios
        LOG_PERFORMANCE_METRICS: true,
        ENABLE_DEV_SHORTCUTS: false, // Keyboard shortcuts for testing
        VERBOSE_CONSOLE_LOGS: true
    },
};

// 🚀 Environment Detection and Auto-Configuration
(function initializeConfig() {
    // Detect if running in development
    const isLocalhost = window.location.hostname === 'localhost' || 
                       window.location.hostname === '127.0.0.1';
    
    if (isLocalhost) {
        console.log('🏠 Running in LOCAL environment');
    } else {
        console.log('🌐 Running in PRODUCTION environment');
        // You can auto-detect production URL here if needed
    }
    
    // Log current configuration
    if (CONFIG.DEBUG.ENABLE_API_LOGGING) {
        console.log('⚙️ Game Configuration Loaded:', CONFIG);
        console.log(`🔗 API Base URL: ${CONFIG.API.BASE_URL}`);
        console.log(`📊 Olive Formula: ${CONFIG.GAME.OLIVE_INVESTMENT_FORMULA}`);
    }
})();

// 🧮 Helper Functions for Game Mechanics
const GAME_UTILS = {
    // Calculate energy bonus based on selected formula
    calculateEnergyBonus: function(seconds) {
        const formula = CONFIG.GAME.OLIVE_INVESTMENT_FORMULA;
        const multiplier = CONFIG.GAME.OLIVE_INVESTMENT_MULTIPLIER;
        const cap = CONFIG.GAME.OLIVE_INVESTMENT_CAP;
        
        let result = 0;
        if (CONFIG.GAME.FORMULAS[formula]) {
            result = CONFIG.GAME.FORMULAS[formula](seconds) * multiplier;
        } else {
            console.warn(`Unknown formula: ${formula}, defaulting to sqrt`);
            result = Math.floor(Math.sqrt(seconds)) * multiplier;
        }
        
        return Math.min(result, cap);
    },
    
    // Calculate next milestone for current formula
    getNextMilestone: function(currentSeconds) {
        const formula = CONFIG.GAME.OLIVE_INVESTMENT_FORMULA;
        const currentBonus = this.calculateEnergyBonus(currentSeconds);
        
        // Find next second where bonus increases
        for (let sec = currentSeconds + 1; sec <= currentSeconds + 100; sec++) {
            if (this.calculateEnergyBonus(sec) > currentBonus) {
                return {
                    seconds: sec,
                    bonus: this.calculateEnergyBonus(sec),
                    timeToWait: sec - currentSeconds
                };
            }
        }
        return null; // No milestone found in next 100 seconds
    },
    
    // Validate configuration
    validateConfig: function() {
        const errors = [];
        
        if (!CONFIG.API.BASE_URL) errors.push('API Base URL is required');
        if (CONFIG.GAME.STARTING_ENERGY < 0) errors.push('Starting energy cannot be negative');
        if (CONFIG.GAME.RISK_SUCCESS_CHANCE < 0 || CONFIG.GAME.RISK_SUCCESS_CHANCE > 1) {
            errors.push('Risk success chance must be between 0 and 1');
        }
        
        if (errors.length > 0) {
            console.error('⚠️ Configuration Errors:', errors);
            return false;
        }
        
        console.log('✅ Configuration validation passed');
        return true;
    }
};

// Export configuration for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CONFIG;
}
