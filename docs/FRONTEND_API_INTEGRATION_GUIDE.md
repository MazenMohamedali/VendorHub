# 📘 VendorHub Web API - Frontend Integration & Complete API Reference Guide

Welcome to the official **VendorHub Web API Frontend Integration Guide**. This document provides an exhaustive, production-grade API reference for frontend developers, mobile engineers, and integration partners.

---

## 1. ⚙️ Base URL & Request Headers

### Environment Base URLs
- **Local Development**: `https://localhost:7081/api` (or `http://localhost:5131/api`)
- **Docker Container**: `http://localhost:7081/api`
- **Production Domain**: `https://api.yourdomain.com/api`

### Mandatory Request Headers

```http
Content-Type: application/json
Accept: application/json
Authorization: Bearer <JWT_ACCESS_TOKEN>
```

> ⚠️ **Note for File Uploads**: Endpoints expecting image files (e.g., `POST /api/Product`, `POST /api/Category`) require `Content-Type: multipart/form-data`. Do **NOT** manually set `Content-Type` header when sending `FormData` objects in Axios/Fetch; the browser will automatically add the boundary string.

---

## 2. 🧱 Standardized Response Format (`GeneralResponse<T>`)

Every API endpoint yields a unified JSON payload structure:

### Single Entity Response (`200 OK` / `201 Created`)
```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": {
    "id": 1,
    "name": "Wireless Noise-Canceling Headphones",
    "price": 199.99,
    "stock": 50,
    "imageUrl": "/Images/Products/sample.png"
  }
}
```

### Paginated List Response (`PagedResult<T>`)
```json
{
  "success": true,
  "message": null,
  "data": {
    "items": [
      { "id": 1, "name": "Headphones", "price": 199.99 },
      { "id": 2, "name": "Mechanical Keyboard", "price": 129.99 }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 10,
    "totalPages": 15,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Error Response (`400 Bad Request` / `404 Not Found` / `403 Forbidden`)
```json
{
  "success": false,
  "message": "Product with ID 999 not found.",
  "data": null
}
```

---

## 3. 🚥 HTTP Status Code Mapping Reference

| Status Code | Meaning | Cause / Description |
| :--- | :--- | :--- |
| **`200 OK`** | Success | Request processed successfully. Check `data` property. |
| **`201 Created`** | Created | Resource successfully created (User, Product, Order, Review). |
| **`400 Bad Request`** | Invalid Input | Validation failed or business rule violation. |
| **`401 Unauthorized`** | Authentication Required | JWT token is missing, invalid, or expired. |
| **`403 Forbidden`** | Permission Denied | User lacks required role (`Admin`/`Vendor`/`Customer`) or claim (`[RequirePermission]`). |
| **`404 Not Found`** | Not Found | Requested entity record does not exist. |
| **`500 Internal Error`** | Server Error | Unhandled exception caught by global handler (RFC 7807 ProblemDetails returned). |

---

## 4. 📚 Exhaustive Endpoints Reference

---

### 🔑 1. Authentication & Account Management (`/api/Account`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/Account/register/customer` | Public | `RegisterCustomerDto` | Register a new Customer account |
| `POST` | `/api/Account/register/vendor` | Public | `RegisterVendorDto` | Register a new Vendor account (Starts in `PENDING` state) |
| `POST` | `/api/Account/register/admin` | `Admin` | `RegisterUserDto` | Provision a new Admin account |
| `POST` | `/api/Account/login` | Public | `LoginDto` | Authenticate user and receive JWT bearer token |
| `POST` | `/api/Account/logout` | `Auth` | None | Invalidate active session |
| `GET` | `/api/Account/me` | `Auth` | None | Get current authenticated user profile & claims |
| `POST` | `/api/Account/change-password` | `Auth` | `ChangePasswordDto` | Update user account password |
| `PATCH` | `/api/Account/approve-vendor/{id}` | `Admin` | `id` (path) | Approve pending vendor registration |
| `PATCH` | `/api/Account/reject-vendor/{id}` | `Admin` | `id` (path) | Reject pending vendor registration |
| `DELETE`| `/api/Account/deactivate/{id}` | `Admin` | `id` (path) | Soft-deactivate a user account |

