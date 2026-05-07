# VendorHub E-Commerce API

A comprehensive, production-ready ASP.NET Core API for a multi-vendor e-commerce platform with real-time notifications, permission management, and advanced analytics.

## 🎯 Project Overview

VendorHub is a full-featured REST API that enables:

- **Multi-vendor marketplace** - Vendors can upload, manage, and sell products
- **Customer shopping** - Browse products, place orders, leave reviews, manage favorites
- **Admin management** - Manage vendors, products, permissions, and system health
- **Real-time notifications** - SignalR for instant order updates and vendor alerts
- **Permission system** - Granular role-based and per-vendor permissions
- **Advanced analytics** - Sales tracking, order statistics, vendor performance

---

## ✨ Key Features

### 🛍️ Product Management

- ✅ Product CRUD with status workflow (Pending → Reviewed → Approved/Rejected)
- ✅ Category management and filtering
- ✅ Product visibility tracking and viewer counts
- ✅ Review system with ratings and comments
- ✅ Favorites/Wishlist functionality

### 📦 Order Management

- ✅ Shopping cart to order conversion
- ✅ Stock reduction on purchase
- ✅ Order status tracking (Pending → Confirmed → Processing → Shipped → Delivered)
- ✅ Order history per customer
- ✅ Real-time order notifications

### 👥 User Management

- ✅ Role-based authentication (Admin, Vendor, Customer)
- ✅ JWT token-based security
- ✅ User account deactivation (soft delete)
- ✅ Vendor approval workflow

### 🔐 Permission System

- ✅ Granular permissions (CanUploadProducts, CanEditProducts, etc.)
- ✅ Per-vendor permission control
- ✅ Role-based bulk permission management
- ✅ Custom authorization attributes

### 🔔 Real-Time Features

- ✅ SignalR for instant notifications
- ✅ Online/offline notification handling
- ✅ Database persistence for offline users
- ✅ Notification read/unread tracking

### 📊 Analytics

- ✅ Vendor dashboard statistics
- ✅ Monthly sales tracking
- ✅ Top products analysis
- ✅ Revenue calculations

---

## 🏗️ Architecture

### Layered Architecture

Controllers (API Endpoints)
↓
Services (Business Logic)
↓
Repositories (Data Access)
↓
Database (Entity Framework Core)

### Project Structure

VendorHub/
├── Controllers/
│ ├── AccountController.cs
│ ├── CategoryController.cs
│ ├── FavoriteController.cs
│ ├── NotificationController.cs
│ ├── OrderController.cs
│ ├── PermissionController.cs
│ ├── ProductController.cs
│ ├── ReviewController.cs
│ └── StatisticsController.cs
│
├── Services/
│ ├── IAccountService.cs & AccountService.cs
│ ├── ICategoryService.cs & CategoryService.cs
│ ├── IFavoriteService.cs & FavoriteService.cs
│ ├── INotificationService.cs & NotificationService.cs
│ ├── IOrderService.cs & OrderService.cs
│ ├── IPermissionService.cs & PermissionService.cs
│ ├── IProductService.cs & ProductService.cs
│ ├── IReviewService.cs & ReviewService.cs
│ └── IStatisticsService.cs & StatisticsService.cs
│
├── Repository/
│ ├── IGeneralRepository.cs
│ └── GeneralRepository.cs
│
├── Models/
│ ├── User.cs (Base class)
│ ├── Admin.cs
│ ├── Vendor.cs
│ ├── Customer.cs
│ ├── Product.cs
│ ├── Category.cs
│ ├── Order.cs
│ ├── OrderItem.cs
│ ├── Review.cs
│ ├── Favorite.cs
│ ├── Notification.cs
│ ├── Permission.cs
│ ├── VendorPermission.cs
│ ├── VendorHubDbContext.cs
│ └── Enums/ (ProductStatus, OrderStatus, AccountStatus, etc.)
│
├── DTOs/
│ ├── AccountDto/
│ ├── CategoryDto/
│ ├── FavoriteDto/
│ ├── NotificationDto/
│ ├── OrderDto/
│ ├── PermissionDto/
│ ├── ProductDto/
│ ├── ReviewDto/
│ ├── StatisticsDto/
│ └── sharedDto/ (GeneralResponse, etc.)
│
├── Hubs/
│ └── NotificationHub.cs (SignalR)
│
├── Attributes/
│ └── RequirePermissionAttribute.cs
│
├── Helpers/
│ ├── RoleSeeder.cs
│ ├── PermissionSeeder.cs
│ └── ProductHelper.cs
│
├── Filters/
│ └── ValidateModelStateFilter.cs
│
└── Program.cs

---

## 🗄️ Database Schema

### Core Entities

