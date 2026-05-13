// src/pages/VendorOrders.jsx
import { getImageUrl } from '../utils/imageUtils';
import React, { useState, useEffect } from 'react';
import { Package, Clock, Truck, CheckCircle, XCircle, Eye, Loader2 } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const VendorOrders = () => {
  const [orders, setOrders] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState(null);

  // 1. جلب طلبات البائع
  const fetchVendorOrders = async () => {
    try {
      setIsLoading(true);
      // ملاحظة: تأكد من "مازن" من اسم المسار الخاص بطلبات البائع، افترضنا هنا /Order/vendor-orders
      const response = await axiosInstance.get('/Order/vendor-orders');
      setOrders(response.data.data || []);
    } catch (error) {
      console.error("Error fetching vendor orders:", error);
      // بيانات افتراضية مؤقتة للتجربة في حال المسار غير جاهز
      setOrders([]);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchVendorOrders();
  }, []);

  // 2. دالة تحديث حالة الطلب
  const handleStatusChange = async (orderId, newStatus) => {
    try {
      // إرسال الحالة الجديدة للباك إند
      await axiosInstance.patch(`/Order/${orderId}/status`, { status: newStatus });
      alert("تم تحديث حالة الطلب بنجاح!");
      fetchVendorOrders(); // تحديث الجدول
      if (selectedOrder && selectedOrder.id === orderId) {
        setSelectedOrder(false);
      }
    } catch (error) {
      console.error("Error updating order status:", error);
      alert("حدث خطأ أثناء تحديث الحالة.");
    }
  };

  // دالة مساعدة لتلوين وترجمة الحالة
  const getStatusBadge = (status) => {
    switch (status) {
      case 'Pending': return <span className="bg-amber-100 text-amber-600 px-3 py-1 rounded-full text-xs font-bold flex items-center gap-1 w-fit"><Clock size={14}/> قيد الانتظار</span>;
      case 'Processing': return <span className="bg-blue-100 text-blue-600 px-3 py-1 rounded-full text-xs font-bold flex items-center gap-1 w-fit"><Package size={14}/> جاري التجهيز</span>;
      case 'Shipped': return <span className="bg-purple-100 text-purple-600 px-3 py-1 rounded-full text-xs font-bold flex items-center gap-1 w-fit"><Truck size={14}/> تم الشحن</span>;
      case 'Delivered': return <span className="bg-emerald-100 text-emerald-600 px-3 py-1 rounded-full text-xs font-bold flex items-center gap-1 w-fit"><CheckCircle size={14}/> تم التوصيل</span>;
      case 'Cancelled': return <span className="bg-red-100 text-red-600 px-3 py-1 rounded-full text-xs font-bold flex items-center gap-1 w-fit"><XCircle size={14}/> ملغي</span>;
      default: return <span className="bg-gray-100 text-gray-600 px-3 py-1 rounded-full text-xs font-bold">{status}</span>;
    }
  };

  return (
    <div className="animate-fade-in-down relative">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-2">طلبات العملاء</h1>
        <p className="text-gray-500 text-sm">تابع طلبات منتجاتك وقم بتحديث حالتها (تجهيز، شحن، توصيل).</p>
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-x-auto">
        {isLoading ? (
          <div className="flex justify-center items-center p-12 text-dokany">
            <Loader2 className="animate-spin" size={40} />
          </div>
        ) : (
          <table className="w-full text-right whitespace-nowrap">
            <thead className="bg-gray-50 text-gray-600 font-bold border-b border-gray-100">
              <tr>
                <th className="p-4">رقم الطلب</th>
                <th className="p-4">تاريخ الطلب</th>
                <th className="p-4">العميل</th>
                <th className="p-4">الإجمالي (لمنتجاتك)</th>
                <th className="p-4">الحالة</th>
                <th className="p-4 text-center">التفاصيل</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {orders.length > 0 ? orders.map((order) => (
                <tr key={order.id} className="hover:bg-gray-50/50 transition-colors">
                  <td className="p-4 font-black text-gray-800">#{order.id}</td>
                  <td className="p-4 text-gray-500 text-sm">{new Date(order.createdAt).toLocaleDateString('ar-EG')}</td>
                  <td className="p-4 text-gray-800 font-medium">{order.customerName || 'عميل دكاني'}</td>
                  <td className="p-4 font-bold text-dokany">{order.totalAmount} ج.م</td>
                  <td className="p-4">{getStatusBadge(order.status)}</td>
                  <td className="p-4 text-center">
                    <button 
                      onClick={() => setSelectedOrder(order)}
                      className="bg-gray-50 hover:bg-dokany hover:text-white text-gray-600 p-2 rounded-lg transition-colors inline-flex items-center justify-center"
                    >
                      <Eye size={20} />
                    </button>
                  </td>
                </tr>
              )) : (
                <tr>
                  <td colSpan="6" className="p-8 text-center text-gray-500">لا توجد طلبات جديدة حتى الآن.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {/* نافذة تفاصيل الطلب (Modal) */}
      {selectedOrder && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-3xl w-full max-w-2xl overflow-hidden shadow-2xl animate-fade-in-up">
            <div className="bg-gray-50 border-b border-gray-100 p-6 flex justify-between items-center">
              <div>
                <h2 className="text-xl font-bold text-gray-800 mb-1">تفاصيل طلب #{selectedOrder.id}</h2>
                <p className="text-sm text-gray-500">{new Date(selectedOrder.createdAt).toLocaleString('ar-EG')}</p>
              </div>
              <button onClick={() => setSelectedOrder(null)} className="text-gray-400 hover:text-red-500 transition-colors">
                <XCircle size={28} />
              </button>
            </div>

            <div className="p-6">
              {/* تحديث حالة الطلب */}
              <div className="bg-emerald-50 border border-emerald-100 rounded-xl p-4 mb-6 flex items-center justify-between">
                <div>
                  <p className="text-sm font-bold text-emerald-800 mb-1">تحديث حالة الطلب</p>
                  <p className="text-xs text-emerald-600">الرجاء تحديث الحالة بناءً على مرحلة الشحن.</p>
                </div>
                <select 
                  value={selectedOrder.status}
                  onChange={(e) => handleStatusChange(selectedOrder.id, e.target.value)}
                  className="bg-white border border-emerald-200 text-emerald-800 font-bold rounded-lg px-4 py-2 outline-none focus:ring-2 focus:ring-emerald-500"
                >
                  <option value="Pending">قيد الانتظار</option>
                  <option value="Processing">جاري التجهيز</option>
                  <option value="Shipped">تم الشحن</option>
                  <option value="Delivered">تم التوصيل</option>
                  <option value="Cancelled">إلغاء الطلب</option>
                </select>
              </div>

              {/* المنتجات في الطلب */}
              <h3 className="font-bold text-gray-800 mb-4 border-b border-gray-100 pb-2">المنتجات المطلوبة</h3>
              <div className="space-y-4 max-h-60 overflow-y-auto pr-2">
                {selectedOrder.items?.map((item, index) => (
                  <div key={index} className="flex items-center justify-between bg-gray-50 p-3 rounded-xl border border-gray-100">
                    <div className="flex items-center gap-4">
                      <div className="w-12 h-12 bg-white rounded-lg p-1 border border-gray-100">
                        <img src={item.imageUrl || "https://placehold.co/100"} alt={item.productName} />
                      </div>
                      <div>
                        <p className="font-bold text-sm text-gray-800 line-clamp-1">{item.productName}</p>
                        <p className="text-xs text-gray-500">الكمية: {item.quantity}</p>
                      </div>
                    </div>
                    <p className="font-bold text-dokany text-sm">{item.price * item.quantity} ج.م</p>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}

    </div>
  );
};

export default VendorOrders;