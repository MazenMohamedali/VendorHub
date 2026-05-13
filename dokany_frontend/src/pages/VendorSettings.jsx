// src/pages/VendorSettings.jsx
import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Store, Phone, Mail, User, ShieldCheck, Loader2 } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';
// إذا كان لديك أكشن لتحديث بيانات المستخدم في Redux، استدعه هنا.
// import { updateUser } from '../store/authSlice'; 

const VendorSettings = () => {
  const user = useSelector((state) => state.auth.user);
  const dispatch = useDispatch();

  const [isLoading, setIsLoading] = useState(false);
  const [successMsg, setSuccessMsg] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  // تهيئة الفورم
  const [formData, setFormData] = useState({
    firstName: '',
    secondName: '',
    phoneNumber: '',
    storeName: '',
    email: '' 
  });

  // 1. جلب بيانات البروفايل الحقيقية من الباك إند عند فتح الصفحة
  const fetchProfileData = async () => {
    try {
      // المسار الجديد من الـ Swagger: /Account/profile
      const response = await axiosInstance.get('/Account/profile');
      const data = response.data.data;
      setFormData({
        firstName: data.firstName || '',
        secondName: data.secondName || '',
        phoneNumber: data.phoneNumber || '',
        storeName: data.storeName || '',
        email: data.email || ''
      });
    } catch (error) {
      console.error("Error fetching profile:", error);
      // fallback لبيانات Redux إذا فشل الطلب
      if (user) {
        setFormData({
          firstName: user.firstName || '',
          secondName: user.secondName || '',
          phoneNumber: user.phoneNumber || '',
          storeName: user.storeName || '',
          email: user.email || ''
        });
      }
    }
  };

  useEffect(() => {
    fetchProfileData();
  }, [user]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setSuccessMsg('');
    setErrorMsg('');

    try {
      // بناءً على التحديثات الجديدة، نحتاج لإرسال طلبين منفصلين إذا تغيرت البيانات
      
      // أ- تحديث بيانات الملف الشخصي (الاسم والهاتف)
      await axiosInstance.put('/Account/update-profile', {
        firstName: formData.firstName,
        secondName: formData.secondName,
        phoneNumber: formData.phoneNumber
      });

      // ب- تحديث اسم المتجر (مسار منفصل حسب Swagger)
      await axiosInstance.put('/Account/update-store-name', {
        storeName: formData.storeName
      });
      
      setSuccessMsg('تم تحديث كافة بيانات المتجر بنجاح!');
      
      // اختياري: تحديث Redux لكي تظهر البيانات الجديدة في القائمة الجانبية فوراً
      // dispatch(updateUser(formData)); 

    } catch (error) {
      console.error("Error updating profile:", error);
      setErrorMsg(error.response?.data?.message || 'حدث خطأ أثناء حفظ التعديلات، تأكد من صحة البيانات.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="animate-fade-in-down" dir="rtl">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-2">إعدادات المتجر</h1>
        <p className="text-gray-500 text-sm">قم بإدارة بياناتك الشخصية وتفاصيل متجرك التجاري.</p>
      </div>

      <div className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden max-w-4xl">
        <div className="p-8 md:p-12">
          
          {successMsg && (
            <div className="bg-emerald-50 text-emerald-600 p-4 rounded-xl mb-8 font-bold flex items-center gap-2 border border-emerald-100">
              <ShieldCheck size={20} />
              {successMsg}
            </div>
          )}

          {errorMsg && (
            <div className="bg-red-50 text-red-600 p-4 rounded-xl mb-8 font-bold flex items-center gap-2 border border-red-100">
              <ShieldCheck size={20} />
              {errorMsg}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-8">
            
            {/* قسم بيانات المتجر */}
            <div>
              <h2 className="text-lg font-black text-gray-800 mb-4 flex items-center gap-2 border-b border-gray-100 pb-2">
                <Store className="text-dokany" size={20} />
                بيانات المتجر
              </h2>
              <div className="grid grid-cols-1 gap-6">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">اسم المتجر (التجاري)</label>
                  <input 
                    type="text" 
                    required 
                    value={formData.storeName} 
                    onChange={(e) => setFormData({...formData, storeName: e.target.value})} 
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 px-4 focus:ring-2 focus:ring-dokany outline-none" 
                  />
                  <p className="text-xs text-gray-400 mt-2">هذا هو الاسم الذي سيظهر للعملاء في صفحة تفاصيل المنتج.</p>
                </div>
              </div>
            </div>

            {/* قسم البيانات الشخصية */}
            <div>
              <h2 className="text-lg font-black text-gray-800 mb-4 flex items-center gap-2 border-b border-gray-100 pb-2">
                <User className="text-dokany" size={20} />
                البيانات الشخصية
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">الاسم الأول</label>
                  <input 
                    type="text" 
                    required 
                    value={formData.firstName} 
                    onChange={(e) => setFormData({...formData, firstName: e.target.value})} 
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 px-4 focus:ring-2 focus:ring-dokany outline-none" 
                  />
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">الاسم الأخير</label>
                  <input 
                    type="text" 
                    required 
                    value={formData.secondName} 
                    onChange={(e) => setFormData({...formData, secondName: e.target.value})} 
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 px-4 focus:ring-2 focus:ring-dokany outline-none" 
                  />
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">البريد الإلكتروني</label>
                  <div className="relative">
                    <Mail size={20} className="absolute right-4 top-3.5 text-gray-400" />
                    <input 
                      type="email" 
                      disabled
                      value={formData.email} 
                      className="w-full bg-gray-100 border border-gray-200 rounded-xl py-3 pr-12 pl-4 text-gray-500 cursor-not-allowed text-right" 
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">رقم الهاتف</label>
                  <div className="relative">
                    <Phone size={20} className="absolute right-4 top-3.5 text-gray-400" />
                    <input 
                      type="tel" 
                      value={formData.phoneNumber} 
                      onChange={(e) => setFormData({...formData, phoneNumber: e.target.value})} 
                      className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none text-right" 
                      placeholder="01XXXXXXXXX"
                    />
                  </div>
                </div>
              </div>
            </div>

            <div className="pt-4 border-t border-gray-100">
              <button 
                type="submit" 
                disabled={isLoading}
                className="bg-dokany text-white px-8 py-4 rounded-xl font-bold text-lg hover:bg-dokany-dark transition-colors shadow-lg shadow-emerald-500/30 flex items-center gap-2 disabled:opacity-70"
              >
                {isLoading ? <Loader2 size={24} className="animate-spin" /> : 'حفظ التعديلات'}
              </button>
            </div>

          </form>
        </div>
      </div>
    </div>
  );
};

export default VendorSettings;