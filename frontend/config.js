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
        
        // Patience trial settings  
        PATIENCE_TIMER_SECONDS: 60,
        PATIENCE_BONUS_THRESHOLD: 30, // seconds to wait for bonus
        
        // Resource management (Olive Tree Investment)
        OLIVE_IMMEDIATE_BONUS: 50,
        OLIVE_INVESTMENT_USE_SQRT: true, // Use square root formula instead of linear
        
        // Risk trial settings
        RISK_SAFE_BONUS: 50,
        RISK_RISKY_SUCCESS: 100,
        RISK_RISKY_PENALTY: -100,
        RISK_SUCCESS_CHANCE: 0.6 // 60% chance of success
    },
    
    // 🎨 UI Settings
    UI: {
        ANIMATION_SPEED: 300, // milliseconds
        NOTIFICATION_DURATION: 3000, // 3 seconds
        ENABLE_CONSOLE_LOGGING: true
    },
    
    // 🔧 Debug Settings
    DEBUG: {
        ENABLE_API_LOGGING: true,
        ENABLE_GAME_STATE_LOGGING: true,
        MOCK_API_RESPONSES: false
    }
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
    }
})();

// Export configuration for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CONFIG;
}
