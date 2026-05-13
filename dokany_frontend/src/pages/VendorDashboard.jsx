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
        
        // 1. جلب الطلبات (بناءً على الـ JSON الخاص بك)
        const ordersRes = await axiosInstance.get('/Order/vendor-orders');
        // الوصول للمصفوفة: res.data (Axios) -> .data (الخاصة بالباك إند) -> .items (الخاصة بالـ Paging)
        const orders = ordersRes.data?.data?.items || [];

        // 2. جلب المنتجات
        const productsRes = await axiosInstance.get('/Product/my-products');
        const products = productsRes.data?.data?.items || (Array.isArray(productsRes.data?.data) ? productsRes.data.data : []);

        // 3. حساب الإحصائيات ديناميكياً
        // ملاحظة: totalPrice و orderId و orderDate هي المسميات في الـ API الخاص بك
        const pending = orders.filter(o => o.status === 'Pending').length;
        
        // حساب الإيرادات (للطلبات المكتملة فقط)
        const revenue = orders
          .filter(o => o.status === 'Delivered' || o.status === 'Shipped')
          .reduce((acc, curr) => acc + (curr.totalPrice || 0), 0);

        setStats({
          totalProducts: products.length,
          totalOrders: ordersRes.data?.data?.totalCount || orders.length,
          pendingOrders: pending,
          totalRevenue: revenue
        });

        // 4. أخذ أحدث 5 طلبات
        setRecentOrders(orders.slice(0, 5));

      } catch (error) {
        console.error("Error fetching dashboard data:", error);
      } finally {
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

  const statCards = [
    { title: 'إجمالي المنتجات', value: stats.totalProducts, icon: <Package size={24} />, color: 'bg-blue-50 text-blue-600' },
    { title: 'إجمالي الطلبات', value: stats.totalOrders, icon: <ShoppingBag size={24} />, color: 'bg-purple-50 text-purple-600' },
    { title: 'طلبات قيد الانتظار', value: stats.pendingOrders, icon: <Clock size={24} />, color: 'bg-amber-50 text-amber-600' },
    { title: 'إجمالي الإيرادات', value: `${stats.totalRevenue.toLocaleString()} ج.م`, icon: <DollarSign size={24} />, color: 'bg-emerald-50 text-emerald-600' },
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
                <tr key={order.orderId} className="hover:bg-gray-50/50 transition-colors">
                  <td className="p-4 font-bold text-gray-800">#{order.orderId}</td>
                  <td className="p-4 text-sm text-gray-500">
                    {order.orderDate ? new Date(order.orderDate).toLocaleDateString('ar-EG') : '---'}
                  </td>
                  <td className="p-4 font-bold text-dokany">{order.totalPrice} ج.م</td>
                  <td className="p-4">
                    <span className={`text-xs px-3 py-1 rounded-full font-bold ${
                      order.status === 'Pending' ? 'bg-amber-50 text-amber-600' : 'bg-gray-100 text-gray-700'
                    }`}>
                      {order.status === 'Pending' ? 'قيد الانتظار' : order.status}
                    </span>
                  </td>
                </tr>
              )) : (
                <tr>
                  <td colSpan="4" className="p-12 text-center text-gray-400">
                    لا توجد طلبات حالياً
                  </td>
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