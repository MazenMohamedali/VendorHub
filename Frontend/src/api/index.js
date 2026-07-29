import axiosInstance from './axiosConfig';

// ==========================================
// 1. ACCOUNT & AUTHENTICATION API
// ==========================================
export const authApi = {
  login: (data) => axiosInstance.post('/Account/login', data),
  registerCustomer: (data) => axiosInstance.post('/Account/register/customer', data),
  registerVendor: (data) => axiosInstance.post('/Account/register/vendor', data),
  registerAdmin: (data) => axiosInstance.post('/Account/register/admin', data),
  logout: () => axiosInstance.post('/Account/logout'),
  getCurrentUser: () => axiosInstance.get('/Account/me'),
  changePassword: (data) => axiosInstance.post('/Account/change-password', data),
  approveVendor: (vendorId) => axiosInstance.patch(`/Account/approve-vendor/${vendorId}`),
  rejectVendor: (vendorId) => axiosInstance.patch(`/Account/reject-vendor/${vendorId}`),
  deactivateUser: (userId) => axiosInstance.delete(`/Account/deactivate/${userId}`),
};

// ==========================================
// 2. PRODUCT API
// ==========================================
export const productApi = {
  getHotProducts: (count = 6) => axiosInstance.get(`/Product/hot-products?count=${count}`),
  getPublicProducts: (page = 1, pageSize = 10) => axiosInstance.get(`/Product/list?page=${page}&pageSize=${pageSize}`),
  getProductsByCategory: (categoryId, page = 1, pageSize = 10) =>
    axiosInstance.get(`/Product/category/${categoryId}?page=${page}&pageSize=${pageSize}`),
  getProductDetailsCustomer: (id) => axiosInstance.get(`/Product/${id}/customer`),
  searchByName: (name, page = 1, pageSize = 10) =>
    axiosInstance.get(`/Product/search-name`, { params: { name, page, pageSize } }),
  searchByCategory: (category, page = 1, pageSize = 10) =>
    axiosInstance.get(`/Product/search-category`, { params: { category, page, pageSize } }),
  searchByPrice: (min, max, page = 1, pageSize = 10) =>
    axiosInstance.get(`/Product/search-price`, { params: { min, max, page, pageSize } }),
  getMyProducts: (page = 1, pageSize = 10) => axiosInstance.get(`/Product/my-products?page=${page}&pageSize=${pageSize}`),
  getProductDetailsVendor: (id) => axiosInstance.get(`/Product/${id}/vendor`),
  createProduct: (formData) =>
    axiosInstance.post('/Product', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),
  updateProduct: (id, formData) =>
    axiosInstance.put(`/Product/${id}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),
  deleteProduct: (id) => axiosInstance.delete(`/Product/${id}`),
};

// ==========================================
// 3. CATEGORY API
// ==========================================
export const categoryApi = {
  getActiveCategories: () => axiosInstance.get('/Category/active'),
  getCategoryById: (id) => axiosInstance.get(`/Category/${id}`),
  searchCategories: (searchTerm) => axiosInstance.get('/Category/search', { params: { searchTerm } }),
  getAllCategoriesAdmin: (pageNumber = 1, pageSize = 10) =>
    axiosInstance.get(`/Category/admin/all?pageNumber=${pageNumber}&pageSize=${pageSize}`),
  createCategory: (formData) =>
    axiosInstance.post('/Category', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),
  updateCategory: (id, formData) =>
    axiosInstance.put(`/Category/${id}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),
  deleteCategory: (id) => axiosInstance.delete(`/Category/${id}`),
  hardDeleteCategory: (id) => axiosInstance.delete(`/Category/${id}/hard`),
};

