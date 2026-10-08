# OrderFlow

### Educational Backend Project — .NET 8

OrderFlow is a small e-commerce backend designed to demonstrate how a traditional CRUD application can evolve into a more scalable read/write architecture using **CQRS, Redis caching, a Materialized View, and background processing**.

The project focuses on understanding real backend architectural trade-offs while keeping the implementation simple enough for learning and classroom walkthroughs.

---

## 📌 Project Overview

OrderFlow allows customers to create orders and provides APIs for reading order information and dashboard data.

The system uses:

* **SQL Server** as the source of truth for transactional data.
* **CQRS** to separate write and read responsibilities.
* **MediatR** to dispatch Commands and Queries.
* **Redis** to cache frequently requested order details.
* A **Materialized View / Read Model** optimized for dashboard queries.
* A **BackgroundService** to process pending orders and refresh the dashboard read model.

The main goal is to demonstrate how different backend patterns solve different problems without introducing unnecessary architectural complexity.

---

## 🏗️ Architecture

The project follows **Clean Architecture** combined with **Vertical Slice Architecture** for application features.

```text
OrderFlow
│
├── OrderFlow.Domain
│   └── Entities
│       ├── Order
│       ├── OrderItem
│       └── Customer
│
├── OrderFlow.Application
│   ├── Common
│   │   └── Interfaces
│   │
│   └── Features
│       ├── Orders
│       │   ├── CreateOrder
│       │   ├── GetOrderById
│       │   └── GetOrders
│       │
│       └── Dashboard
│           └── GetDashboard
│
├── OrderFlow.Infrastructure
│   ├── Persistence
│   │   ├── ApplicationDbContext
│   │   └── ReadModels
│   │
│   ├── Services
│   │   ├── DashboardRefreshService
│   │   └── PendingOrdersProcessor
│   │
│   └── BackgroundJobs
│       └── DashboardRefreshBackgroundService
│
└── OrderFlow.API
    └── Controllers
```

### Architecture Flow

```text
                    ┌──────────────────┐
                    │     API Layer    │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │ Application      │
                    │ CQRS + MediatR   │
                    └────────┬─────────┘
                             │
              ┌──────────────┴──────────────┐
              ▼                             ▼
       Commands / Write              Queries / Read
              │                             │
              ▼                             ▼
       SQL Server                    Redis Cache
       Source of Truth                    │
              │                            │
              │                       Cache Miss
              │                            │
              │                            ▼
              │                       SQL Server
              │
              ▼
      Transactional Data
              │
              │ Background Processing
              ▼
     Materialized Read Model
              │
              ▼
        Dashboard Query
```

---

# ✨ Features

## 1. Create Order

The system supports creating an order for a customer.

An order contains one or more items.

Each order item contains:

* Product Name
* Quantity
* Unit Price

The order total is calculated from its items.

```text
Order Total = Σ (Quantity × Unit Price)
```

### Example

```text
Product A
Quantity: 2
Unit Price: 100

Product B
Quantity: 1
Unit Price: 50

Total = 250
```

---

## 2. Get Order By ID

The system provides an endpoint for retrieving order details by ID.

This operation demonstrates the **Query + Cache** flow.

```text
GET /api/orders/{id}
```

The request follows this flow:

```text
Request
   │
   ▼
Check Redis
   │
   ├── Cache Hit ──────► Return cached order
   │
   └── Cache Miss
          │
          ▼
      SQL Server
          │
          ▼
      Store in Redis
          │
          ▼
      Return order
```

This demonstrates how caching can reduce repeated database reads for frequently requested data.

---

## 3. Get Orders

The system provides a query for retrieving a list of orders for read/admin screens.

```text
GET /api/orders
```

This is implemented as a **Query**, keeping read logic separate from commands that modify state.

---

## 4. Order Dashboard

The system exposes dashboard data using a dedicated **Read Model**.

```text
GET /api/dashboard/orders
```

The dashboard contains optimized information such as:

* Customer Name
* Item Count
* Order Total
* Order Status

Instead of repeatedly building this information from multiple transactional tables, the system maintains a separate read-optimized table.

---

# 🧩 CQRS

OrderFlow uses **CQRS (Command Query Responsibility Segregation)**.

The basic idea is to separate:

### Commands

Commands change application state.

Examples:

```text
CreateOrderCommand
```

```text
Pending → Completed
```

### Queries

Queries retrieve data without changing application state.

Examples:

```text
GetOrderByIdQuery
GetOrdersQuery
GetDashboardQuery
```

The goal is not simply to create more classes.

The goal is to allow read and write operations to evolve independently based on their different requirements.

---

# 💾 SQL Server

