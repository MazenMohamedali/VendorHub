// src/pages/AdminVendors.jsx
import React, { useState, useEffect } from 'react';
import { Check, X, Ban, Loader2, Users, ShieldCheck, ShieldAlert, Key, XCircle } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const AdminVendors = () => {
  const [vendors, setVendors] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  const [isPermissionModalOpen, setIsPermissionModalOpen] = useState(false);
  const [selectedVendor, setSelectedVendor] = useState(null);
  const [vendorPermissions, setVendorPermissions] = useState([]);
  const [isPermissionLoading, setIsPermissionLoading] = useState(false);

  // الأسماء الدقيقة للصلاحيات بناءً على توثيق الباك إند
  const availablePermissions = [
    { type: 'CanUploadProducts', label: 'إضافة منتجات', desc: 'يسمح للبائع برفع منتجات جديدة للمتجر.' },
    { type: 'CanEditProducts', label: 'تعديل المنتجات', desc: 'يسمح للبائع بتعديل بيانات وصور منتجاته.' },
    { type: 'CanDeleteProducts', label: 'حذف المنتجات', desc: 'يسمح للبائع بحذف منتجاته من المنصة.' },
    { type: 'CanViewProducts', label: 'عرض المنتجات', desc: 'يسمح للبائع برؤية قائمة منتجاته.' },
    { type: 'CanViewOrders', label: 'عرض الطلبات', desc: 'يسمح للبائع برؤية طلبات العملاء الخاصة به.' },
    { type: 'CanUpdateOrderStatus', label: 'تحديث حالة الطلب', desc: 'يسمح للبائع بتغيير حالة الطلب.' },
    { type: 'CanCancelOrders', label: 'إلغاء الطلبات', desc: 'يسمح للبائع بإلغاء طلبات العملاء.' },
    { type: 'CanViewAnalytics', label: 'عرض الإحصائيات', desc: 'يسمح للبائع برؤية تقارير مبيعاته.' },
    { type: 'CanManageInventory', label: 'إدارة المخزون', desc: 'يسمح للبائع بمتابعة وتحديث كميات المنتجات.' }
  ];

  const fetchVendors = async () => {
    try {
      setIsLoading(true);
      const response = await axiosInstance.get('/Vendor');
      setVendors(response.data.data || []);
    } catch (error) {
      console.error("Error fetching vendors:", error);
      if (error.response?.status === 403) {
        alert("غير مصرح لك بالوصول إلى بيانات البائعين. تأكد من أنك مسجل دخول كمدير.");
      } else {
        alert("حدث خطأ أثناء تحميل البائعين.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchVendors();
  }, []);

  const handleOpenPermissions = async (vendor) => {
    setSelectedVendor(vendor);
    setIsPermissionModalOpen(true);
    setIsPermissionLoading(true);
    try {
      // التأكد من صحة الرابط كما طلب الباك إند
      const res = await axiosInstance.get(`/Permission/vendor/${vendor.id}`);

      // قراءة الداتا بشكل صحيح بناءً على الـ JSON القادم
      const permissionsList = res.data?.data || [];
      setVendorPermissions(Array.isArray(permissionsList) ? permissionsList : []);
    } catch (error) {
      console.error("Error fetching permissions:", error);
      setVendorPermissions([]);
    } finally {
      setIsPermissionLoading(false);
    }
  };

  const togglePermission = async (permissionType, isCurrentlyEnabled) => {
    try {
      const endpoint = isCurrentlyEnabled ? 'disable' : 'enable';
      await axiosInstance.post(`/Permission/vendor/${selectedVendor.id}/${endpoint}/${permissionType}`, {});

      // تحديث حالة الزر محلياً بناءً على هيكل البيانات الجديد
      if (isCurrentlyEnabled) {
        setVendorPermissions(prev => prev.map(p =>
          p.permissionName === permissionType ? { ...p, isEnabled: false } : p
        ));
      } else {
        const exists = vendorPermissions.some(p => p.permissionName === permissionType);
        if (exists) {
          setVendorPermissions(prev => prev.map(p =>
            p.permissionName === permissionType ? { ...p, isEnabled: true } : p
          ));
        } else {
          setVendorPermissions(prev => [...prev, { permissionName: permissionType, isEnabled: true }]);
        }
      }
    } catch (error) {
      alert("حدث خطأ أثناء تحديث الصلاحية.");
    }
  };

  const handleApprove = async (id) => {
    try {
      await axiosInstance.patch(`/Account/approve-vendor/${id}`);
      alert("تم تفعيل حساب البائع. يمكنك الآن منحه الصلاحيات المطلوبة.");
      fetchVendors();
    } catch (error) { alert("حدث خطأ أثناء التفعيل."); }
  };

  const handleDeactivate = async (id) => {
    if (window.confirm("هل أنت متأكد من تعطيل هذا الحساب؟")) {
      try {
        await axiosInstance.delete(`/Account/deactivate/${id}`);
        fetchVendors();
      } catch (error) { alert("حدث خطأ أثناء التعطيل."); }
    }
  };

  return (
    <div className="animate-fade-in-down p-4" dir="rtl">
      <div className="mb-8">
        <h1 className="text-2xl font-black text-gray-800 mb-2 flex items-center gap-2">
          <ShieldCheck className="text-dokany" size={32} />
          إدارة البائعين والصلاحيات
        </h1>
        <p className="text-gray-500 text-sm">تحكم في قبول البائعين ومنحهم صلاحيات النشر والإدارة بشكل مخصص.</p>
      </div>

      <div className="bg-white rounded-3xl border border-gray-100 shadow-sm overflow-x-auto">
        {isLoading ? (
          <div className="flex justify-center p-12 text-dokany"><Loader2 className="animate-spin" size={40} /></div>
        ) : (
          <table className="w-full text-right whitespace-nowrap">
            <thead className="bg-gray-50 text-gray-600 font-bold border-b border-gray-100">
              <tr>
                <th className="p-5">البائع</th>
                <th className="p-5">المتجر</th>
                <th className="p-5 text-center">الحالة</th>
                <th className="p-5 text-center">الصلاحيات</th>
                <th className="p-5 text-center">الإجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {vendors.map((vendor) => {
                const status = vendor.accountStatus?.toUpperCase();
                return (
                  <tr key={vendor.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="p-5">
                      <div className="font-bold text-gray-800">{vendor.firstName} {vendor.secondName}</div>
                      <div className="text-xs text-gray-400">{vendor.email}</div>
                    </td>
                    <td className="p-5 text-gray-600 font-medium">{vendor.storeName || '---'}</td>
                    <td className="p-5 text-center">
                      <span className={`px-3 py-1.5 rounded-full text-xs font-bold ${status === 'ACTIVE' ? 'bg-emerald-100 text-emerald-600' : 'bg-amber-100 text-amber-600'}`}>
                        {status === 'ACTIVE' ? 'نشط' : 'قيد المراجعة'}
                      </span>
                    </td>
                    <td className="p-5 text-center">
                      <button
                        onClick={() => handleOpenPermissions(vendor)}
                        className="bg-gray-100 hover:bg-dokany hover:text-white text-gray-600 px-4 py-2 rounded-xl text-sm font-bold transition-all flex items-center gap-2 mx-auto"
                      >
                        <Key size={16} /> إدارة الصلاحيات
                      </button>
                    </td>
                    <td className="p-5">
                      <div className="flex items-center justify-center gap-2">
                        {status !== 'ACTIVE' ? (
                          <button onClick={() => handleApprove(vendor.id)} className="p-2 text-emerald-500 bg-emerald-50 rounded-lg hover:bg-emerald-500 hover:text-white transition-all"><Check size={20} /></button>
                        ) : (
                          <button onClick={() => handleDeactivate(vendor.id)} className="p-2 text-gray-400 bg-gray-50 rounded-lg hover:bg-red-500 hover:text-white transition-all"><Ban size={20} /></button>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {isPermissionModalOpen && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-white rounded-[2.5rem] w-full max-w-lg overflow-hidden shadow-2xl animate-fade-in-up">
            <div className="p-8 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
              <div>
                <h2 className="text-xl font-black text-gray-800">صلاحيات: {selectedVendor?.storeName || selectedVendor?.firstName}</h2>
                <p className="text-sm text-gray-500">حدد ما يمكن للبائع القيام به في المنصة.</p>
              </div>
              <button onClick={() => setIsPermissionModalOpen(false)} className="text-gray-400 hover:text-red-500 transition-colors"><XCircle size={32} /></button>
            </div>

            <div className="p-8 max-h-[60vh] overflow-y-auto custom-scrollbar">
              {isPermissionLoading ? (
                <div className="flex justify-center p-10"><Loader2 className="animate-spin text-dokany" size={32} /></div>
              ) : (
                <div className="space-y-4">
                  {availablePermissions.map((perm) => {
                    // الشرط الدقيق كما طلبه فريق الباك إند
                    const isEnabled = vendorPermissions.some(
                      p => p.permissionName === perm.type && p.isEnabled === true
                    );

                    return (
                      <div key={perm.type} className={`p-4 rounded-2xl border-2 transition-all flex items-center justify-between ${isEnabled ? 'border-emerald-100 bg-emerald-50/30' : 'border-gray-100 bg-white'}`}>
                        <div className="flex items-center gap-4">
                          <div className={`p-3 rounded-xl ${isEnabled ? 'bg-emerald-500 text-white' : 'bg-gray-100 text-gray-400'}`}>
                            {isEnabled ? <ShieldCheck size={20} /> : <ShieldAlert size={20} />}
                          </div>
                          <div>
                            <p className="font-bold text-gray-800 text-sm">{perm.label}</p>
                            <p className="text-xs text-gray-500">{perm.desc}</p>
                          </div>
                        </div>

                        <button
                          onClick={() => togglePermission(perm.type, isEnabled)}
                          className={`w-14 h-8 flex-shrink-0 rounded-full relative transition-colors p-1 ${isEnabled ? 'bg-emerald-500' : 'bg-gray-300'}`}
                        >
                          <div className={`w-6 h-6 bg-white rounded-full transition-all duration-300 shadow-sm ${isEnabled ? 'mr-6' : 'mr-0'}`}></div>
                        </button>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            <div className="p-8 bg-gray-50 border-t border-gray-100 flex justify-end">
              <button onClick={() => setIsPermissionModalOpen(false)} className="bg-gray-900 text-white px-8 py-3 rounded-xl font-bold hover:bg-black transition-all">إغلاق</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminVendors;