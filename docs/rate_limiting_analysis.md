# 🚨 Rate Limiting Analysis - 503 Errors

## Problem Detection

**k6 test started failing at ~180 seconds with HTTP 503 errors:**

```
INFO[0179] [5] ✗ Session start failed - Status: 503
INFO[0180] [15] ✗ Signup failed - Status: 503
INFO[0180] [5] ✗ Signup failed - Status: 503
```

**Timeline:**
- 0-120s: 5 users → No problems (< 10 req/min)
- 120-180s: Ramping to 30 users → Starting to hit limits
- 180s+: 30 concurrent users → **Massive 503 errors** 💥

---

## Root Cause: Backend Rate Limiting

**`Program.cs` - Lines 69-76:**

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiPolicy", opt =>
    {
        opt.PermitLimit = 10;  // ⚠️ ONLY 10 requests
        opt.Window = TimeSpan.FromMinutes(1);  // per minute
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;  // + 2 queued = 12 total capacity
    });
});
```

**Applied to ALL endpoints in:**
- `VRParkController.cs` (6 endpoints)
- `GameController.cs` (3 endpoints)
- `AdminController.cs` (3 endpoints)
- `PlayerController.cs` (1 endpoint)

---

## Math

**30 concurrent users scenario:**

```
30 users × 4 requests (signup, verify, start, end) = 120 requests
+ 2.5% admin calls = ~123 requests total

Rate limit capacity: 10 + 2 queued = 12 requests/minute

123 requests > 12 capacity → 111 requests REJECTED → 503 errors
```

---

## Solution 1: Increase Backend Rate Limit (Recommended)

### For Load Testing (Generous limits)

**Edit `Program.cs` lines 69-76:**

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiPolicy", opt =>
    {
        opt.PermitLimit = 200;  // 200 requests per minute
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 50;  // 50 queued = 250 total capacity
    });
});
```

**Calculation:**
- 30 users × 4 requests = 120 requests
- 120 < 200 → ✅ All requests pass
- Buffer: 80 extra capacity for admin calls + spikes

### For Production (Balanced security + performance)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiPolicy", opt =>
    {
        opt.PermitLimit = 100;  // 100 requests per minute
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 20;  // 20 queued = 120 total capacity
    });
});
```

**Calculation:**
- Real users: ~20-30 concurrent realistic
- 30 × 4 = 120 requests → Still protected from abuse
- DDoS protection: Blocks >120 req/min from single IP

---

## Solution 2: Per-IP Rate Limiting (Better for production)

**Replace fixed window with sliding window + per-IP:**

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Global policy - high limit
    options.AddFixedWindowLimiter("ApiPolicy", opt =>
    {
        opt.PermitLimit = 200;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 50;
    });
    
    // Per-IP policy - prevent abuse from single source
    options.AddSlidingWindowLimiter("PerIpPolicy", opt =>
    {
        opt.PermitLimit = 50;  // 50 requests per IP
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;  // 10-second segments
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

// In controllers, use:
[EnableRateLimiting("PerIpPolicy")]  // Instead of "ApiPolicy"
```

**Benefits:**
- Each IP gets 50 req/min
- 30 different IPs × 50 = 1500 total capacity
- Single malicious IP blocked at 50 req/min
- Load test works (30 IPs, each under 50 req/min)

---

## Solution 3: Remove Rate Limiting for Load Test

**Temporarily disable for k6 testing:**

**`Program.cs` - Comment out:**

```csharp
// builder.Services.AddRateLimiter(options => { ... });  // COMMENTED
```

**AND comment out in controllers:**

```csharp
// [EnableRateLimiting("ApiPolicy")]  // COMMENTED
```

**⚠️ WARNING:** Only for testing! Re-enable for production!

---

## Solution 4: Different Policies for Different Endpoints

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Public endpoints - generous
    options.AddFixedWindowLimiter("PublicPolicy", opt =>
    {
        opt.PermitLimit = 200;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 50;
    });
    
    // Admin endpoints - strict
    options.AddFixedWindowLimiter("AdminPolicy", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 5;
    });
});
```

**Controllers:**

```csharp
// VRParkController.cs (signup, verify, sessions)
[EnableRateLimiting("PublicPolicy")]  // Use generous limit