SQL Server is the **source of truth** for transactional data.

The main transactional entities include:

```text
Customer
Order
OrderItem
```

The write database remains authoritative.

The dashboard read model is **not** treated as the source of truth.

---

# ⚡ Redis Caching

Redis is used for frequently requested order details.

The basic strategy is:

```text
Read Request
     │
     ▼
Redis
  │     │
 Hit   Miss
  │     │
  │     ▼
  │   SQL Server
  │     │
  │     ▼
  │   Redis
  │
  ▼
Response
```

Cache entries use an expiration time (TTL).

When an order is modified, its cached representation should be invalidated or refreshed to prevent stale data.

---

# 📊 Materialized View / Read Model

The dashboard uses a dedicated read-optimized table.

Conceptually:

```text
Transactional Tables
       │
       │ Refresh
       ▼
┌────────────────────────┐
│ OrderDashboardReadModel│
├────────────────────────┤
│ CustomerName           │
│ ItemCount              │
│ Total                  │
│ Status                 │
└────────────────────────┘
       │
       ▼
Dashboard Query
```

The read model exists specifically to make dashboard reads simpler and cheaper.

It can contain denormalized data because it is optimized for reading rather than transactional consistency.

### Important

The read model is **not the source of truth**.

If the read model is lost, it can be rebuilt from the transactional database.

---

# 🔄 Background Processing

OrderFlow uses `.NET BackgroundService` for background operations.

The background worker is responsible for:

### 1. Processing Pending Orders

Pending orders are processed and moved to:

```text
Pending → Completed
```

### 2. Refreshing the Dashboard Read Model

After processing changes, the dashboard read model is refreshed.

The overall flow is:

```text
BackgroundService
       │
       ├── Process Pending Orders
       │        │
       │        ▼
       │   Pending → Completed
       │        │
       │        ▼
       │   Invalidate Cache
       │
       └── Refresh Dashboard Read Model
```

The worker creates a dependency scope before resolving scoped services such as the database context.

---

# 🧱 Technologies

| Technology                  | Purpose                  |
| --------------------------- | ------------------------ |
| .NET 8                      | Backend framework        |
| ASP.NET Core Web API        | REST API                 |
| C#                          | Programming language     |
| Entity Framework Core       | ORM / Database access    |
| SQL Server                  | Transactional database   |
| MediatR                     | CQRS request dispatching |
| Redis                       | Distributed caching      |
| BackgroundService           | Background processing    |
| Swagger / OpenAPI           | API documentation        |
| Clean Architecture          | Separation of concerns   |
| Vertical Slice Architecture | Feature organization     |

---

# 🌐 API Endpoints

| Method | Endpoint                | Purpose                  |
| ------ | ----------------------- | ------------------------ |
| POST   | `/api/orders`           | Create an order          |
| GET    | `/api/orders/{id}`      | Get order details        |
| GET    | `/api/orders`           | Get orders list          |
| GET    | `/api/dashboard/orders` | Get dashboard read model |

---

# 🔀 Request Flows

## Create Order

```text
HTTP POST
   │
   ▼
OrdersController
   │
   ▼
CreateOrderCommand
   │
   ▼
MediatR
   │
   ▼
CreateOrderHandler
   │
   ▼
SQL Server
   │
   ▼
Order Created
```

---

## Get Order By ID

```text
HTTP GET
   │
   ▼
OrdersController
   │
   ▼
GetOrderByIdQuery
   │
   ▼
MediatR
   │
   ▼
Handler
   │
   ▼
Redis
 ┌─┴─────────────┐
 │               │
Hit             Miss
 │               │
 ▼               ▼
Return       SQL Server
                 │
                 ▼
               Redis
                 │
                 ▼
              Return
```

---

## Dashboard

```text
HTTP GET
   │
   ▼
DashboardController
   │
   ▼
GetDashboardQuery
   │
   ▼
Materialized Read Model
   │
   ▼
Dashboard Response
```

---

# 🎯 Functional Requirements

| ID    | Requirement                                               | Status |
| ----- | --------------------------------------------------------- | ------ |
| FR-01 | Create an order for a customer                            | ✅      |
| FR-02 | Order contains one or more items                          | ✅      |
| FR-03 | Order items contain product name, quantity and unit price | ✅      |
| FR-04 | Calculate order total                                     | ✅      |
| FR-05 | Get order details by ID                                   | ✅      |
| FR-06 | Get list of orders                                        | ✅      |
| FR-07 | Expose order dashboard                                    | ✅      |
| FR-08 | Process pending orders and move them to Completed         | ✅      |
| FR-09 | Cache frequently requested order details                  | ✅      |
| FR-10 | Maintain a read-optimized dashboard read model            | ✅      |

