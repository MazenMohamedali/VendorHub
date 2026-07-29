import React, { useState, useRef, useEffect } from 'react';
import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { LayoutDashboard, Package, ShoppingBag, Settings, LogOut, Store, ArrowLeft, Menu, X, User, Shield } from 'lucide-react';
import { useDispatch, useSelector } from 'react-redux';
import { logout } from '../store/authSlice';

const DashboardLayout = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const userMenuRef = useRef(null);
  
  const user = useSelector((state) => state.auth.user);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (userMenuRef.current && !userMenuRef.current.contains(event.target)) {
        setIsUserMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  const userRoles = Array.isArray(user?.roles) ? user.roles : user?.role ? [user.role] : [];
  const isAdmin = userRoles.includes('Admin');

  const menuItems = [
    { path: '/vendor', icon: <LayoutDashboard size={20} />, label: 'Dashboard' },
    { path: '/vendor/products', icon: <Package size={20} />, label: 'My Products' },
    { path: '/vendor/orders', icon: <ShoppingBag size={20} />, label: 'Store Orders' },
    { path: '/vendor/settings', icon: <Settings size={20} />, label: 'Store Settings' },
  ];

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col md:flex-row font-sans text-gray-800" dir="ltr">
      
      {/* Desktop Sidebar */}
      <aside className="w-64 bg-gray-900 text-white flex-col hidden md:flex fixed h-full left-0 z-30 shadow-xl">
        <div className="p-6 flex items-center justify-between border-b border-gray-800">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-emerald-500/10 border border-emerald-500/20 rounded-xl flex items-center justify-center text-emerald-400">
              <Store size={22} />
            </div>
            <div className="overflow-hidden">
              <h1 className="text-base font-black tracking-tight text-white truncate">{user?.storeName || 'My Store'}</h1>
              <p className="text-xs text-emerald-400 font-semibold">Vendor Portal</p>
            </div>
          </div>
        </div>
        
        <nav className="flex-1 p-4 space-y-1.5 mt-2">
          {menuItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all ${
                  isActive 
                    ? 'bg-emerald-600 text-white font-bold shadow-lg shadow-emerald-900/40' 
                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                }`}
              >
                {item.icon}
                <span>{item.label}</span>
              </Link>
            );
          })}
        </nav>

        <div className="p-4 border-t border-gray-800 space-y-2">
          <Link 
            to="/" 
            className="flex items-center gap-3 w-full px-4 py-2.5 text-gray-300 hover:bg-emerald-600/20 hover:text-white rounded-xl transition-colors text-sm font-semibold"
          >
            <ArrowLeft size={18} />
            <span>Back to Main Store</span>
          </Link>

          <button 
            onClick={handleLogout}
            className="flex items-center gap-3 w-full px-4 py-2.5 text-red-400 hover:bg-red-500/10 rounded-xl transition-colors text-sm font-bold"
          >
            <LogOut size={18} />
            <span>Sign Out</span>
          </button>
        </div>
      </aside>

      {/* Mobile Header Sidebar Drawer */}
      {isMobileMenuOpen && (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm md:hidden flex" onClick={() => setIsMobileMenuOpen(false)}>
          <div className="w-72 bg-gray-900 text-white h-full flex flex-col p-6 animate-fade-in-right" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between pb-6 border-b border-gray-800">
              <div className="flex items-center gap-3">
                <Store className="text-emerald-400" size={24} />
                <span className="font-bold text-lg text-white">Vendor Navigation</span>
              </div>
              <button onClick={() => setIsMobileMenuOpen(false)} className="text-gray-400 hover:text-white">
                <X size={24} />
              </button>
            </div>

            <nav className="flex-1 py-6 space-y-2">
              {menuItems.map((item) => {
                const isActive = location.pathname === item.path;
                return (
                  <Link
                    key={item.path}
                    to={item.path}
                    onClick={() => setIsMobileMenuOpen(false)}
                    className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all ${
                      isActive 
                        ? 'bg-emerald-600 text-white font-bold shadow-md' 
                        : 'text-gray-300 hover:bg-gray-800'
                    }`}
                  >
                    {item.icon}
                    <span>{item.label}</span>
                  </Link>
                );
              })}
            </nav>

            <div className="pt-6 border-t border-gray-800 space-y-3">
              <Link 
                to="/" 
                onClick={() => setIsMobileMenuOpen(false)}
                className="flex items-center justify-center gap-2 w-full py-3 bg-emerald-600 text-white rounded-xl font-bold text-sm"
              >
                <Store size={18} />
                Back to Main Store
              </Link>
              <button 
                onClick={handleLogout}
                className="flex items-center justify-center gap-2 w-full py-3 bg-red-500/10 text-red-400 rounded-xl font-bold text-sm"
              >
                <LogOut size={18} />
                Sign Out
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Main Content Area */}
      <main className="flex-1 md:ml-64 transition-all duration-300 min-h-screen flex flex-col">
        
        {/* Sticky Header */}
        <header className="bg-white border-b border-gray-200 h-20 flex items-center justify-between px-4 sm:px-8 sticky top-0 z-20 shadow-sm">
          <div className="flex items-center gap-3">
            <button 
              onClick={() => setIsMobileMenuOpen(true)}
              className="p-2 text-gray-600 hover:text-emerald-600 rounded-lg md:hidden hover:bg-gray-100"
            >
              <Menu size={24} />
            </button>
            <h2 className="text-lg sm:text-xl font-black text-gray-800">
              {menuItems.find(item => item.path === location.pathname)?.label || 'Vendor Dashboard'}
            </h2>
          </div>

          <div className="flex items-center gap-3 sm:gap-4">
            {/* Prominent Back To Main Store Button */}
            <Link 
              to="/" 
              className="flex items-center gap-2 px-3 sm:px-4 py-2 bg-emerald-50 text-emerald-700 hover:bg-emerald-600 hover:text-white rounded-xl font-bold text-xs sm:text-sm transition-all shadow-sm border border-emerald-100"
            >
              <Store size={16} />
              <span>Exit to Store</span>
            </Link>

            {/* Clickable Interactive User Profile Menu */}
            <div ref={userMenuRef} className="relative">
              <button
                onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                className="flex items-center gap-3 p-1.5 pl-3 rounded-2xl hover:bg-gray-100 transition-all border border-gray-200 cursor-pointer group"
              >
                <div className="hidden sm:flex flex-col text-right">
                  <p className="font-bold text-gray-800 text-xs sm:text-sm line-clamp-1 group-hover:text-emerald-600 transition-colors">
                    {user?.firstName || 'Vendor'}
                  </p>
                  <p className="text-[10px] text-emerald-600 font-semibold truncate max-w-[140px]">{user?.storeName || 'Verified Vendor'}</p>
                </div>

                <div className="w-9 h-9 sm:w-10 sm:h-10 bg-emerald-600 text-white rounded-xl flex items-center justify-center font-bold text-sm shadow-md shadow-emerald-500/20 shrink-0">
                  {user?.firstName?.charAt(0) || 'V'}
                </div>
              </button>

              {/* User Dropdown Menu */}
              {isUserMenuOpen && (
                <div className="absolute right-0 mt-3 w-60 bg-white rounded-2xl shadow-xl border border-gray-100 z-50 overflow-hidden animate-fade-in-up py-2" dir="ltr">
                  <div className="px-4 py-3 border-b border-gray-100 bg-gray-50/50">
                    <p className="font-bold text-sm text-gray-800">{user?.firstName} {user?.secondName || ''}</p>
                    <p className="text-xs text-gray-400 truncate">{user?.email}</p>
                  </div>

                  <Link
                    to="/"
                    onClick={() => setIsUserMenuOpen(false)}
                    className="flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-gray-700 hover:bg-emerald-50 hover:text-emerald-600 transition-colors"
                  >
                    <Store size={18} /> Return to Store
                  </Link>

                  {isAdmin && (
                    <Link
                      to="/admin"
                      onClick={() => setIsUserMenuOpen(false)}
                      className="flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-purple-600 hover:bg-purple-50 transition-colors"
                    >
                      <Shield size={18} /> Admin Panel
                    </Link>
                  )}

                  <Link
                    to="/my-orders"
                    onClick={() => setIsUserMenuOpen(false)}
                    className="flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-gray-700 hover:bg-emerald-50 hover:text-emerald-600 transition-colors"
                  >
                    <Package size={18} /> Customer Orders
                  </Link>

                  <div className="border-t border-gray-100 my-1"></div>

                  <button
                    onClick={handleLogout}
                    className="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-red-600 hover:bg-red-50 transition-colors text-left"
                  >
                    <LogOut size={18} /> Sign Out
                  </button>
                </div>
              )}
            </div>

          </div>
        </header>

        {/* Dynamic Route Content */}
        <div className="p-4 sm:p-8 flex-1">
          <Outlet />
        </div>
      </main>
      
    </div>
  );
};

export default DashboardLayout;