# Polly Policies - Where and How to Use

## 📋 Overview

This document explains **where** and **how** to use each Polly policy in your OrderService microservice. Polly policies provide **resilience patterns** to handle failures gracefully when calling external microservices.

---

## 🎯 **WHERE Policies Are Applied**

### **Location: `Program.cs` - HttpClient Configuration**

Policies are applied to `HttpClient` instances using `AddHttpClient()` and `AddPolicyHandler()`. This is the **recommended approach** in .NET.

```csharp
// In Program.cs
services.AddHttpClient<ProductsMicroserviceClient>()
    .AddPolicyHandler(/* policy here */);
```

**Why here?**
- Policies are applied **automatically** to all HTTP calls
- No need to modify client code
- Centralized configuration
- Works with dependency injection

---

## 🔧 **Policy Usage Breakdown**

### **1. Products Microservice Policies**

#### **Location Applied**: `Program.cs` → `AddHttpClient<ProductsMicroserviceClient>()`

#### **Policies Used**:
1. **Bulkhead Isolation Policy**
2. **Fallback Policy**

#### **How It Works**:
```csharp
services.AddHttpClient<ProductsMicroserviceClient>()
    .AddPolicyHandler((serviceProvider, request) =>
    {
        var policies = serviceProvider.GetRequiredService<IProductsMicroservicePolicies>();
        
        // Combine: Bulkhead → Fallback
        return Policy.WrapAsync(
            policies.GetBulkheadIsolationPolicy(),  // Outer: Limits concurrent requests
            policies.GetFallbackPolicy()             // Inner: Returns dummy data on failure
        );
    });
```

#### **Execution Flow**:
```
HTTP Request
    ↓
Bulkhead Policy (Max 2 concurrent, Queue 40)
    ↓
Fallback Policy (Returns dummy product on failure)
    ↓
Actual HTTP Call to Products Service
```

#### **What Each Policy Does**:

**Bulkhead Isolation Policy**:
- **Purpose**: Prevents too many concurrent requests from overwhelming the service
- **Configuration**: 
  - Max 2 concurrent requests
  - Queue up to 40 additional requests
- **When Triggered**: When more than 2 requests are in-flight
- **Result**: Throws `BulkheadRejectedException` (caught in `ProductsMicroserviceClient`)

**Fallback Policy**:
- **Purpose**: Provides dummy data when service is unavailable
- **When Triggered**: When HTTP response is not successful
- **Result**: Returns dummy `ProductDTO` with "Temporarily Unavailable" message
- **Benefit**: OrderService continues working even if ProductsService is down

---

### **2. Users Microservice Policies**

#### **Location Applied**: `Program.cs` → `AddHttpClient<UsersMicroserviceClient>()`

#### **Policies Used**:
1. **Retry Policy** (5 retries with exponential backoff)
2. **Circuit Breaker Policy** (Opens after 3 failures, waits 2 minutes)
3. **Timeout Policy** (Fails if request takes > 5 seconds)

#### **How It Works**:
```csharp
services.AddHttpClient<UsersMicroserviceClient>()
    .AddPolicyHandler((serviceProvider, request) =>
    {
        var policies = serviceProvider.GetRequiredService<IUsersMicroservicePolicies>();
        
        // Combined: Retry → Circuit Breaker → Timeout
        return policies.GetCombinedPolicy();
    });
```

#### **Execution Flow**:
```
HTTP Request
    ↓
Timeout Policy (5 seconds max)
    ↓
Circuit Breaker Policy (Check if circuit is open/closed)
    ↓
Retry Policy (Retry up to 5 times with exponential backoff)
    ↓
Actual HTTP Call to Users Service
```

#### **What Each Policy Does**:

**Timeout Policy**:
- **Purpose**: Prevents requests from hanging indefinitely
- **Configuration**: 5 seconds timeout
- **When Triggered**: When request takes longer than 5 seconds
- **Result**: Request fails immediately

**Circuit Breaker Policy**:
- **Purpose**: Stops making requests when service is failing repeatedly
- **Configuration**: 
  - Opens circuit after 3 consecutive failures
  - Stays open for 2 minutes
- **States**:
  - **Closed**: Normal operation, requests allowed
  - **Open**: Circuit is open, requests blocked immediately
  - **Half-Open**: Testing if service recovered
- **Result**: Throws `BrokenCircuitException` (caught in `UsersMicroserviceClient`)