**Users (Table Per Hierarchy)**
AspNetUsers
├── Id (PK)
├── Email
├── PasswordHash
├── FirstName, SecondName
├── AccountStatus (ACTIVE, PENDING, DELETED)
├── Role (Discriminator: Admin, Vendor, Customer)
├── CreatedAt, UpdatedAt
├── StoreName (Vendor only)
├── Balance (Vendor only)
└── Address (Customer only)

**Products**
Products
├── Id (PK)
├── Name, Price, Quantity
├── ImgUrl, Status (PENDING, REVIEWED, REJECTED, Archived)
├── ProductionDate, ExpireDate
├── VendorId (FK) → Vendors
├── CategoryId (FK) → Categories
├── ViewersNo, OverallStars, ReviewCount
└── CreatedAt, UpdatedAt

**Orders & OrderItems**
Orders
├── Id (PK)
├── CustomerId (FK) → Customers
├── TotalPrice, Status (Pending, Confirmed, Processing, Shipped, Delivered, Cancelled)
├── DeliveryAddress, PhoneNumber
└── CreatedAt, UpdatedAt
OrderItems
├── Id (PK)
├── OrderId (FK) → Orders
├── ProductId (FK) → Products
├── Quantity, PriceAtPurchase
└── CreatedAt

**Permissions**
Permissions
├── Id (PK)
├── Type (Enum as string: CanUploadProducts, etc.)
├── Description, Category
├── IsActive
└── CreatedAt
VendorPermissions
├── Id (PK)
├── VendorId (FK) → Vendors
├── PermissionId (FK) → Permissions
├── IsEnabled
└── CreatedAt, UpdatedAt

---

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 or higher
- SQL Server 2019 or higher
- Visual Studio 2022 or VS Code

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/VendorHub.git
cd VendorHub

# 2. Restore NuGet packages
dotnet restore

# 3. Update appsettings.json with your database connection
{
  "ConnectionStrings": {
    "sqlServerCs": "Server=YOUR_SERVER;Database=VendorHubDb;User Id=sa;Password=YOUR_PASSWORD;"
  },
  "JWT": {
    "SecritKey": "your-secret-key-min-32-chars",
    "IssuerIP": "https://localhost:7001",
    "AudienceIP": "https://localhost:3000"
  }
}

# 4. Apply migrations
dotnet ef database update

# 5. Run the application
dotnet run

# API available at: https://localhost:7001
# Swagger UI: https://localhost:7001/swagger
```

---

## 🔑 Authentication

### JWT Token Flow

User registers/logs in
POST /api/account/login
Receives JWT token
{
"accessToken": "eyJhbGciOiJIUzI1NiIs...",
"expiresIn": 3600
}
Include in requests
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Token contains claims

sub (user ID)
email
name
role (Admin, Vendor, Customer)
custom claims (permissions, etc.)

### Default Admin

Email: admin@gmail.com
Password: P@ssw0rd
Role: Admin

---

## 📡 Real-Time Notifications (SignalR)

### Connection

```javascript
// Client-side
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/notificationHub", {
    accessTokenFactory: () => localStorage.getItem("token"),
  })
  .withAutomaticReconnect()
  .build();

connection
  .start()
  .then(() => console.log("Connected"))
  .catch((err) => console.error(err));
```

### Receiving Notifications

```javascript
// Vendor receives new purchase
connection.on("ReceiveNotification", (notification) => {
  console.log("New order:", notification);
  // { Title, Message, Type, OrderId, CreatedAt }
});

