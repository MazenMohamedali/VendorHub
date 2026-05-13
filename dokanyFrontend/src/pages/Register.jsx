// src/pages/Register.jsx
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Mail, Lock, User, Phone, MapPin, Store, ArrowRight, ShieldCheck, Loader2 } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const Register = () => {
  const navigate = useNavigate();
  
  const [role, setRole] = useState('customer'); // 'customer' or 'vendor'
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  const [formData, setFormData] = useState({
    firstName: '',
    secondName: '',
    email: '',
    password: '',
    confirmPassword: '',
    phoneNumber: '',
    address: '', // للعميل فقط
    storeName: '', // للبائع فقط
  });

  const validatePhone = (phone) => {
    if (!phone) return true; 
    const regex = /^01[0125][0-9]{8}$/;
    return regex.test(phone);
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');

    // 1. التحقق من الباسورد
    if (formData.password !== formData.confirmPassword) {
      setError('كلمتا المرور غير متطابقتين!');
      return;
    }

    // 2. التحقق من رقم الهاتف
    if (formData.phoneNumber && !validatePhone(formData.phoneNumber)) {
      setError('رقم الهاتف غير صحيح. يجب أن يبدأ بـ 01 ويتبعه 8 أرقام.');
      return;
    }

    setIsSubmitting(true);

    try {
      // 3. تحديد المسار وتجهيز البيانات
      const endpoint = role === 'customer' ? '/Account/register/customer' : '/Account/register/vendor';
      
      const payload = {
        firstName: formData.firstName,
        secondName: formData.secondName,
        email: formData.email,
        password: formData.password,
        confirmPassword: formData.confirmPassword,
        phoneNumber: formData.phoneNumber,
        ...(role === 'customer' ? { address: formData.address } : { storeName: formData.storeName })
      };

      // 4. إرسال الطلب
      await axiosInstance.post(endpoint, payload);

      // 5. التوجيه بعد النجاح
      if (role === 'vendor') {
        alert("تم تسجيل حساب البائع بنجاح! حسابك الآن بانتظار موافقة الإدارة.");
      } else {
        alert("تم إنشاء حسابك بنجاح! يمكنك الآن تسجيل الدخول.");
      }
      navigate('/login');

    } catch (err) {
      console.error("Registration error:", err);
      // عرض رسالة الخطأ القادمة من الباك إند إن وجدت
      setError(err.response?.data?.message || err.response?.data?.errors?.[0] || 'حدث خطأ أثناء التسجيل. تأكد من البيانات.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4 font-sans py-10" dir="rtl">
      <div className="max-w-6xl w-full bg-white rounded-3xl shadow-xl overflow-hidden flex flex-col md:flex-row">
        
        {/* النصف الأول: الهوية البصرية */}
        <div className="hidden md:flex md:w-5/12 bg-dokany p-12 flex-col justify-between relative overflow-hidden">
          <div className="relative z-10">
            <Link to="/" className="flex items-center gap-2 mb-12 w-fit hover:opacity-80 transition-opacity">
              <div className="bg-white w-10 h-10 rounded-xl flex items-center justify-center text-dokany font-bold text-2xl">د</div>
              <span className="text-2xl font-black text-white tracking-tight">دكاني</span>
            </Link>
            <h1 className="text-4xl font-black text-white mb-6 leading-tight">
              {role === 'customer' ? 'ابدأ رحلة تسوق\nممتعة وآمنة' : 'انضم إلينا كبائع\nوضاعف أرباحك'}
            </h1>
            <p className="text-emerald-100 text-lg leading-relaxed">
              {role === 'customer' 
                ? 'أنشئ حسابك الآن لتتمكن من الشراء، متابعة طلباتك، وحفظ منتجاتك المفضلة.' 
                : 'منصة دكاني توفر لك الأدوات اللازمة لإدارة منتجاتك ومبيعاتك بكل احترافية.'}
            </p>
          </div>
          <div className="absolute -bottom-24 -right-24 w-96 h-96 bg-emerald-500 rounded-full mix-blend-multiply filter blur-3xl opacity-50 animate-blob"></div>
        </div>

        {/* النصف الثاني: نموذج التسجيل */}
        <div className="w-full md:w-7/12 p-8 sm:p-12">
          <Link to="/" className="text-gray-400 hover:text-dokany flex items-center gap-2 w-fit mb-6 transition-colors">
            <ArrowRight size={20} /> العودة للرئيسية
          </Link>

          <h2 className="text-3xl font-black text-gray-800 mb-6">إنشاء حساب جديد</h2>
          
          <div className="flex bg-gray-100 p-1 rounded-xl mb-8">
            <button 
              type="button"
              onClick={() => { setRole('customer'); setError(''); }}
              className={`flex-1 py-3 text-sm font-bold rounded-lg transition-all ${role === 'customer' ? 'bg-white text-dokany shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
            >
              حساب مشتري
            </button>
            <button 
              type="button"
              onClick={() => { setRole('vendor'); setError(''); }}
              className={`flex-1 py-3 text-sm font-bold rounded-lg transition-all ${role === 'vendor' ? 'bg-white text-dokany shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
            >
              حساب بائع
            </button>
          </div>

          {error && (
            <div className="bg-red-50 text-red-600 p-4 rounded-xl mb-6 font-medium text-sm flex items-center gap-2">
              <ShieldCheck size={18} /> {error}
            </div>
          )}

          <form onSubmit={handleRegister} className="space-y-5">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">الاسم الأول</label>
                <div className="relative">
                  <User size={20} className="absolute right-4 top-3.5 text-gray-400" />
                  <input type="text" required minLength="2" maxLength="100" value={formData.firstName} onChange={(e) => setFormData({...formData, firstName: e.target.value})} className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none" placeholder="الاسم الأول" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">الاسم الأخير</label>
                <div className="relative">
                  <User size={20} className="absolute right-4 top-3.5 text-gray-400" />
                  <input type="text" required minLength="2" maxLength="100" value={formData.secondName} onChange={(e) => setFormData({...formData, secondName: e.target.value})} className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none" placeholder="الاسم الأخير" />
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">البريد الإلكتروني</label>
                <div className="relative">
                  <Mail size={20} className="absolute right-4 top-3.5 text-gray-400" />
                  <input type="email" required value={formData.email} onChange={(e) => setFormData({...formData, email: e.target.value})} className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none text-right" placeholder="example@email.com" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">رقم الهاتف (اختياري)</label>
                <div className="relative">
                  <Phone size={20} className="absolute right-4 top-3.5 text-gray-400" />
                  <input type="tel" value={formData.phoneNumber} onChange={(e) => setFormData({...formData, phoneNumber: e.target.value})} className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none text-right" placeholder="01XXXXXXXXX" />
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">كلمة المرور</label>
                <div className="relative">
                  <Lock size={20} className="absolute right-4 top-3.5 text-gray-400" />
                  <input type="password" required minLength="6" value={formData.password} onChange={(e) => setFormData({...formData, password: e.target.value})} className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none text-right" placeholder="••••••••" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">تأكيد كلمة المرور</label>
                <div className="relative">
                  <Lock size={20} className="absolute right-4 top-3.5 text-gray-400" />
                  <input type="password" required minLength="6" value={formData.confirmPassword} onChange={(e) => setFormData({...formData, confirmPassword: e.target.value})} className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none text-right" placeholder="••••••••" />
                </div>
              </div>
            </div>

            {role === 'customer' ? (
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">عنوان التوصيل (اختياري)</label>
                <div className="relative">
                  <MapPin size={20} className="absolute right-4 top-3.5 text-gray-400" />
                  <input type="text" value={formData.address} onChange={(e) => setFormData({...formData, address: e.target.value})} className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none" placeholder="المدينة، الحي، الشارع..." />
                </div>
              </div>
            ) : (
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">اسم المتجر (مطلوب)</label>
                <div className="relative">
                  <Store size={20} className="absolute right-4 top-3.5 text-gray-400" />
                  <input type="text" required minLength="2" maxLength="200" value={formData.storeName} onChange={(e) => setFormData({...formData, storeName: e.target.value})} className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none" placeholder="اسم متجرك التجاري" />
                </div>
              </div>
            )}

            <button type="submit" disabled={isSubmitting} className="w-full bg-dokany text-white py-4 rounded-xl font-bold text-lg hover:bg-dokany-dark transition-colors shadow-lg shadow-emerald-500/30 mt-4 flex justify-center items-center gap-2 disabled:opacity-70">
              {isSubmitting ? <Loader2 className="animate-spin" size={24} /> : 'إنشاء الحساب'}
            </button>
          </form>

          <div className="mt-8 text-center text-gray-600">
            لديك حساب بالفعل؟ <Link to="/login" className="font-bold text-dokany hover:underline">سجل دخولك هنا</Link>
          </div>
        </div>

      </div>
    </div>
  );
};

export default Register;