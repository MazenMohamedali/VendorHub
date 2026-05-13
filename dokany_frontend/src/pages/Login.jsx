// src/pages/Login.jsx
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Mail, Lock, ArrowRight, Loader2 } from 'lucide-react';
import { useDispatch } from 'react-redux';
import { login } from '../store/authSlice';
import axiosInstance from '../api/axiosConfig'; // <-- استيراد محطة الاتصال

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false); // حالة التحميل
  const [errorMsg, setErrorMsg] = useState(''); // حالة الخطأ

  const navigate = useNavigate();
  const dispatch = useDispatch();

  const handleLogin = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setErrorMsg('');

    try {
      // 1. إرسال طلب تسجيل الدخول للباك إند
      const loginResponse = await axiosInstance.post('/Account/login', {
        email: email,
        password: password
      });

      // 2. استخراج التوكن وحفظه في المتصفح
      const token = loginResponse.data.data;
      localStorage.setItem('token', token);

      // 3. جلب بيانات المستخدم (اسمه وصلاحياته) باستخدام مسار /me
      const userResponse = await axiosInstance.get('/Account/me');
      const userData = userResponse.data.data;
      console.log("User roles:", userData.roles);
      // 4. إرسال البيانات للـ Redux (بدلاً من البيانات الوهمية القديمة)
      console.log("Dispatched user:", userData);
      dispatch(login(userData));

      // 5. توجيه المستخدم حسب دوره (Role)
      if (userData.roles && userData.roles.includes('Admin')) {
        navigate('/admin');
      } else if (userData.roles && userData.roles.includes('Vendor')) {
        navigate('/vendor');
      } else {
        navigate('/'); // المشتري العادي يذهب للرئيسية
      }

    } catch (error) {
      // قراءة رسالة الخطأ القادمة من الباك إند (مثلاً: الباسوورد غلط)
      if (error.response && error.response.data) {
        setErrorMsg(error.response.data.message || 'بيانات الدخول غير صحيحة.');
      } else {
        setErrorMsg('حدث خطأ في الاتصال بالسيرفر. تأكد من تشغيل الباك إند.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4 font-sans" dir="rtl">
      <div className="max-w-5xl w-full bg-white rounded-3xl shadow-xl overflow-hidden flex flex-col md:flex-row">
        
        {/* النصف الأول: الهوية البصرية */}
        <div className="hidden md:flex md:w-1/2 bg-dokany p-12 flex-col justify-between relative overflow-hidden">
          <div className="relative z-10">
            <Link to="/" className="flex items-center gap-2 mb-12 w-fit hover:opacity-80 transition-opacity">
              <div className="bg-white w-10 h-10 rounded-xl flex items-center justify-center text-dokany font-bold text-2xl">د</div>
              <span className="text-2xl font-black text-white tracking-tight">دكاني</span>
            </Link>
            <h1 className="text-4xl font-black text-white mb-6 leading-tight">أهلاً بك مجدداً في <br /> عالم التسوق الذكي</h1>
            <p className="text-emerald-100 text-lg leading-relaxed">سجل دخولك الآن لمتابعة طلباتك، إدارة منتجاتك، والاستمتاع بتجربة تسوق آمنة.</p>
          </div>
          <div className="absolute -bottom-24 -right-24 w-96 h-96 bg-emerald-500 rounded-full mix-blend-multiply filter blur-3xl opacity-50 animate-blob"></div>
        </div>

        {/* النصف الثاني: نموذج تسجيل الدخول */}
        <div className="w-full md:w-1/2 p-8 sm:p-12 lg:p-16 flex flex-col justify-center">
          <Link to="/" className="text-gray-400 hover:text-dokany flex items-center gap-2 w-fit mb-8 transition-colors">
            <ArrowRight size={20} /> العودة للرئيسية
          </Link>

          <h2 className="text-3xl font-black text-gray-800 mb-2">تسجيل الدخول</h2>
          <p className="text-gray-500 mb-6">أدخل بياناتك للمتابعة إلى حسابك</p>

          {/* عرض رسالة الخطأ إن وجدت */}
          {errorMsg && (
            <div className="bg-red-50 text-red-600 p-4 rounded-xl mb-6 font-medium text-sm">
              {errorMsg}
            </div>
          )}

          <form onSubmit={handleLogin} className="space-y-6">
            <div>
              <label className="block text-sm font-bold text-gray-700 mb-2">البريد الإلكتروني</label>
              <div className="relative">
                <Mail size={20} className="absolute right-4 top-3.5 text-gray-400" />
                <input 
                  type="email" 
                  required 
                  value={email} 
                  onChange={(e) => setEmail(e.target.value)} 
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none text-right" 
                  placeholder="example@email.com" 
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-bold text-gray-700 mb-2">كلمة المرور</label>
              <div className="relative">
                <Lock size={20} className="absolute right-4 top-3.5 text-gray-400" />
                <input 
                  type="password" 
                  required 
                  value={password} 
                  onChange={(e) => setPassword(e.target.value)} 
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pr-12 pl-4 focus:ring-2 focus:ring-dokany outline-none text-right" 
                  placeholder="••••••••" 
                />
              </div>
            </div>

            <button 
              type="submit" 
              disabled={isLoading}
              className="w-full bg-dokany text-white py-4 rounded-xl font-bold text-lg hover:bg-dokany-dark transition-colors shadow-lg shadow-emerald-500/30 flex justify-center items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
            >
              {isLoading ? <Loader2 size={24} className="animate-spin" /> : 'دخول'}
            </button>
          </form>

          <div className="mt-8 text-center text-gray-600">
            ليس لديك حساب؟ <Link to="/register" className="font-bold text-dokany hover:underline">أنشئ حساباً جديداً</Link>
          </div>
        </div>

      </div>
    </div>
  );
};

export default Login;