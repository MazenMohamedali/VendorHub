// src/App.jsx
import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

// تخطيطات الموقع
import MainLayout from './layouts/MainLayout';
import DashboardLayout from './layouts/DashboardLayout';
import AdminLayout from './layouts/AdminLayout'; // تخطيط الإدارة

// صفحات العميل
import Home from './pages/Home';
import ProductDetails from './pages/ProductDetails';
import Cart from './pages/Cart';
import Login from './pages/Login';
import Register from './pages/Register';
import Favorites from './pages/Favorites';
import CustomerOrders from './pages/CustomerOrders'; // <-- الصفحة الجديدة: طلبات العميل

// صفحات البائع
import VendorDashboard from './pages/VendorDashboard';
import VendorProducts from './pages/VendorProducts';
import VendorOrders from './pages/VendorOrders';
import VendorSettings from './pages/VendorSettings';

// صفحات الإدارة
import AdminVendors from './pages/AdminVendors';
import AdminProducts from './pages/AdminProducts';
import AdminCategories from './pages/AdminCategories'; // <-- الصفحة الجديدة: إدارة الأقسام

function App() {
  return (
    <BrowserRouter>
      <Routes>
        
        {/* مسارات الحسابات */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* مسارات المتجر العام (العميل) */}
        <Route path="/" element={<MainLayout />}>
          <Route index element={<Home />} />
          <Route path="product/:id" element={<ProductDetails />} />
          <Route path="cart" element={<Cart />} />
          <Route path="favorites" element={<Favorites />} />
          <Route path="my-orders" element={<CustomerOrders />} /> {/* <-- مسار طلبات العميل */}
        </Route>

        {/* مسارات البائع */}
        <Route path="/vendor" element={<DashboardLayout />}>
          <Route index element={<VendorDashboard />} />
          <Route path="products" element={<VendorProducts />} />
          <Route path="orders" element={<VendorOrders />} />
          <Route path="settings" element={<VendorSettings />} />
        </Route>

        {/* مسارات الإدارة (Admin) */}
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<AdminVendors />} />
          <Route path="products" element={<AdminProducts />} />
          <Route path="categories" element={<AdminCategories />} /> {/* <-- مسار إدارة الأقسام */}
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;