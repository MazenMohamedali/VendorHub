// src/pages/VendorDashboard.jsx
import React, { useState, useEffect } from 'react';
import { Package, ShoppingBag, DollarSign, Clock, ArrowLeft, Loader2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import axiosInstance from '../api/axiosConfig';

const VendorDashboard = () => {
  const [stats, setStats] = useState({
    totalProducts: 0,
    totalOrders: 0,
    pendingOrders: 0,
    totalRevenue: 0
  });
  const [recentOrders, setRecentOrders] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setIsLoading(true);
        
        // 1. جلب المنتجات لحساب العدد الإجمالي
        const productsRes = await axiosInstance.get('/Product/my-products');
        // تأمين صارم للتأكد من أنها مصفوفة
        const products = Array.isArray(productsRes.data?.data) ? productsRes.data.data : [];

        // 2. جلب الطلبات لحساب الإحصائيات والإيرادات
        const ordersRes = await axiosInstance.get('/Order/vendor-orders');
        // تأمين صارم للتأكد من أنها مصفوفة لمنع خطأ filter is not a function
        const orders = Array.isArray(ordersRes.data?.data) ? ordersRes.data.data : [];

        // 3. حساب الإحصائيات ديناميكياً
        const pending = orders.filter(o => o.status === 'Pending').length;
        
        // حساب الإيرادات (للطلبات المكتملة أو المشحونة فقط)
        const revenue = orders
          .filter(o => o.status === 'Delivered' || o.status === 'Shipped')
          .reduce((acc, curr) => acc + (curr.totalAmount || 0), 0);

        setStats({
          totalProducts: products.length,
          totalOrders: orders.length,
          pendingOrders: pending,
          totalRevenue: revenue
        });

        // 4. أخذ أحدث 5 طلبات فقط للعرض السريع في الجدول
        setRecentOrders(orders.slice(0, 5));

      }catch (error) {
      console.error("Error fetching dashboard data:", error);
      if (error.response) {
        console.error("Response status:", error.response.status);
        console.error("Response data:", error.response.data);
      } 
      }finally {
        setIsLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-full min-h-[60vh] text-dokany">
        <Loader2 className="animate-spin" size={48} />
      </div>
    );
  }

  // مصفوفة كروت الإحصائيات لتسهيل العرض
  const statCards = [
    { title: 'إجمالي المنتجات', value: stats.totalProducts, icon: <Package size={24} />, color: 'bg-blue-50 text-blue-600' },
    { title: 'إجمالي الطلبات', value: stats.totalOrders, icon: <ShoppingBag size={24} />, color: 'bg-purple-50 text-purple-600' },
    { title: 'طلبات قيد الانتظار', value: stats.pendingOrders, icon: <Clock size={24} />, color: 'bg-amber-50 text-amber-600' },
    { title: 'إجمالي الإيرادات', value: `${stats.totalRevenue} ج.م`, icon: <DollarSign size={24} />, color: 'bg-emerald-50 text-emerald-600' },
  ];

  return (
    <div className="animate-fade-in-down" dir="rtl">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-2">نظرة عامة على متجرك</h1>
        <p className="text-gray-500 text-sm">تابع أداء مبيعاتك وإحصائيات منتجاتك في مكان واحد.</p>
      </div>

      {/* كروت الإحصائيات */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        {statCards.map((stat, index) => (
          <div key={index} className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm flex items-center gap-4 transition-transform hover:-translate-y-1">
            <div className={`w-14 h-14 rounded-xl flex items-center justify-center shrink-0 ${stat.color}`}>
              {stat.icon}
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">{stat.title}</p>
              <p className="text-2xl font-black text-gray-800">{stat.value}</p>
            </div>
          </div>
        ))}
      </div>

      {/* قسم أحدث الطلبات */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="p-6 border-b border-gray-100 flex justify-between items-center">
          <h2 className="text-lg font-bold text-gray-800">أحدث الطلبات</h2>
          <Link to="/vendor/orders" className="text-sm font-bold text-dokany hover:underline flex items-center gap-1">
            عرض الكل <ArrowLeft size={16} />
          </Link>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-right whitespace-nowrap">
            <thead className="bg-gray-50 text-gray-600 font-bold border-b border-gray-100">
              <tr>
                <th className="p-4">رقم الطلب</th>
                <th className="p-4">التاريخ</th>
                <th className="p-4">الإجمالي</th>
                <th className="p-4">الحالة</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {recentOrders.length > 0 ? recentOrders.map((order) => (
                <tr key={order.id} className="hover:bg-gray-50/50 transition-colors">
                  <td className="p-4 font-bold text-gray-800">#{order.id}</td>
                  <td className="p-4 text-sm text-gray-500">{new Date(order.createdAt).toLocaleDateString('ar-EG')}</td>
                  <td className="p-4 font-bold text-dokany">{order.totalAmount} ج.م</td>
                  <td className="p-4">
                    <span className="bg-gray-100 text-gray-700 text-xs px-3 py-1 rounded-full font-bold">
                      {order.status}
                    </span>
                  </td>
                </tr>
              )) : (
                <tr>
                  <td colSpan="4" className="p-8 text-center text-gray-500">لا توجد طلبات حديثة.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default VendorDashboard;