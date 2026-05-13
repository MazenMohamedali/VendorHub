// src/pages/AdminProducts.jsx
import { getImageUrl } from '../utils/imageUtils';
import React, { useState, useEffect } from 'react';
import { Check, X, Search, Loader2 } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const AdminProducts = () => {
  const [products, setProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  const fetchAdminProducts = async () => {
    try {
      setIsLoading(true);
      const response = await axiosInstance.get('/Product/admin/all');
      console.log("🔍 Products from API:", response.data.data); // Debug: check actual data
      setProducts(response.data.data || []);
    } catch (error) {
      console.error("Error fetching admin products:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchAdminProducts();
  }, []);

  // Helper to get status display and badge color
  const getStatusInfo = (status) => {
    const normalizedStatus = status?.toString().toUpperCase();
    switch (normalizedStatus) {
      case 'PENDING':
        return { text: 'قيد المراجعة', color: 'bg-amber-100 text-amber-600' };
      case 'REVIEWED':
        return { text: 'مقبول', color: 'bg-emerald-100 text-emerald-600' };
      case 'REJECTED':
        return { text: 'مرفوض', color: 'bg-red-100 text-red-600' };
      default:
        // If status is unknown, treat as pending (for safety)
        return { text: status || 'قيد المراجعة', color: 'bg-amber-100 text-amber-600' };
    }
  };

  // Helper to determine if product is pending (show approve/reject buttons)
  const isPending = (status) => {
    const normalizedStatus = status?.toString().toUpperCase();
    return normalizedStatus !== 'REVIEWED' && normalizedStatus !== 'REJECTED';
  };

  const handleApprove = async (id) => {
    try {
      await axiosInstance.patch(`/Product/${id}/approve`);
      alert("تمت الموافقة على المنتج وسيظهر للعملاء.");
      fetchAdminProducts(); // refresh
    } catch (error) {
      console.error("Approve error:", error.response?.data);
      alert(error.response?.data?.message || "حدث خطأ أثناء الموافقة.");
    }
  };

  const handleReject = async (id) => {
    if (window.confirm('هل أنت متأكد من رفض هذا المنتج؟')) {
      try {
        await axiosInstance.patch(`/Product/${id}/reject`);
        alert("تم رفض المنتج.");
        fetchAdminProducts();
      } catch (error) {
        console.error("Reject error:", error.response?.data);
        alert(error.response?.data?.message || "حدث خطأ أثناء الرفض.");
      }
    }
  };

  return (
    <div className="animate-fade-in-down">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-2">مراجعة المنتجات المضافة</h1>
        <p className="text-gray-500 text-sm">راجع المنتجات التي أضافها البائعون وقم بقبولها لتظهر في المتجر.</p>
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-x-auto">
        {isLoading ? (
          <div className="flex justify-center p-10 text-dokany"><Loader2 className="animate-spin" size={32} /></div>
        ) : (
          <table className="w-full text-right whitespace-nowrap">
            <thead className="bg-gray-50 text-gray-600 font-bold border-b border-gray-100">
              <tr>
                <th className="p-4">المنتج</th>
                <th className="p-4">البائع (المتجر)</th>
                <th className="p-4">السعر</th>
                <th className="p-4 text-center">الحالة</th>
                <th className="p-4 text-center">الإجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {products.length > 0 ? products.map((product) => {
                const statusInfo = getStatusInfo(product.status);
                const pending = isPending(product.status);
                return (
                  <tr key={product.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="p-4">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-gray-50 border border-gray-100 rounded-lg flex items-center justify-center p-1 shrink-0">
                          <img 
                            src={getImageUrl(product.imgUrl, 'Products')} 
                            alt={product.name} 
                            className="max-h-full mix-blend-multiply object-contain"
                            onError={(e) => e.target.src = "https://placehold.co/100x100?text=No+Image"}
                          />
                        </div>
                        <p className="font-bold text-gray-800">{product.name}</p>
                      </div>
                    </td>
                    <td className="p-4 text-gray-600">{product.storeName || 'غير معروف'}</td>
                    <td className="p-4 font-bold text-gray-800">{product.price} ج.م</td>
                    <td className="p-4 text-center">
                      <span className={`px-3 py-1 rounded-full text-xs font-bold ${statusInfo.color}`}>
                        {statusInfo.text}
                      </span>
                    </td>
                    <td className="p-4">
                      <div className="flex items-center justify-center gap-2">
                        {pending && (
                          <>
                            <button 
                              onClick={() => handleApprove(product.id)} 
                              className="p-2 text-emerald-500 hover:bg-emerald-50 rounded-lg transition-colors" 
                              title="موافقة"
                            >
                              <Check size={18} />
                            </button>
                            <button 
                              onClick={() => handleReject(product.id)} 
                              className="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors" 
                              title="رفض"
                            >
                              <X size={18} />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              }) : (
                <tr><td colSpan="5" className="p-8 text-center text-gray-500">لا توجد منتجات للعرض</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default AdminProducts;