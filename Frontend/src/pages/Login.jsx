import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Mail, Lock, ArrowLeft, Loader2 } from 'lucide-react';
import { useDispatch } from 'react-redux';
import { login } from '../store/authSlice';
import axiosInstance from '../api/axiosConfig';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');

  const navigate = useNavigate();
  const dispatch = useDispatch();

  const handleLogin = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setErrorMsg('');

    try {
      const loginResponse = await axiosInstance.post('/Account/login', {
        email: email,
        password: password
      });

      const token = loginResponse.data.data;
      localStorage.setItem('token', token);

      const userResponse = await axiosInstance.get('/Account/me');
      const userData = userResponse.data.data;
      dispatch(login(userData));

      if (userData.roles && userData.roles.includes('Admin')) {
        navigate('/admin');
      } else if (userData.roles && userData.roles.includes('Vendor')) {
        navigate('/vendor');
      } else {
        navigate('/');
      }

    } catch (error) {
      if (error.response && error.response.data) {
        setErrorMsg(error.response.data.message || 'Invalid login credentials.');
      } else {
        setErrorMsg('Connection error. Please make sure the backend server is running.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4 font-sans" dir="ltr">
      <div className="max-w-5xl w-full bg-white rounded-3xl shadow-xl overflow-hidden flex flex-col md:flex-row">
        
        {/* Left Visual Branding Panel */}
        <div className="hidden md:flex md:w-1/2 bg-emerald-600 p-12 flex-col justify-between relative overflow-hidden">
          <div className="relative z-10">
            <Link to="/" className="flex items-center gap-3 mb-12 w-fit hover:opacity-80 transition-opacity">
              <div className="bg-white w-10 h-10 rounded-xl flex items-center justify-center text-emerald-600 font-bold text-2xl">V</div>
              <span className="text-2xl font-black text-white tracking-tight">VendorHub</span>
            </Link>
            <h1 className="text-4xl font-black text-white mb-6 leading-tight">Welcome Back to <br /> VendorHub</h1>
            <p className="text-emerald-100 text-lg leading-relaxed">Sign in now to track your orders, manage products, and enjoy a seamless shopping experience.</p>
          </div>
          <div className="absolute -bottom-24 -right-24 w-96 h-96 bg-emerald-500 rounded-full mix-blend-multiply filter blur-3xl opacity-50 animate-blob"></div>
        </div>

        {/* Right Login Form */}
        <div className="w-full md:w-1/2 p-8 sm:p-12 lg:p-16 flex flex-col justify-center">
          <Link to="/" className="text-gray-400 hover:text-emerald-600 flex items-center gap-2 w-fit mb-8 transition-colors">
            <ArrowLeft size={20} /> Back to Home
          </Link>

          <h2 className="text-3xl font-black text-gray-800 mb-2">Sign In</h2>
          <p className="text-gray-500 mb-6">Enter your credentials to access your account</p>

          {errorMsg && (
            <div className="bg-red-50 text-red-600 p-4 rounded-xl mb-6 font-medium text-sm">
              {errorMsg}
            </div>
          )}

          <form onSubmit={handleLogin} className="space-y-6">
            <div>
              <label className="block text-sm font-bold text-gray-700 mb-2">Email Address</label>
              <div className="relative">
                <Mail size={20} className="absolute left-4 top-3.5 text-gray-400" />
                <input 
                  type="email" 
                  required 
                  value={email} 
                  onChange={(e) => setEmail(e.target.value)} 
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none text-left" 
                  placeholder="example@email.com" 
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-bold text-gray-700 mb-2">Password</label>
              <div className="relative">
                <Lock size={20} className="absolute left-4 top-3.5 text-gray-400" />
                <input 
                  type="password" 
                  required 
                  value={password} 
                  onChange={(e) => setPassword(e.target.value)} 
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none text-left" 
                  placeholder="••••••••" 
                />
              </div>
            </div>

            <button 
              type="submit" 
              disabled={isLoading}
              className="w-full bg-emerald-600 text-white py-4 rounded-xl font-bold text-lg hover:bg-emerald-700 transition-colors shadow-lg shadow-emerald-600/30 flex justify-center items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
            >
              {isLoading ? <Loader2 size={24} className="animate-spin" /> : 'Sign In'}
            </button>
          </form>

          <div className="mt-8 text-center text-gray-600">
            Don't have an account? <Link to="/register" className="font-bold text-emerald-600 hover:underline">Create a new account</Link>
          </div>
        </div>

      </div>
    </div>
  );
};

export default Login;