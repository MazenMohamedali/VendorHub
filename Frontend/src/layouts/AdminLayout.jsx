import React, { useState, useRef, useEffect } from 'react';
import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { Users, Package, LogOut, ShieldCheck, FolderTree, Store, Menu, X, ArrowLeft, Sparkles, LayoutDashboard, User } from 'lucide-react';
import { useDispatch, useSelector } from 'react-redux';
import { logout } from '../store/authSlice';

const AdminLayout = () => {
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

  const menuItems = [
    { path: '/admin', icon: <Users size={20} />, label: 'Vendor Management', badge: 'Vendors' },
    { path: '/admin/products', icon: <Package size={20} />, label: 'Product Review', badge: 'Catalog' },
    { path: '/admin/categories', icon: <FolderTree size={20} />, label: 'Category Management', badge: 'Categories' },
  ];

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col md:flex-row font-sans text-slate-800" dir="ltr">
      
      {/* Desktop Sidebar */}
      <aside className="w-72 bg-slate-900 text-white flex-col hidden md:flex fixed h-full left-0 z-30 shadow-2xl border-r border-slate-800/80">
        
        {/* Brand Header */}
        <div className="p-6 flex items-center justify-between border-b border-slate-800/80 bg-slate-950/40">
          <Link to="/" className="flex items-center gap-3 group">
            <div className="w-11 h-11 bg-gradient-to-tr from-emerald-600 to-teal-400 rounded-2xl flex items-center justify-center text-white font-black text-xl shadow-lg shadow-emerald-500/20 group-hover:scale-105 transition-transform duration-300">
              V
            </div>
            <div>
              <div className="flex items-center gap-1.5">
                <span className="text-lg font-black tracking-tight text-white">VendorHub</span>
                <Sparkles size={14} className="text-emerald-400" />
              </div>
              <p className="text-[11px] text-emerald-400 font-bold uppercase tracking-wider">Admin Portal</p>
            </div>
          </Link>
        </div>
        
        {/* Nav Links */}
        <div className="px-4 py-6">
          <p className="px-4 text-[10px] font-black uppercase tracking-widest text-slate-400 mb-3">Management</p>
          <nav className="space-y-1.5">
            {menuItems.map((item) => {
              const isActive = location.pathname === item.path;
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  className={`flex items-center justify-between px-4 py-3.5 rounded-2xl transition-all duration-200 group ${
                    isActive 
                      ? 'bg-emerald-600 text-white font-bold shadow-lg shadow-emerald-600/30' 
                      : 'text-slate-400 hover:bg-slate-800/60 hover:text-slate-100 font-medium'
                  }`}
                >
                  <div className="flex items-center gap-3.5">
                    <div className={`transition-transform duration-200 group-hover:scale-110 ${isActive ? 'text-white' : 'text-slate-400 group-hover:text-emerald-400'}`}>
                      {item.icon}
                    </div>
                    <span className="text-sm">{item.label}</span>
                  </div>
                  <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold uppercase ${isActive ? 'bg-emerald-700/80 text-white' : 'bg-slate-800 text-slate-400'}`}>
                    {item.badge}
                  </span>
                </Link>
              );
            })}
          </nav>
        </div>

        {/* Footer Actions */}
        <div className="mt-auto p-4 border-t border-slate-800/80 bg-slate-950/40 space-y-2">
          <Link 
            to="/" 
            className="flex items-center gap-3 w-full px-4 py-3 text-slate-300 hover:bg-slate-800/70 hover:text-white rounded-2xl transition-colors text-sm font-semibold border border-slate-800/50"
          >
            <ArrowLeft size={18} className="text-emerald-400" />
            <span>Return to Main Store</span>
          </Link>

          <button 
            onClick={handleLogout}
            className="flex items-center gap-3 w-full px-4 py-3 text-rose-400 hover:bg-rose-500/10 hover:text-rose-300 rounded-2xl transition-colors text-sm font-bold"
          >
            <LogOut size={18} />
            <span>Sign Out</span>
          </button>
        </div>
      </aside>

      {/* Mobile Drawer */}
      {isMobileMenuOpen && (
        <div className="fixed inset-0 z-50 bg-slate-950/70 backdrop-blur-md md:hidden flex" onClick={() => setIsMobileMenuOpen(false)}>
          <div className="w-80 bg-slate-900 text-white h-full flex flex-col p-6 shadow-2xl animate-fade-in-right border-r border-slate-800" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between pb-6 border-b border-slate-800">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-emerald-600 rounded-xl flex items-center justify-center font-black text-lg text-white">V</div>
                <div>
                  <span className="font-black text-lg text-white block">Admin Panel</span>
                  <span className="text-xs text-emerald-400 font-semibold">VendorHub Platform</span>
                </div>
              </div>
              <button onClick={() => setIsMobileMenuOpen(false)} className="text-slate-400 hover:text-white p-2 rounded-xl bg-slate-800">
                <X size={20} />
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
                    className={`flex items-center justify-between px-4 py-3.5 rounded-2xl transition-all ${
                      isActive 
                        ? 'bg-emerald-600 text-white font-bold shadow-lg shadow-emerald-600/30' 
                        : 'text-slate-300 hover:bg-slate-800'
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      {item.icon}
                      <span className="text-sm">{item.label}</span>
                    </div>
                  </Link>
                );
              })}
            </nav>

            <div className="pt-6 border-t border-slate-800 space-y-3">
              <Link 
                to="/" 
                onClick={() => setIsMobileMenuOpen(false)}
                className="flex items-center justify-center gap-2 w-full py-3.5 bg-emerald-600 text-white rounded-2xl font-bold text-sm shadow-md"
              >
                <Store size={18} />
                Return to Store
              </Link>
              <button 
                onClick={handleLogout}
                className="flex items-center justify-center gap-2 w-full py-3.5 bg-rose-500/10 text-rose-400 rounded-2xl font-bold text-sm"
              >
                <LogOut size={18} />
                Sign Out
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Main Content Area */}
      <main className="flex-1 md:ml-72 transition-all duration-300 min-h-screen flex flex-col">
        
        {/* Header */}
        <header className="bg-white/80 backdrop-blur-md border-b border-slate-200/80 h-20 flex items-center justify-between px-4 sm:px-8 sticky top-0 z-20 shadow-sm">
          <div className="flex items-center gap-3">
            <button 
              onClick={() => setIsMobileMenuOpen(true)}
              className="p-2 text-slate-600 hover:text-emerald-600 rounded-xl md:hidden hover:bg-slate-100"
            >
              <Menu size={24} />
            </button>
            <div>
              <h2 className="text-lg sm:text-xl font-black text-slate-900 tracking-tight">
                {menuItems.find(item => item.path === location.pathname)?.label || 'System Administration'}
              </h2>
              <p className="text-xs text-slate-400 font-medium hidden sm:block">Control center for vendor permissions, product vetting & categories</p>
            </div>
          </div>

          <div className="flex items-center gap-3 sm:gap-4">
            <Link 
              to="/" 
              className="flex items-center gap-2 px-3.5 py-2 bg-emerald-50 text-emerald-700 hover:bg-emerald-600 hover:text-white rounded-xl font-bold text-xs sm:text-sm transition-all shadow-sm border border-emerald-200/60"
            >
              <Store size={16} />
              <span>Exit to Main App</span>
            </Link>

            {/* Clickable Interactive User Profile Menu */}
            <div ref={userMenuRef} className="relative">
              <button
                onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                className="flex items-center gap-3 p-1.5 pl-3 rounded-2xl hover:bg-slate-100 transition-all border border-slate-200 cursor-pointer group"
              >
                <div className="hidden sm:flex flex-col text-right">
                  <p className="font-black text-slate-900 text-xs sm:text-sm group-hover:text-emerald-600 transition-colors">
                    {user?.firstName || 'Super Admin'}
                  </p>
                  <span className="text-[10px] text-emerald-600 font-bold uppercase tracking-wider">System Administrator</span>
                </div>

                <div className="w-10 h-10 bg-gradient-to-br from-emerald-500 to-teal-700 text-white rounded-xl flex items-center justify-center font-black text-sm shadow-md shadow-emerald-500/20 shrink-0">
                  {user?.firstName?.charAt(0) || 'A'}
                </div>
              </button>

              {/* User Dropdown Menu */}
              {isUserMenuOpen && (
                <div className="absolute right-0 mt-3 w-60 bg-white rounded-2xl shadow-xl border border-slate-100 z-50 overflow-hidden animate-fade-in-up py-2" dir="ltr">
                  <div className="px-4 py-3 border-b border-slate-100 bg-slate-50/50">
                    <p className="font-bold text-sm text-slate-800">{user?.firstName} {user?.secondName || ''}</p>
                    <p className="text-xs text-slate-400 truncate">{user?.email}</p>
                  </div>

                  <Link
                    to="/"
                    onClick={() => setIsUserMenuOpen(false)}
                    className="flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-slate-700 hover:bg-emerald-50 hover:text-emerald-600 transition-colors"
                  >
                    <Store size={18} /> Return to Store
                  </Link>

                  <Link
                    to="/my-orders"
                    onClick={() => setIsUserMenuOpen(false)}
                    className="flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-slate-700 hover:bg-emerald-50 hover:text-emerald-600 transition-colors"
                  >
                    <Package size={18} /> Customer Orders
                  </Link>

                  <div className="border-t border-slate-100 my-1"></div>

                  <button
                    onClick={handleLogout}
                    className="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-rose-600 hover:bg-rose-50 transition-colors text-left"
                  >
                    <LogOut size={18} /> Sign Out
                  </button>
                </div>
              )}
            </div>

          </div>
        </header>

        {/* Dynamic Page Content */}
        <div className="p-4 sm:p-8 flex-1 max-w-7xl w-full mx-auto">
          <Outlet />
        </div>
      </main>
      
    </div>
  );
};

export default AdminLayout;