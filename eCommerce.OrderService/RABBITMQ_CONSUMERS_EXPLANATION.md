# RabbitMQ Consumers Explanation

## 📋 Overview

This document explains the **RabbitMQ Consumer Pattern** used in the OrderService to keep product data synchronized across microservices using **event-driven architecture**.

---

## 🎯 **WHY We Need RabbitMQ Consumers**

### The Problem:
1. **OrderService caches product data** locally to reduce HTTP calls to ProductService
2. When ProductService **deletes or updates** a product, OrderService's cache becomes **stale/invalid**
3. Without synchronization, OrderService might:
   - Use deleted products in orders
   - Show outdated product names/prices
   - Make unnecessary HTTP calls for deleted products

### The Solution:
**RabbitMQ Consumers** listen for product events and automatically update/invalidate the cache in real-time.

---

## 🔄 **How It Works - Event Flow**

```
┌─────────────────┐                    ┌──────────────┐                    ┌─────────────────┐
│ ProductService  │                    │  RabbitMQ   │                    │  OrderService   │
│                 │                    │   Broker    │                    │                 │
│ 1. Delete       │───Publish Event───▶│             │───Consume Event───▶│ 2. Remove from  │
│    Product      │                    │             │                    │    Cache        │
│                 │                    │             │                    │                 │
│ 3. Update       │───Publish Event───▶│             │───Consume Event───▶│ 4. Update Cache │
│    Product Name │                    │             │                    │    with new data│
└─────────────────┘                    └──────────────┘                    └─────────────────┘
```

---

## 📦 **Components Explained**

### 1. **Message Classes**
```csharp
ProductDeletionMessage(Guid ProductID, string? ProductName)
```
- **Purpose**: Data structure for product deletion events
- **Sent by**: ProductService when a product is deleted
- **Received by**: OrderService consumer

### 2. **Consumer Interfaces**
```csharp
IRabbitMQProductDeletionConsumer
IRabbitMQProductNameUpdateConsumer
```
- **Purpose**: Define contract for consumers
- **Methods**: `Consume()` - starts listening, `Dispose()` - cleanup

### 3. **Consumer Implementations**

#### **RabbitMQProductDeletionConsumer**
- **Listens for**: `product.delete` events
- **Action**: Removes product from cache when deleted
- **Queue**: `orders.product.delete.queue`
- **Exchange Type**: `Headers` (matches messages by headers, not routing keys)

#### **RabbitMQProductNameUpdateConsumer**
- **Listens for**: `product.update` events  
- **Action**: Updates cache with new product information
- **Queue**: `orders.product.update.name.queue`
- **Exchange Type**: `Headers`

### 4. **Hosted Services**
```csharp
RabbitMQProductDeletionHostedService
RabbitMQProductNameUpdateHostedService
```
- **Purpose**: Background services that start consumers when application starts
- **Lifecycle**: 
  - `StartAsync()` - Called when app starts, begins consuming
  - `StopAsync()` - Called when app stops, disposes consumers

---

## 🔧 **Technical Details**

### **Header-Based Routing**
Instead of routing keys, we use **headers** to match messages:
```csharp
var headers = new Dictionary<string, object>()
{
    { "x-match", "all" },      // All headers must match
    { "event", "product.delete" },
    { "RowCount", 1 }
};
```

**Why Headers?**
- More flexible than routing keys
- Can match multiple criteria
- Better for event-driven patterns

### **Cache Key Format**
```csharp
string cacheKey = $"product: {productID}";  // Matches ProductsMicroserviceClient format
```

**Important**: The cache key format must match exactly what `ProductsMicroserviceClient` uses!

### **Exchange Configuration**
- **Exchange Name**: `RabbitMQ_Products_Exchange` (from ProductService)
- **Type**: `Headers` - routes based on message headers
- **Durable**: `true` - survives RabbitMQ server restarts

---

## 🚀 **How It's Integrated**

### **1. Dependency Injection (DI.cs)**
```csharp
// Register as Singleton (they run continuously)
services.AddSingleton<IRabbitMQProductDeletionConsumer, RabbitMQProductDeletionConsumer>();
services.AddSingleton<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>();

// Register Hosted Services (start automatically)
services.AddHostedService<RabbitMQProductDeletionHostedService>();
services.AddHostedService<RabbitMQProductNameUpdateHostedService>();
```

### **2. Configuration (appsettings.json)**
```json
{
  "RabbitMQ_HostName": "localhost",
  "RabbitMQ_Port": "5672",
  "RabbitMQ_UserName": "guest",
  "RabbitMQ_Password": "guest",
  "RabbitMQ_Products_Exchange": "product.exchange"  // Exchange from ProductService
}
```

### **3. Distributed Cache (Program.cs)**
```csharp
builder.Services.AddDistributedMemoryCache();  // For development
// In production, use: AddStackExchangeRedisCache()
```

---

## 📊 **Complete Flow Example**

### **Scenario: Product Deletion**

1. **ProductService** deletes a product
2. **ProductService** publishes `ProductDeletionMessage` to RabbitMQ with headers:
   ```json
   {
     "event": "product.delete",
     "RowCount": 1
   }
   ```
3. **RabbitMQ** routes message to `orders.product.delete.queue` (header match)
4. **OrderService Consumer** receives message
5. **Consumer** removes product from cache: `cache.RemoveAsync("product: {productID}")`
6. **Next time** OrderService requests this product:
   - Cache miss → HTTP call to ProductService
   - ProductService returns 404 → OrderService knows product is deleted

### **Scenario: Product Update**

1. **ProductService** updates product name
2. **ProductService** publishes `ProductDTO` to RabbitMQ with headers:
   ```json
   {
     "event": "product.update",
     "RowCount": 1
   }
   ```
3. **RabbitMQ** routes message to `orders.product.update.name.queue`
4. **OrderService Consumer** receives message
5. **Consumer** updates cache with new product data
6. **Next time** OrderService requests this product:
   - Cache hit → Returns updated product data immediately (no HTTP call)

---

## ✅ **Benefits**

1. **Real-time Synchronization**: Cache updates immediately when products change
2. **Reduced HTTP Calls**: Cache stays fresh, fewer API calls needed
3. **Loose Coupling**: Services communicate via events, not direct calls
4. **Scalability**: Multiple services can consume the same events
5. **Reliability**: Messages are durable, won't be lost if service is down

---

## ⚠️ **Important Notes**

1. **Cache Key Consistency**: Must match `ProductsMicroserviceClient` format exactly
2. **Exchange Must Exist**: ProductService must create the exchange first
3. **Queue Durability**: Queues are durable, messages persist across restarts
4. **Auto-Acknowledge**: Currently using `autoAck: true` (for production, consider manual ack)
5. **Error Handling**: Add try-catch in consumer handlers for production

---

## 🔍 **Testing**

1. **Start OrderService** - Consumers start automatically
2. **Delete a product in ProductService** - Check logs for cache removal
3. **Update a product in ProductService** - Check logs for cache update
4. **Verify cache** - Try to get product via OrderService, should reflect changes

---

## 📝 **Summary**

**RabbitMQ Consumers** enable **event-driven cache synchronization** between microservices:
- **Listen** for product events from ProductService
- **Update/Invalidate** local cache automatically
- **Keep data consistent** across services without tight coupling
- **Improve performance** by maintaining fresh cache

This is a **best practice** in microservices architecture for maintaining data consistency!

