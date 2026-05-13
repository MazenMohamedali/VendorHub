// src/layouts/AdminLayout.jsx
import React from 'react';
import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { Users, Package, LogOut, ShieldCheck, FolderTree } from 'lucide-react';
import { useDispatch, useSelector } from 'react-redux';
import { logout } from '../store/authSlice';

const AdminLayout = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  
  // قراءة بيانات الأدمن من Redux
  const user = useSelector((state) => state.auth.user);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  // عناصر القائمة الجانبية (تم إضافة إدارة الأقسام هنا)
  const menuItems = [
    { path: '/admin', icon: <Users size={20} />, label: 'إدارة البائعين' },
    { path: '/admin/products', icon: <Package size={20} />, label: 'مراجعة المنتجات' },
    { path: '/admin/categories', icon: <FolderTree size={20} />, label: 'إدارة الأقسام' }, // <-- العنصر الجديد
  ];

  return (
    <div className="min-h-screen bg-gray-50 flex" dir="rtl">
      
      {/* القائمة الجانبية (Sidebar) */}
      <aside className="w-64 bg-gray-900 text-white flex flex-col hidden md:flex fixed h-full right-0">
        <div className="p-6 flex items-center gap-3 border-b border-gray-800">
          <ShieldCheck className="text-emerald-500" size={32} />
          <span className="text-xl font-black tracking-tight text-white">إدارة النظام</span>
        </div>
        
        <nav className="flex-1 p-4 space-y-2 mt-4">
          {menuItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all ${
                  isActive 
                    ? 'bg-emerald-500/10 text-emerald-500 font-bold' 
                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                }`}
              >
                {item.icon}
                <span>{item.label}</span>
              </Link>
            );
          })}
        </nav>

        <div className="p-4 border-t border-gray-800">
          <button 
            onClick={handleLogout}
            className="flex items-center gap-3 w-full px-4 py-3 text-red-400 hover:bg-red-500/10 rounded-xl transition-colors"
          >
            <LogOut size={20} />
            <span className="font-bold">تسجيل الخروج</span>
          </button>
        </div>
      </aside>

      {/* المحتوى الرئيسي */}
      <main className="flex-1 md:mr-64 transition-all duration-300">
        
        {/* شريط علوي صغير (Header) */}
        <header className="bg-white shadow-sm h-20 flex items-center justify-between px-8 sticky top-0 z-10">
          <h2 className="text-xl font-bold text-gray-800">
            {menuItems.find(item => item.path === location.pathname)?.label || 'لوحة التحكم'}
          </h2>
          <div className="flex items-center gap-4">
            <span className="font-bold text-gray-700">
              مرحباً، {user?.firstName || user?.name || 'مدير النظام'} (Admin)
            </span>
            <div className="w-10 h-10 bg-gray-900 text-white rounded-full flex items-center justify-center font-bold">
              {user?.firstName?.charAt(0) || 'A'}
            </div>
          </div>
        </header>

        {/* مساحة عرض الصفحات (Outlet) */}
        <div className="p-8">
          <Outlet />
        </div>
      </main>
      
    </div>
  );
};

export default AdminLayout;