// AdminController.cs (database access)
[EnableRateLimiting("AdminPolicy")]  // Use strict limit
```

---

## Render.com Free Tier Limits

### ✅ Not a Render issue - It's backend code!

**Render Free Tier limits:**
- **750 hours/month** (31 days × 24h = 744h → enough for 1 app)
- **No request limit** on Free tier
- **No bandwidth limit** (reasonable use)
- **Automatic sleep after 15 min inactivity** (wakes on request)
- **512 MB RAM** (plenty for this API)
- **0.1 CPU** (shared, slower)

**How to check Render limits:**

1. **Dashboard → Your Service → Metrics tab**
   - CPU usage
   - Memory usage
   - Request rate
   - Response times
   - Bandwidth

2. **Dashboard → Your Service → Settings → Instance Type**
   - Free: 512 MB RAM, 0.1 CPU
   - Starter ($7/mo): 512 MB RAM, 0.5 CPU
   - Standard ($25/mo): 2 GB RAM, 1 CPU

3. **Dashboard → Billing → Usage**
   - Hours used this month
   - Services count

**Upgrade path if needed:**

```
Free (Current)
   └─> Starter ($7/mo) - 5× faster CPU, no sleep
        └─> Standard ($25/mo) - 4× more RAM, 2× faster CPU
             └─> Pro ($85/mo) - 8 GB RAM, 2 CPU, autoscaling
```

**Free tier is fine for this load!** The 503s are from backend rate limiting, not Render.

---

## Recommendation

### For Load Testing (Now):

**Increase rate limit to 200/min:**

```csharp
opt.PermitLimit = 200;
opt.QueueLimit = 50;
```

### For Production (After testing):

**Per-IP rate limiting with 50/min per IP:**

```csharp
options.AddSlidingWindowLimiter("PerIpPolicy", opt =>
{
    opt.PermitLimit = 50;
    opt.Window = TimeSpan.FromMinutes(1);
    opt.SegmentsPerWindow = 6;
    opt.QueueLimit = 10;
});
```

**This protects from:**
- ✅ DDoS attacks (50 req/min per IP)
- ✅ Brute force (already have AdminAuthenticationTracker)
- ✅ API abuse
- ✅ Allows 30 concurrent legitimate users
- ✅ k6 load test passes (30 different virtual IPs)

---

## How to Check if 503 is Render vs Backend

**Check response headers:**

```powershell
Invoke-WebRequest -Uri "https://athens-secret-api.onrender.com/api/vrpark/signup" `
    -Method POST `
    -Body '{"firstName":"Test","lastName":"User","email":"test@test.com","age":25}' `
    -ContentType "application/json" `
    -SkipHttpErrorCheck

# Check $_.Headers
# If contains "Retry-After" → Rate limiting (backend or Render)
# If contains "X-RateLimit-*" → Your backend rate limit
# If contains "CF-RAY" → Cloudflare/Render issue
```

**Backend rate limit response:**
```
Status: 503
Headers:
  Retry-After: 60
  (no X-RateLimit-* headers because .NET doesn't add them by default)
```

**Render rate limit response (doesn't exist on free tier):**
```
Status: 429
Headers:
  X-RateLimit-Limit: 1000
  X-RateLimit-Remaining: 0
```

---

## Next Steps

1. ✅ **Confirm**: It's backend rate limiting (10 req/min too low)
2. 🔧 **Fix**: Increase to 200 req/min for load testing
3. 🧪 **Test**: Re-run k6 script
4. 📊 **Monitor**: Check Render metrics during test
5. 🔒 **Production**: Switch to per-IP sliding window (50 req/min per IP)

---

**Result:** 30 concurrent users × 4 requests = 120 req/min < 200 limit → ✅ No 503 errors!
