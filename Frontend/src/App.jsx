import React, { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { login, logout } from './store/authSlice';
import { authApi } from './api';
import { ToastProvider } from './components/Toast';
import ProtectedRoute from './components/ProtectedRoute';

// Layouts
import MainLayout from './layouts/MainLayout';
import DashboardLayout from './layouts/DashboardLayout';
import AdminLayout from './layouts/AdminLayout';

// Public Pages
import Home from './pages/Home';
import ProductDetails from './pages/ProductDetails';
import Cart from './pages/Cart';
import Login from './pages/Login';
import Register from './pages/Register';
import Favorites from './pages/Favorites';
import CustomerOrders from './pages/CustomerOrders';

// Vendor Pages
import VendorDashboard from './pages/VendorDashboard';
import VendorProducts from './pages/VendorProducts';
import VendorOrders from './pages/VendorOrders';
import VendorSettings from './pages/VendorSettings';

// Admin Pages
import AdminVendors from './pages/AdminVendors';
import AdminProducts from './pages/AdminProducts';
import AdminCategories from './pages/AdminCategories';
import { Loader2 } from 'lucide-react';

function App() {
  const dispatch = useDispatch();
  const [isInitializing, setIsInitializing] = useState(true);

  // Restore current session on page refresh
  useEffect(() => {
    const restoreSession = async () => {
      const token = localStorage.getItem('token');
      if (!token) {
        setIsInitializing(false);
        return;
      }

      try {
        const response = await authApi.getCurrentUser();
        if (response.data?.data) {
          dispatch(login(response.data.data));
        }
      } catch (error) {
        console.warn("Session restoration failed:", error);
        localStorage.removeItem('token');
        dispatch(logout());
      } finally {
        setIsInitializing(false);
      }
    };

    restoreSession();
  }, [dispatch]);

  if (isInitializing) {
    return (
      <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center text-emerald-600" dir="ltr">
        <div className="bg-emerald-600 w-16 h-16 rounded-2xl flex items-center justify-center text-white font-bold text-3xl mb-4 shadow-xl shadow-emerald-500/20 animate-pulse">
          V
        </div>
        <Loader2 className="animate-spin text-emerald-600" size={32} />
        <p className="font-bold text-gray-600 mt-3 text-sm">Initializing VendorHub...</p>
      </div>
    );
  }

  return (
    <ToastProvider>
      <BrowserRouter>
        <Routes>
          {/* Auth Routes */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          {/* Public Storefront Routes */}
          <Route path="/" element={<MainLayout />}>
            <Route index element={<Home />} />
            <Route path="product/:id" element={<ProductDetails />} />
            <Route path="cart" element={<Cart />} />
            <Route path="favorites" element={<Favorites />} />

            {/* Customer Protected Route */}
            <Route element={<ProtectedRoute allowedRoles={['Customer', 'Vendor', 'Admin']} />}>
              <Route path="my-orders" element={<CustomerOrders />} />
            </Route>
          </Route>

          {/* Vendor Protected Dashboard Routes */}
          <Route element={<ProtectedRoute allowedRoles={['Vendor']} />}>
            <Route path="/vendor" element={<DashboardLayout />}>
              <Route index element={<VendorDashboard />} />
              <Route path="products" element={<VendorProducts />} />
              <Route path="orders" element={<VendorOrders />} />
              <Route path="settings" element={<VendorSettings />} />
            </Route>
          </Route>

          {/* Admin Protected Control Panel Routes */}
          <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
            <Route path="/admin" element={<AdminLayout />}>
              <Route index element={<AdminVendors />} />
              <Route path="products" element={<AdminProducts />} />
              <Route path="categories" element={<AdminCategories />} />
            </Route>
          </Route>

          {/* Fallback 404 Route */}
          <Route
            path="*"
            element={
              <div className="min-h-screen flex flex-col items-center justify-center text-center p-6 bg-gray-50" dir="ltr">
                <div className="bg-gray-100 text-gray-400 w-24 h-24 rounded-full flex items-center justify-center text-4xl mb-4">
                  🔍
                </div>
                <h1 className="text-4xl font-black text-gray-800 mb-2">404</h1>
                <p className="text-gray-500 mb-6">Page not found.</p>
                <a href="/" className="bg-emerald-600 text-white px-6 py-3 rounded-xl font-bold hover:bg-emerald-700 transition-all">
                  Back to Home
                </a>
              </div>
            }
          />
        </Routes>
      </BrowserRouter>
    </ToastProvider>
  );
}

export default App;