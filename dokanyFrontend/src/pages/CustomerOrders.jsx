// src/pages/CustomerOrders.jsx
import { getImageUrl } from '../utils/imageUtils';
import React, { useState, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { ShoppingBag, User, Package, MapPin, Phone, Calendar, Clock, ChevronDown, ChevronUp, Loader2 } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const CustomerOrders = () => {
  const user = useSelector((state) => state.auth.user);
  
  const [orders, setOrders] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [expandedOrder, setExpandedOrder] = useState(null);

  useEffect(() => {
    const fetchMyOrders = async () => {
      try {
        setIsLoading(true);
        // جلب طلبات العميل الحالي من الباك إند
        const response = await axiosInstance.get('/Order/my-orders');
        setOrders(response.data.data || []);
      } catch (error) {
        console.error("Error fetching customer orders:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchMyOrders();
  }, []);

  const toggleOrder = (orderId) => {
    setExpandedOrder(expandedOrder === orderId ? null : orderId);
  };

  // دالة لتحويل حالة الطلب لنص عربي ولون مناسب
  const getStatusDetails = (status) => {
    switch (status) {
      case 'Pending': return { text: 'قيد الانتظار', color: 'bg-amber-100 text-amber-600' };
      case 'Confirmed': return { text: 'تم التأكيد', color: 'bg-blue-100 text-blue-600' };
      case 'Processing': return { text: 'جاري التجهيز', color: 'bg-indigo-100 text-indigo-600' };
      case 'Shipped': return { text: 'تم الشحن', color: 'bg-purple-100 text-purple-600' };
      case 'Delivered': return { text: 'تم التوصيل', color: 'bg-emerald-100 text-dokany' };
      case 'Cancelled': return { text: 'ملغي', color: 'bg-red-100 text-red-600' };
      default: return { text: status || 'غير معروف', color: 'bg-gray-100 text-gray-600' };
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 animate-fade-in-down" dir="rtl">
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* النصف الأول: بيانات الملف الشخصي */}
        <div className="lg:col-span-1">
          <div className="bg-white rounded-3xl shadow-sm border border-gray-100 p-8 sticky top-28">
            <div className="flex flex-col items-center text-center mb-8">
              <div className="w-24 h-24 bg-emerald-50 rounded-full flex items-center justify-center text-dokany mb-4 border-2 border-emerald-100">
                <User size={48} />
              </div>
              <h2 className="text-2xl font-black text-gray-800">{user?.firstName || 'عميل'} {user?.secondName || 'دكاني'}</h2>
              <p className="text-gray-500 text-sm">{user?.email || 'user@email.com'}</p>
            </div>

            <div className="space-y-6">
              <div className="flex items-center gap-4 text-gray-600 bg-gray-50 p-4 rounded-2xl">
                <Phone size={20} className="text-dokany" />
                <div>
                  <p className="text-xs text-gray-400">رقم الهاتف</p>
                  <p className="font-bold text-sm">{user?.phoneNumber || 'غير مسجل'}</p>
                </div>
              </div>
              <div className="flex items-center gap-4 text-gray-600 bg-gray-50 p-4 rounded-2xl">
                <MapPin size={20} className="text-dokany" />
                <div>
                  <p className="text-xs text-gray-400">العنوان الافتراضي</p>
                  <p className="font-bold text-sm">{user?.address || 'لم يتم تحديد عنوان'}</p>
                </div>
              </div>
              <div className="flex items-center gap-4 text-gray-600 bg-gray-50 p-4 rounded-2xl">
                <Calendar size={20} className="text-dokany" />
                <div>
                  <p className="text-xs text-gray-400">تاريخ الانضمام</p>
                  <p className="font-bold text-sm">مايو 2026</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* النصف الثاني: سجل الطلبات */}
        <div className="lg:col-span-2">
          <div className="flex items-center gap-3 mb-8">
            <ShoppingBag className="text-dokany" size={28} />
            <h1 className="text-2xl font-black text-gray-800">طلباتي السابقة</h1>
          </div>

          <div className="space-y-6">
            {isLoading ? (
              <div className="flex flex-col items-center justify-center py-20 text-dokany">
                <Loader2 className="animate-spin mb-4" size={40} />
                <p className="font-bold text-gray-600">جاري تحميل طلباتك...</p>
              </div>
            ) : orders.length > 0 ? (
              orders.map((order) => {
                const status = getStatusDetails(order.status);
                const isExpanded = expandedOrder === order.id;

                return (
                  <div key={order.id} className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden transition-all">
                    {/* رأس الطلب */}
                    <div 
                      onClick={() => toggleOrder(order.id)}
                      className="p-6 cursor-pointer hover:bg-gray-50/50 flex flex-wrap items-center justify-between gap-4"
                    >
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-gray-100 rounded-2xl flex items-center justify-center text-gray-400">
                          <Package size={24} />
                        </div>
                        <div>
                          <h3 className="font-black text-gray-800">طلب رقم #{order.id}</h3>
                          <div className="flex items-center gap-2 text-xs text-gray-400 mt-1">
                            <Clock size={14} />
                            <span>{new Date(order.createdAt).toLocaleDateString('ar-EG')}</span>
                          </div>
                        </div>
                      </div>
                      
                      <div className="flex items-center gap-6">
                        <div className="text-left md:text-right">
                          <p className="text-xs text-gray-400 mb-1">الإجمالي</p>
                          <p className="font-black text-dokany">{order.totalAmount || order.totalPrice} ج.م</p>
                        </div>
                        <span className={`px-4 py-1.5 rounded-full text-xs font-bold ${status.color}`}>
                          {status.text}
                        </span>
                        {isExpanded ? <ChevronUp size={20} className="text-gray-400" /> : <ChevronDown size={20} className="text-gray-400" />}
                      </div>
                    </div>

                    {/* تفاصيل الطلب */}
                    {isExpanded && (
                      <div className="p-6 bg-gray-50 border-t border-gray-100 animate-fade-in">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                          <div className="flex items-start gap-3">
                            <MapPin size={18} className="text-gray-400 mt-1" />
                            <div>
                              <p className="text-xs font-bold text-gray-400 mb-1">عنوان التوصيل</p>
                              <p className="text-sm text-gray-700">{order.deliveryAddress}</p>
                            </div>
                          </div>
                          <div className="flex items-start gap-3">
                            <Phone size={18} className="text-gray-400 mt-1" />
                            <div>
                              <p className="text-xs font-bold text-gray-400 mb-1">رقم التواصل</p>
                              <p className="text-sm text-gray-700">{order.phoneNumber}</p>
                            </div>
                          </div>
                        </div>

                        <div className="space-y-4">
                          <p className="text-sm font-bold text-gray-800 border-b border-gray-200 pb-2">المنتجات في هذا الطلب</p>
                          {order.items?.map((item, idx) => (
                            <div key={idx} className="flex items-center justify-between bg-white p-3 rounded-2xl border border-gray-100">
                              <div className="flex items-center gap-4">
                                <img src={getImageUrl(product.imgUrl, 'Products')} alt={product.name} />
                                <div>
                                  <p className="text-sm font-bold text-gray-800 line-clamp-1">{item.productName || item.name}</p>
                                  <p className="text-xs text-gray-500">الكمية: {item.quantity}</p>
                                </div>
                              </div>
                              <p className="font-bold text-dokany text-sm">{item.price} ج.م</p>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                );
              })
            ) : (
              <div className="bg-white rounded-3xl p-12 text-center border border-gray-100">
                <ShoppingBag size={48} className="mx-auto text-gray-300 mb-4" />
                <h3 className="text-xl font-bold text-gray-800 mb-2">لا توجد طلبات سابقة</h3>
                <p className="text-gray-500">لم تقم بإجراء أي عمليات شراء حتى الآن.</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default CustomerOrders;