// ==========================================
// 4. ORDER API
// ==========================================
export const orderApi = {
  createOrder: (data) => axiosInstance.post('/Order', data),
  getCustomerOrders: (pageNumber = 1, pageSize = 10) =>
    axiosInstance.get(`/Order/my-orders?pageNumber=${pageNumber}&pageSize=${pageSize}`),
  getOrderDetails: (orderId) => axiosInstance.get(`/Order/${orderId}`),
  getVendorOrders: (page = 1, pageSize = 10, statusFilter = null) =>
    axiosInstance.get('/Order/vendor-orders', {
      params: { page, pageSize, statusFilter: statusFilter || undefined },
    }),
  getVendorOrderById: (orderId) => axiosInstance.get(`/Order/vendor-orders/${orderId}`),
  updateOrderStatus: (orderId, status) => axiosInstance.patch(`/Order/${orderId}/status`, { status }),
  getVendorOrdersStats: () => axiosInstance.get('/Order/vendor-orders-stats'),
};

// ==========================================
// 5. VENDOR & CUSTOMER PROFILE API
// ==========================================
export const vendorApi = {
  getProfile: () => axiosInstance.get('/Vendor/profile'),
  updateProfile: (data) => axiosInstance.put('/Vendor/profile', data),
  getVendorsAdmin: (page = 1, pageSize = 10) => axiosInstance.get(`/Vendor?page=${page}&pageSize=${pageSize}`),
};

export const customerApi = {
  getProfile: () => axiosInstance.get('/Customer/profile'),
  updateProfile: (data) => axiosInstance.put('/Customer/profile', data),
};

// ==========================================
// 6. FAVORITE API
// ==========================================
export const favoriteApi = {
  getFavorites: () => axiosInstance.get('/Favorite'),
  addFavorite: (productId) => axiosInstance.post(`/Favorite/product/${productId}`),
  removeFavorite: (productId) => axiosInstance.delete(`/Favorite/product/${productId}`),
};

// ==========================================
// 7. REVIEW API
// ==========================================
export const reviewApi = {
  getProductReviews: (productId, page = 1, pageSize = 10) =>
    axiosInstance.get(`/Review/${productId}?page=${page}&pageSize=${pageSize}`),
  addReview: (productId, data) => axiosInstance.post(`/Review/${productId}`, data),
};

// ==========================================
// 8. NOTIFICATION API
// ==========================================
export const notificationApi = {
  getHistory: (pageNumber = 1, pageSize = 10) =>
    axiosInstance.get(`/Notifications?pageNumber=${pageNumber}&pageSize=${pageSize}`),
  getUnread: () => axiosInstance.get('/Notifications/unread'),
  markAsRead: (notificationId) => axiosInstance.put(`/Notifications/${notificationId}/mark-read`),
  markAllAsRead: () => axiosInstance.put('/Notifications/mark-all-read'),
  deleteNotification: (notificationId) => axiosInstance.delete(`/Notifications/${notificationId}`),
};

// ==========================================
// 9. PERMISSION API (ADMIN)
// ==========================================
export const permissionApi = {
  getAllPermissions: () => axiosInstance.get('/Permission'),
  getVendorPermissions: (vendorId) => axiosInstance.get(`/Permission/vendor/${vendorId}`),
  enableForVendor: (vendorId, permissionType) =>
    axiosInstance.post(`/Permission/vendor/${vendorId}/enable/${permissionType}`),
  disableForVendor: (vendorId, permissionType) =>
    axiosInstance.post(`/Permission/vendor/${vendorId}/disable/${permissionType}`),
  enableGlobally: (permissionType) => axiosInstance.post(`/Permission/global/enable/${permissionType}`),
  disableGlobally: (permissionType) => axiosInstance.post(`/Permission/global/disable/${permissionType}`),
};

// ==========================================
// 10. STATISTICS API
// ==========================================
export const statisticsApi = {
  getVendorStatistics: (vendorId) => axiosInstance.get(`/Statistics/vendor/${vendorId}`),
};

// ==========================================
// 11. ADMIN PRODUCT MANAGEMENT API
// ==========================================
export const adminApi = {
  getProductDetails: (id) => axiosInstance.get(`/Admin/${id}/admin`),
  getAllProducts: (page = 1, pageSize = 10) => axiosInstance.get(`/Admin/admin/all?page=${page}&pageSize=${pageSize}`),
  approveProduct: (id) => axiosInstance.patch(`/Admin/${id}/approve`),
  rejectProduct: (id) => axiosInstance.patch(`/Admin/${id}/reject`),
};
