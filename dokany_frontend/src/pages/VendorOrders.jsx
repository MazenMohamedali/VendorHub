import React, { useState, useEffect } from 'react';
import { Package, Clock, Truck, CheckCircle, XCircle, Eye, Loader2, X } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const VendorOrders = () => {
  const [orders, setOrders] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [isUpdating, setIsUpdating] = useState(false);

  const fetchVendorOrders = async () => {
    try {
      setIsLoading(true);
      const response = await axiosInstance.get('/Order/vendor-orders');
      setOrders(response.data?.data?.items || []);
    } catch (error) {
      console.error("Error fetching orders:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchVendorOrders();
  }, []);

  const handleStatusChange = async (orderId, newStatus) => {
    try {
      setIsUpdating(true);
      // التعديل هنا: المسار الصحيح حسب كود الباك إند الخاص بك
      // الباك إند: [HttpPatch("{orderId}/status")] داخل OrderController
      await axiosInstance.patch(`/Order/${orderId}/status`, { 
        status: newStatus 
      });
      
      setOrders(prev => prev.map(order => 
        order.orderId === orderId ? { ...order, status: newStatus } : order
      ));
      
      if (selectedOrder) {
        setSelectedOrder(prev => ({ ...prev, status: newStatus }));
      }
      
      alert("تم تحديث الحالة بنجاح");
    } catch (error) {
      console.error("Error update status:", error);
      alert("فشل التحديث: تأكد أن الحالة (Status) المرسلة مقبولة في السيرفر");
    } finally {
      setIsUpdating(false);
    }
  };

  if (isLoading) return <div className="flex justify-center p-20 text-dokany"><Loader2 className="animate-spin" size={40} /></div>;

  return (
    <div className="p-4" dir="rtl">
      <h1 className="text-2xl font-bold mb-6">طلبات المتجر</h1>
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <table className="w-full text-right">
          <thead className="bg-gray-50 border-b border-gray-100">
            <tr>
              <th className="p-4">رقم الطلب</th>
              <th className="p-4">العميل</th>
              <th className="p-4">الإجمالي</th>
              <th className="p-4">الحالة</th>
              <th className="p-4">الإجراء</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {orders.map((order) => (
              <tr key={order.orderId} className="hover:bg-gray-50/50">
                <td className="p-4 font-bold">#{order.orderId}</td>
                <td className="p-4 text-sm">{order.customerName}</td>
                <td className="p-4 font-bold text-dokany">{order.totalPrice} ج.م</td>
                <td className="p-4">
                  <span className={`px-3 py-1 rounded-full text-xs font-bold ${
                    order.status === 'Pending' ? 'bg-amber-100 text-amber-600' : 'bg-emerald-100 text-emerald-600'
                  }`}>
                    {order.status}
                  </span>
                </td>
                <td className="p-4">
                  <button onClick={() => setSelectedOrder(order)} className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg">
                    <Eye size={20} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {selectedOrder && (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex justify-center items-center p-4">
          <div className="bg-white rounded-3xl w-full max-w-lg shadow-2xl overflow-hidden">
            <div className="p-6 border-b flex justify-between items-center">
              <h2 className="text-xl font-bold">تحديث طلب #{selectedOrder.orderId}</h2>
              <button onClick={() => setSelectedOrder(null)}><X size={24} /></button>
            </div>
            <div className="p-6 space-y-4">
              <div className="bg-gray-50 p-4 rounded-xl">
                <p className="text-sm font-bold mb-3">اختر الحالة الجديدة:</p>
                <div className="flex flex-wrap gap-2">
                  {['Shipped', 'Delivered', 'Cancelled'].map((status) => (
                    <button
                      key={status}
                      disabled={isUpdating}
                      onClick={() => handleStatusChange(selectedOrder.orderId, status)}
                      className={`px-4 py-2 rounded-lg text-sm font-bold border transition-all ${
                        selectedOrder.status === status ? 'bg-dokany text-white' : 'bg-white hover:border-dokany'
                      }`}
                    >
                      {status}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default VendorOrders;