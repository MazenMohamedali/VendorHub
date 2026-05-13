// src/layouts/DashboardLayout.jsx
import React, { useState, useEffect } from 'react';
import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { LayoutDashboard, Package, ShoppingBag, Settings, LogOut, Bell, X, ShoppingCart } from 'lucide-react';

const DashboardLayout = () => {
  const location = useLocation();
  const navigate = useNavigate();

  // حالات الإشعارات (Sockets Simulation)
  const [notifications, setNotifications] = useState([]); // قائمة الإشعارات في الجرس
  const [toasts, setToasts] = useState([]); // الإشعارات المنبثقة (Toasts)
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  // الروابط الخاصة بالبائع
  const menuItems = [
    { title: 'نظرة عامة', icon: <LayoutDashboard size={20} />, path: '/vendor' },
    { title: 'إدارة المنتجات', icon: <Package size={20} />, path: '/vendor/products' },
    { title: 'الطلبات والمبيعات', icon: <ShoppingBag size={20} />, path: '/vendor/orders' },
    { title: 'الإعدادات', icon: <Settings size={20} />, path: '/vendor/settings' },
  ];

  // 🔴 دالة محاكاة وصول طلب جديد عبر الـ Sockets 🔴
  // (سيقوم مازن باستبدال هذا الزر بـ socket.on('newOrder', ...))
  const simulateNewSocketOrder = () => {
    const newOrder = {
      id: Date.now(),
      orderNumber: `ORD-${Math.floor(1000 + Math.random() * 9000)}`,
      amount: Math.floor(500 + Math.random() * 4500),
      time: new Date().toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })
    };

    // 1. إضافة الإشعار في القائمة
    setNotifications(prev => [newOrder, ...prev]);

    // 2. إظهار الإشعار المنبثق (Toast)
    setToasts(prev => [...prev, newOrder]);

    // إخفاء الـ Toast بعد 5 ثوانٍ تلقائياً
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== newOrder.id));
    }, 5000);
  };

  return (
    <div className="min-h-screen bg-gray-50 flex font-sans" dir="rtl">
      
      {/* الشريط الجانبي (Sidebar) */}
      <aside className="w-64 bg-white border-l border-gray-100 hidden md:flex flex-col shadow-sm">
        <Link to="/" className="p-6 border-b border-gray-100 flex items-center gap-2 cursor-pointer hover:opacity-80 transition-opacity">
          <div className="bg-dokany w-8 h-8 rounded-lg flex items-center justify-center text-white font-bold text-xl">د</div>
          <span className="text-xl font-black text-dokany tracking-tight">لوحة البائع</span>
        </Link>

        <nav className="flex-1 p-4 space-y-2">
          {menuItems.map((item, index) => {
            const isActive = location.pathname === item.path;
            return (
              <Link 
                key={index} 
                to={item.path}
                className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-colors font-medium ${
                  isActive ? 'bg-emerald-50 text-dokany' : 'text-gray-500 hover:bg-gray-50 hover:text-dokany'
                }`}
              >
                {item.icon}
                {item.title}
              </Link>
            );
          })}
        </nav>

        <div className="p-4 border-t border-gray-100">
          <button onClick={() => navigate('/')} className="flex items-center gap-3 px-4 py-3 w-full text-red-500 hover:bg-red-50 rounded-xl transition-colors font-medium">
            <LogOut size={20} />
            تسجيل الخروج
          </button>
        </div>
      </aside>

      {/* منطقة المحتوى الرئيسية */}
      <main className="flex-1 flex flex-col h-screen overflow-hidden relative">
        
        {/* الشريط العلوي (Topbar) */}
        <header className="bg-white border-b border-gray-100 h-16 flex items-center justify-between px-6 shrink-0 shadow-sm z-20">
          
          <div className="flex items-center gap-4">
            <h2 className="text-xl font-bold text-gray-800">مرحباً، تكنو ستور 👋</h2>
            {/* زر محاكاة الـ Socket */}
            <button 
              onClick={simulateNewSocketOrder}
              className="hidden md:flex text-xs bg-dokany text-white px-3 py-1.5 rounded-lg hover:bg-dokany-dark transition-colors font-bold shadow-sm"
            >
              🚀 محاكاة طلب جديد (Socket)
            </button>
          </div>
          
          <div className="flex items-center gap-4 relative">
            
            {/* أيقونة الجرس */}
            <button 
              onClick={() => setIsDropdownOpen(!isDropdownOpen)}
              className="relative text-gray-500 hover:text-dokany transition-colors p-2"
            >
              <Bell size={22} />
              {notifications.length > 0 && (
                <span className="absolute top-1 right-1 w-4 h-4 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center border-2 border-white animate-pulse">
                  {notifications.length}
                </span>
              )}
            </button>

            {/* قائمة الإشعارات المنسدلة */}
            {isDropdownOpen && (
              <div className="absolute top-12 left-10 w-80 bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden z-50 animate-fade-in-down">
                <div className="p-4 border-b border-gray-100 flex justify-between items-center bg-gray-50">
                  <span className="font-bold text-gray-800">الإشعارات</span>
                  {notifications.length > 0 && (
                    <button onClick={() => setNotifications([])} className="text-xs text-dokany hover:underline font-medium">تحديد كـ مقروء</button>
                  )}
                </div>
                <div className="max-h-80 overflow-y-auto">
                  {notifications.length > 0 ? (
                    notifications.map((notif) => (
                      <div key={notif.id} className="p-4 border-b border-gray-50 hover:bg-emerald-50/50 transition-colors cursor-pointer">
                        <div className="flex justify-between items-start mb-1">
                          <span className="font-bold text-gray-800 text-sm">طلب جديد تم استلامه! 🎉</span>
                          <span className="text-[10px] text-gray-400">{notif.time}</span>
                        </div>
                        <p className="text-xs text-gray-500">
                          تم إضافة طلب برقم <span className="font-bold text-dokany">{notif.orderNumber}</span> بقيمة {notif.amount} ج.م.
                        </p>
                      </div>
                    ))
                  ) : (
                    <div className="p-8 text-center text-gray-400 text-sm">
                      <Bell size={24} className="mx-auto mb-2 text-gray-300" />
                      لا توجد إشعارات جديدة.
                    </div>
                  )}
                </div>
              </div>
            )}

            <div className="w-9 h-9 bg-dokany-light rounded-full flex items-center justify-center text-dokany font-bold border border-emerald-100 cursor-pointer">
              ت
            </div>
          </div>
        </header>

        {/* محتوى الصفحات المتغير */}
        <div className="flex-1 overflow-y-auto p-6 lg:p-8">
          <Outlet />
        </div>

        {/* منطقة التنبيهات المنبثقة (Toasts Container) */}
        <div className="fixed bottom-6 left-6 z-50 flex flex-col gap-3">
          {toasts.map((toast) => (
            <div key={toast.id} className="bg-white border-r-4 border-dokany rounded-xl shadow-2xl p-4 w-80 flex items-start gap-4 animate-fade-in-up">
              <div className="w-10 h-10 rounded-full bg-emerald-100 text-dokany flex items-center justify-center shrink-0">
                <ShoppingCart size={20} />
              </div>
              <div className="flex-1">
                <h4 className="font-bold text-gray-800 text-sm mb-1">طلب شراء جديد!</h4>
                <p className="text-xs text-gray-500 leading-relaxed">
                  عميل للتو قام بشراء منتجات بقيمة <span className="font-bold text-dokany">{toast.amount} ج.م</span>. (رقم الطلب: {toast.orderNumber})
                </p>
              </div>
              <button onClick={() => setToasts(prev => prev.filter(t => t.id !== toast.id))} className="text-gray-400 hover:text-red-500 transition-colors">
                <X size={16} />
              </button>
            </div>
          ))}
        </div>
        
      </main>
    </div>
  );
};

export default DashboardLayout;