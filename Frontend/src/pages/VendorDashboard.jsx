import React, { useState, useEffect } from 'react';
import { Package, ShoppingBag, DollarSign, Clock, ArrowRight, Loader2 } from 'lucide-react';
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
        
        const ordersRes = await axiosInstance.get('/Order/vendor-orders');
        const orders = ordersRes.data?.data?.items || [];

        const productsRes = await axiosInstance.get('/Product/my-products');
        const products = productsRes.data?.data?.items || (Array.isArray(productsRes.data?.data) ? productsRes.data.data : []);

        const pending = orders.filter(o => o.status === 'Pending').length;
        
        const revenue = orders
          .filter(o => o.status === 'Delivered' || o.status === 'Shipped')
          .reduce((acc, curr) => acc + (curr.totalPrice || 0), 0);

        setStats({
          totalProducts: products.length,
          totalOrders: ordersRes.data?.data?.totalCount || orders.length,
          pendingOrders: pending,
          totalRevenue: revenue
        });

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
      <div className="flex justify-center items-center h-full min-h-[60vh] text-emerald-600">
        <Loader2 className="animate-spin" size={48} />
      </div>
    );
  }

  const statCards = [
    { title: 'Total Products', value: stats.totalProducts, icon: <Package size={24} />, color: 'bg-blue-50 text-blue-600' },
    { title: 'Total Orders', value: stats.totalOrders, icon: <ShoppingBag size={24} />, color: 'bg-purple-50 text-purple-600' },
    { title: 'Pending Orders', value: stats.pendingOrders, icon: <Clock size={24} />, color: 'bg-amber-50 text-amber-600' },
    { title: 'Total Revenue', value: `${stats.totalRevenue.toLocaleString()} EGP`, icon: <DollarSign size={24} />, color: 'bg-emerald-50 text-emerald-600' },
  ];

  return (
    <div className="animate-fade-in-down" dir="ltr">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-2">Store Overview</h1>
        <p className="text-gray-500 text-sm">Track your sales performance and product statistics in one place.</p>
      </div>

      {/* Stats Cards */}
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

      {/* Recent Orders Section */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="p-6 border-b border-gray-100 flex justify-between items-center">
          <h2 className="text-lg font-bold text-gray-800">Recent Orders</h2>
          <Link to="/vendor/orders" className="text-sm font-bold text-emerald-600 hover:underline flex items-center gap-1">
            View All <ArrowRight size={16} />
          </Link>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left whitespace-nowrap">
            <thead className="bg-gray-50 text-gray-600 font-bold border-b border-gray-100">
              <tr>
                <th className="p-4">Order ID</th>
                <th className="p-4">Date</th>
                <th className="p-4">Total</th>
                <th className="p-4">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {recentOrders.length > 0 ? recentOrders.map((order) => (
                <tr key={order.orderId} className="hover:bg-gray-50/50 transition-colors">
                  <td className="p-4 font-bold text-gray-800">#{order.orderId}</td>
                  <td className="p-4 text-sm text-gray-500">
                    {order.orderDate ? new Date(order.orderDate).toLocaleDateString('en-US') : '---'}
                  </td>
                  <td className="p-4 font-bold text-emerald-600">{order.totalPrice} EGP</td>
                  <td className="p-4">
                    <span className={`text-xs px-3 py-1 rounded-full font-bold ${
                      order.status === 'Pending' ? 'bg-amber-50 text-amber-600' : 'bg-gray-100 text-gray-700'
                    }`}>
                      {order.status}
                    </span>
                  </td>
                </tr>
              )) : (
                <tr>
                  <td colSpan="4" className="p-12 text-center text-gray-400">
                    No orders found
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