#### Example: Customer Registration Payload (`POST /api/Account/register/customer`)
```json
{
  "firstName": "John",
  "secondName": "Doe",
  "email": "john.doe@example.com",
  "password": "Password123!",
  "phoneNumber": "01012345678",
  "address": "123 Market Street, Cairo, Egypt"
}
```

---

### 🛡️ 2. Admin Governance & Moderation (`/api/Admin`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Admin/admin/all` | `Admin` | `page=1`, `pageSize=10` | Get paginated list of all products for admin review |
| `GET` | `/api/Admin/{id}/admin` | `Admin` | `id` (path) | Get comprehensive product details for admin audit |
| `PATCH` | `/api/Admin/{id}/approve` | `Admin` | `id` (path) | Approve product for live catalog display |
| `PATCH` | `/api/Admin/{id}/reject` | `Admin` | `id` (path) | Reject submitted product |

---

### 🏬 3. Category Catalog Management (`/api/Category`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Category/active` | Public | None | Get all active categories (Cached in L1/L2) |
| `GET` | `/api/Category/{id}` | Public | `id` (path) | Get category details by ID |
| `GET` | `/api/Category/search` | Public | `searchTerm` (query) | Search categories by name |
| `GET` | `/api/Category/admin/all` | `Admin` | `pageNumber=1`, `pageSize=10` | Admin view of all categories (active & inactive) |
| `POST` | `/api/Category` | `Admin` | `multipart/form-data` (`CreateCategoryDto`) | Create a new category |
| `PUT` | `/api/Category/{id}` | `Admin` | `multipart/form-data` (`UpdateCategoryDto`) | Update existing category |
| `DELETE`| `/api/Category/{id}` | `Admin` | `id` (path) | Soft-delete category |
| `DELETE`| `/api/Category/{id}/hard` | `Admin` | `id` (path) | Permanent hard-delete category |

---

### 🛍️ 4. Customer Profile Management (`/api/Customer`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Customer/profile` | `Customer` | None | Get logged-in customer profile |
| `PUT` | `/api/Customer/profile` | `Customer` | `UpdateCustomerProfileDto` | Update address and personal details |

---

### 🏪 5. Vendor Storefront Management (`/api/Vendor`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Vendor/profile` | `Vendor` | None | Get logged-in vendor store profile |
| `PUT` | `/api/Vendor/profile` | `Vendor` | `UpdateVendorProfileDto` | Update store name, bio, and contact info |
| `GET` | `/api/Vendor` | `Admin` | `page=1`, `pageSize=10` | Get paginated list of all vendors (Admin view) |

---

### 📦 6. Product Catalog & Search (`/api/Product`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Product/list` | Public | `page=1`, `pageSize=10` | Get active public product cards |
| `GET` | `/api/Product/hot-products` | Public | `count=6` | Get featured hot products |
| `GET` | `/api/Product/{id}/customer` | Public | `id` (path) | Get full product details for customers |
| `GET` | `/api/Product/category/{categoryId}` | Public | `categoryId`, `page`, `pageSize` | Filter products by category ID |
| `GET` | `/api/Product/search-name` | Public | `name`, `page`, `pageSize` | Search products by name keyword |
| `GET` | `/api/Product/search-category` | Public | `category`, `page`, `pageSize` | Search products by category name |
| `GET` | `/api/Product/search-price` | Public | `min`, `max`, `page`, `pageSize` | Filter products by price range |
| `POST` | `/api/Product` | `Vendor` + `CanUploadProducts` | `multipart/form-data` (`AddProductDto`) | Upload a new product |
| `GET` | `/api/Product/my-products` | `Vendor` | `page=1`, `pageSize=10` | Get vendor's own uploaded products |
| `GET` | `/api/Product/{id}/vendor` | `Vendor` | `id` (path) | Get product details for vendor management |
| `PUT` | `/api/Product/{id}` | `Vendor` + `CanEditProducts` | `multipart/form-data` (`EditProductDto`) | Update product details or stock |
| `DELETE`| `/api/Product/{id}` | `Vendor`/`Admin` + `CanDeleteProducts` | `id` (path) | Soft-delete product |

---