---

# 🧠 Learning Objectives

This project demonstrates several important backend concepts.

### CQRS

Understanding why commands and queries can have different responsibilities and implementation strategies.

### Caching

Understanding:

* Why caching is needed
* Cache hit vs cache miss
* Cache expiration
* Cache invalidation
* Why caching does not replace the database

### Materialized Read Models

Understanding the difference between:

```text
Transactional Database
```

and:

```text
Read-Optimized Model
```

and why denormalization can improve read performance.

### Background Processing

Understanding why some operations can be moved outside the HTTP request lifecycle.

### Architecture Trade-offs

The project intentionally avoids unnecessary complexity.

The goal is to understand:

> Use architecture to solve a real problem, not to add architecture for its own sake.

---

# 🚫 Out of Scope

The following are intentionally excluded:

* Authentication / Authorization
* Payment gateways
* Real message brokers
* Kafka
* RabbitMQ
* Kubernetes
* Microservices
* Event Sourcing
* Database Sharding
* Complex Inventory Management
* Distributed Deployment

These can be added later as extension exercises.

---

# 🔧 Future Improvements

The project can be extended with the following improvements.

## 1. Automated Tests

Add:

* Unit tests for handlers
* Integration tests for API endpoints
* Tests for cache behavior
* Background processing tests

Important scenario:

```text
Pending Order
      ↓
Completed
      ↓
Cache Invalidated
      ↓
Dashboard Refreshed
```

---

## 2. Better Cache Key Management

Instead of manually writing cache keys throughout the application:

```csharp
$"order:{id}"
```

a centralized cache-key strategy could be introduced.

Example:

```text
CacheKeys.Order(id)
```

This reduces duplicated strings and makes future changes safer.

---

## 3. Structured Logging

Add meaningful logs for:

* Order creation
* Cache hit / miss
* Pending order processing
* Dashboard refresh
* Background worker failures

This would make troubleshooting easier in production.

---

## 4. Better Background Worker Resilience

The worker can be improved with:

* `PeriodicTimer`
* Exception handling per execution cycle
* Structured logging
* Graceful cancellation
* Configurable refresh intervals

This prevents a single unexpected exception from stopping background processing permanently.

---

## 5. Database Optimization

Potential improvements include:

* Indexes on frequently filtered columns
* `AsNoTracking()` for read-only queries
* Query projection using DTOs
* Reviewing generated SQL
* Avoiding unnecessary database round trips

---

## 6. Health Checks

Add health checks for:

```text
API
 │
 ├── SQL Server
 │
 └── Redis
```

This makes it easier to determine whether the application dependencies are available.

---

## 7. Configuration

Connection strings and infrastructure settings should remain outside the code.

Example:

```text
appsettings.json
Environment Variables
```

Potential configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "Redis": "..."
  }
}
```

Secrets should never be committed to GitHub.

---

# 📚 Key Architectural Lessons

### 1. CQRS is not mandatory everywhere

CQRS is useful when read and write operations have different requirements.

For a very small CRUD application, traditional CRUD may be simpler.

---

### 2. Redis is not the database

Redis is a cache.

The SQL Server database remains the source of truth.

```text
SQL Server = Source of Truth
Redis      = Performance Optimization
```

---

### 3. Read Models can be rebuilt

The dashboard read model is derived from transactional data.

Therefore:

```text
Transactional Data
        ↓
   Rebuild Process
        ↓
 Dashboard Read Model
```

---

### 4. Background processing reduces request responsibilities

A user request should not necessarily wait for every piece of secondary processing.

Background processing can handle tasks that do not need to block the HTTP response.

---

### 5. Simplicity matters

The project intentionally avoids:

```text
Microservices
Message Brokers
Kubernetes
Event Sourcing
Distributed Systems
```

The architecture should match the actual problem.

---

# 🏁 Conclusion

OrderFlow demonstrates how a backend application can evolve from a simple transactional CRUD system into an architecture that supports:

```text
Clean Architecture
       +
Vertical Slices
       +
CQRS
       +
MediatR
       +
Redis Caching
       +
Materialized Read Model
       +
Background Processing
```

The main objective is not to build the most complicated architecture possible.

It is to understand **why each component exists, what problem it solves, and what trade-offs it introduces.**

---

## 👩‍💻 Project Focus

This project was built as a practical backend learning project focusing on:

* ASP.NET Core Web API
* Clean Architecture
* CQRS
* Vertical Slice Architecture
* Entity Framework Core
* SQL Server
* Redis
* Background Services
* Read Models
* Caching Strategies
* Backend Performance
* Separation of Concerns