**Retry Policy**:
- **Purpose**: Automatically retries failed requests
- **Configuration**: 
  - 5 retry attempts
  - Exponential backoff: 2s, 4s, 8s, 16s, 32s
- **When Triggered**: When HTTP response is not successful
- **Result**: Retries the request automatically

---

## 📊 **Policy Execution Order**

### **For Products Service**:
```
Request → Bulkhead Check → Fallback Check → HTTP Call
```

### **For Users Service**:
```
Request → Timeout Check → Circuit Breaker Check → Retry Logic → HTTP Call
```

**Important**: Policies execute from **outermost to innermost** when wrapped.

---

## 🎯 **Where Policies Are NOT Used**

### **❌ Don't Apply Policies In**:
1. **Client Classes** (`ProductsMicroserviceClient`, `UsersMicroserviceClient`)
   - Policies are already applied via `AddHttpClient()`
   - No need to manually wrap calls

2. **Service Classes** (`OrderService`)
   - Services just use the clients normally
   - Policies work transparently

3. **Repository Classes**
   - Repositories don't make HTTP calls
   - No policies needed

---

## 🔍 **How Policies Are Registered**

### **In `DI.cs`**:
```csharp
// Register policy implementations
services.AddScoped<IPollyPolicies, PollyPolicies>();
services.AddScoped<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();
services.AddScoped<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
```

### **In `Program.cs`**:
```csharp
// Apply policies to HttpClient
services.AddHttpClient<ProductsMicroserviceClient>()
    .AddPolicyHandler(/* policy */);
```

---

## 📝 **Real-World Scenarios**

### **Scenario 1: Products Service is Slow**
1. **Bulkhead** limits concurrent requests to 2
2. Additional requests wait in queue (up to 40)
3. If queue is full → `BulkheadRejectedException` → Returns dummy product

### **Scenario 2: Products Service is Down**
1. HTTP call fails
2. **Fallback Policy** triggers
3. Returns dummy `ProductDTO` with "Temporarily Unavailable"
4. OrderService continues working

### **Scenario 3: Users Service is Intermittently Failing**
1. First request fails → **Retry Policy** retries (5 times)
2. If all retries fail → **Circuit Breaker** opens after 3 failures
3. Next requests blocked immediately for 2 minutes
4. After 2 minutes → Circuit half-opens, tests service
5. If successful → Circuit closes, normal operation resumes

### **Scenario 4: Users Service Takes Too Long**
1. Request takes > 5 seconds
2. **Timeout Policy** cancels request
3. **Retry Policy** retries with new timeout
4. If still timing out → **Circuit Breaker** opens

---

## ✅ **Benefits of This Approach**

1. **Automatic**: Policies apply to all HTTP calls automatically
2. **Transparent**: Client code doesn't need to know about policies
3. **Centralized**: All policy configuration in one place (`Program.cs`)
4. **Testable**: Policies can be mocked for testing
5. **Flexible**: Easy to change policies without modifying client code

---

## ⚙️ **Configuration Values**

### **Products Service Policies**:
- **Bulkhead**: 2 concurrent, 40 queued
- **Fallback**: Returns dummy product on failure

### **Users Service Policies**:
- **Retry**: 5 attempts, exponential backoff (2s, 4s, 8s, 16s, 32s)
- **Circuit Breaker**: Opens after 3 failures, waits 2 minutes
- **Timeout**: 5 seconds

**To Change**: Modify values in policy classes (`ProductsMicroservicePolicies.cs`, `UsersMicroservicePolicies.cs`)

---

## 🚨 **Important Notes**

1. **Base URLs**: Update `BaseAddress` in `Program.cs` with your actual gateway URLs
2. **Policy Order**: Order matters when wrapping policies
3. **Exception Handling**: Clients still need try-catch for `BulkheadRejectedException` and `BrokenCircuitException`
4. **Logging**: All policies log their actions (check logs to see policy behavior)
5. **Testing**: Test policies by simulating service failures

---

## 📚 **Summary**

**Where to Use**:
- ✅ `Program.cs` - Apply policies to HttpClient
- ✅ `DI.cs` - Register policy services
- ❌ Client classes - Don't manually apply policies
- ❌ Service classes - Don't need to know about policies

**How It Works**:
1. Policies registered in DI
2. Policies applied to HttpClient in `Program.cs`
3. Policies execute automatically on all HTTP calls
4. Client code works normally, policies handle failures

This is the **recommended .NET pattern** for resilience in microservices! 🎯