### 🛒 7. Orders & Checkout Workflow (`/api/Order`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/Order` | `Customer` | `CreateOrderDto` | Checkout cart items and place order |
| `GET` | `/api/Order/my-orders` | `Customer` | `pageNumber=1`, `pageSize=10` | Get customer's order history |
| `GET` | `/api/Order/{orderId}` | `Customer` | `orderId` (path) | Get customer order invoice details |
| `GET` | `/api/Order/vendor-orders` | `Vendor` | `page`, `pageSize`, `statusFilter` | Get vendor incoming orders |
| `GET` | `/api/Order/vendor-orders/{orderId}` | `Vendor` | `orderId` (path) | Get vendor order item details |
| `PATCH` | `/api/Order/{orderId}/status` | `Vendor` | `UpdateOrderStatusDto` | Update status (`Processing`, `Shipped`, `Delivered`) |
| `GET` | `/api/Order/vendor-orders-stats` | `Vendor` | None | Get vendor order revenue & status metrics |

#### Example: Order Creation Payload (`POST /api/Order`)
```json
{
  "deliveryAddress": "45 Nile Street, Cairo, Egypt",
  "phoneNumber": "01012345678",
  "cartItems": [
    { "productId": 1, "quantity": 2 },
    { "productId": 4, "quantity": 1 }
  ]
}
```

---

### ❤️ 8. Favorites & Wishlist (`/api/Favorite`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Favorite` | `Customer` | None | Get customer saved wishlist products |
| `POST` | `/api/Favorite/product/{productId}` | `Customer` | `productId` (path) | Add product to wishlist |
| `DELETE`| `/api/Favorite/product/{productId}` | `Customer` | `productId` (path) | Remove product from wishlist |

---

### ⭐ 9. Ratings & Reviews (`/api/Review`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/Review/{productId}` | `Customer` | `CreateReviewDto` | Submit star rating (1-5) and review |
| `GET` | `/api/Review/{productId}` | Public | `productId`, `page`, `pageSize` | Get paginated reviews for a product |

---

### 🔔 10. Notifications Management (`/api/Notifications`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Notifications` | `Auth` | `pageNumber=1`, `pageSize=10` | Get paginated notification history |
| `GET` | `/api/Notifications/unread` | `Auth` | None | Get unread notification list |
| `PUT` | `/api/Notifications/{id}/mark-read` | `Auth` | `id` (path) | Mark single notification as read |
| `PUT` | `/api/Notifications/mark-all-read` | `Auth` | None | Mark all notifications as read |
| `DELETE`| `/api/Notifications/{id}` | `Auth` | `id` (path) | Delete notification entry |

---

### 🔐 11. Claims-Based Permission Engine (`/api/Permission`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Permission` | `Admin` | None | Get list of all system permissions |
| `GET` | `/api/Permission/vendor/{vendorId}` | `Admin` | `vendorId` (path) | Get permissions enabled for specific vendor |
| `POST` | `/api/Permission/vendor/{vendorId}/enable/{permissionType}` | `Admin` | `vendorId`, `permissionType` | Grant claim to vendor |
| `POST` | `/api/Permission/vendor/{vendorId}/disable/{permissionType}` | `Admin` | `vendorId`, `permissionType` | Revoke claim from vendor |
| `POST` | `/api/Permission/global/enable/{permissionType}` | `Admin` | `permissionType` | Grant permission globally |
| `POST` | `/api/Permission/global/disable/{permissionType}` | `Admin` | `permissionType` | Revoke permission globally |

---

### 📊 12. Analytics & Business Intelligence (`/api/Statistics`)

| Method | Endpoint | Auth | Request Body / Params | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Statistics/vendor/{vendorId}` | `Vendor`/`Admin` | `vendorId` (path) | Get store performance, revenue & order metrics |

---

## 5. 💬 Real-Time SignalR Websocket Integration

VendorHub provides a real-time SignalR WebSocket hub for instant alerts.

### Websocket Connection Setup
- **Hub URL**: `https://localhost:7081/hubs/notification`
- **Transport**: WebSockets / Long Polling
- **Authentication**: Pass JWT token via query parameter `?access_token=<JWT_TOKEN>`

### Frontend SignalR Integration Example (JavaScript / React):

```javascript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7081/hubs/notification', {
    accessTokenFactory: () => localStorage.getItem('token')
  })
  .withAutomaticReconnect()
  .build();

// Listen for incoming notifications
connection.on('ReceiveNotification', (notification) => {
  console.log('New Alert Received:', notification);
  // e.g. toast.info(notification.message);
});

// Start connection
connection.start()
  .then(() => console.log('SignalR Connected!'))
  .catch(err => console.error('SignalR Connection Error:', err));
```