// Customer receives order update
connection.on("OrderStatusChanged", (update) => {
  console.log("Order update:", update);
  // { OrderId, Status, Message, UpdatedAt }
});
```

---

## 🔗 API Endpoints

### Authentication

POST /api/account/register/customer
POST /api/account/register/vendor
POST /api/account/login
POST /api/account/logout
GET /api/account/profile [Authorize]
POST /api/account/change-password [Authorize]
POST /api/account/deactivate [Authorize]
POST /api/account/admin/create-admin [Authorize(Admin)]
POST /api/account/admin/approve-vendor/{id} [Authorize(Admin)]
POST /api/account/admin/reject-vendor/{id} [Authorize(Admin)]

### Products

POST /api/product [Authorize(Vendor)]
GET /api/product
GET /api/product/{id}
GET /api/product/my-products [Authorize(Vendor)]
PUT /api/product/{id} [Authorize(Vendor)]
DELETE /api/product/{id} [Authorize(Vendor)]

### Categories

POST /api/category [Authorize(Admin)]
GET /api/category
GET /api/category/{id}
GET /api/category/active
GET /api/category/search?searchTerm=...
PUT /api/category/{id} [Authorize(Admin)]
DELETE /api/category/{id} [Authorize(Admin)]

### Orders

POST /api/order [Authorize(Customer)]
GET /api/order/{id} [Authorize]
GET /api/order [Authorize(Customer)]

### Reviews

POST /api/review/{productId} [Authorize(Customer)]
GET /api/review/product/{productId}
DELETE /api/review/{id} [Authorize(Customer)]

### Favorites

POST /api/favorite/{productId} [Authorize(Customer)]
GET /api/favorite [Authorize(Customer)]
DELETE /api/favorite/{productId} [Authorize(Customer)]

### Notifications

GET /api/notification/unread [Authorize]
GET /api/notification [Authorize]
PUT /api/notification/{id}/read [Authorize]
DELETE /api/notification/{id} [Authorize]
PUT /api/notification/read-all [Authorize]

### Permissions

POST /api/permission [Authorize(Admin)]
GET /api/permission [Authorize(Admin)]
GET /api/permission/vendor/{vendorId} [Authorize(Admin)]
POST /api/permission/vendor/{vendorId}/enable/{permissionType} [Admin]
POST /api/permission/vendor/{vendorId}/disable/{permissionType} [Admin]
POST /api/permission/role/enable/{permissionType} [Admin]
POST /api/permission/role/disable/{permissionType} [Admin]

### Statistics

GET /api/statistics/dashboard [Authorize(Vendor)]

---

## 🛡️ Security

- ✅ JWT token-based authentication
- ✅ Role-based authorization (Admin, Vendor, Customer)
- ✅ Granular permission system
- ✅ Password hashing with Identity
- ✅ Account lockout after failed attempts
- ✅ Data validation and sanitization
- ✅ SQL injection prevention via Entity Framework
- ✅ CORS policy configuration

---

## 📊 Database Performance

### Indexes

- Product status, name, vendor
- Order customer, status, created date
- Review/Favorite customer, product
- Notification user, read status

### Unique Constraints

- Product name per vendor
- Customer can't favorite same product twice
- Customer can't review product twice
- Vendor can't have same permission twice

---

## 🧪 Testing Workflow

### 1. Register Users

```bash
# Register as Vendor
POST /api/account/register/vendor
{
  "firstName": "Ahmed",
  "secondName": "Ali",
  "email": "vendor@example.com",
  "password": "P@ssw0rd1",
  "phoneNumber": "01234567890",
  "storeName": "My Store"
}

# Register as Customer
POST /api/account/register/customer
{
  "firstName": "Fatima",
  "secondName": "Hassan",
  "email": "customer@example.com",
  "password": "P@ssw0rd2",
  "phoneNumber": "01234567891",
  "address": "Cairo"
}
```

### 2. Login

```bash
POST /api/account/login
{
  "email": "vendor@example.com",
  "password": "P@ssw0rd1"
}
```

### 3. Create Product (Vendor)

```bash
POST /api/product
[Authorization: Bearer <vendor_token>]
{
  "name": "Laptop",
  "price": 999.99,
  "quantity": 10,
  "categoryId": 1,
  "imgUrl": "https://example.com/laptop.jpg",
  "description": "High-end laptop"
}
```

### 4. Approve Product (Admin)

```bash
POST /api/product/{id}/approve
[Authorization: Bearer <admin_token>]
```

### 5. Browse & Order (Customer)

```bash
GET /api/product
[Get approved products]

POST /api/order
[Authorization: Bearer <customer_token>]
{
  "cartItems": [
    { "productId": 1, "quantity": 2, "price": 999.99 }
  ],
  "deliveryAddress": "Cairo, Egypt",
  "phoneNumber": "01234567891"
}
```

---

## 📦 Technologies Used

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Real-time**: SignalR
- **API Documentation**: Swagger/OpenAPI
- **Validation**: FluentValidation (via ModelState)
- **Logging**: Built-in ILogger

---

## 🔄 Data Flow

### Order Creation Flow

Customer submits order
↓
OrderService validates cart
↓
Reduce product stock
↓
Create Order + OrderItems
↓
Send real-time notifications
├─ Vendor: "New purchase notification" (SignalR)
├─ Vendor: Save to database
└─ Customer: "Order confirmed" (SignalR)
↓
Return order details

### Permission Check Flow

Customer tries to upload product
↓
Authorization checks: [Authorize(Roles = "Vendor")]
✓ Passes: Is Vendor
↓
Custom Attribute checks: [RequirePermission(CanUploadProducts)]
├─ Get vendor ID from JWT
├─ Check PermissionService.HasPermissionAsync()
└─ ✓ Has permission → Allow
✗ No permission → Deny (403)
↓
Execute controller action

---

## 🐛 Troubleshooting

### Database Connection Issues

Error: "Cannot open database"
Solution: Verify connection string in appsettings.json
Ensure SQL Server is running

### JWT Token Invalid

Error: "Invalid token"
Solution: Ensure token hasn't expired
Verify secret key matches appsettings.json
Check token format in Authorization header

### SignalR Connection Failed

Error: "WebSocket connection failed"
Solution: Ensure UseWebSockets() is called in Program.cs
Check CORS policy allows SignalR
Verify token is passed correctly
