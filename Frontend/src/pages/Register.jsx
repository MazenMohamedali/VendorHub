import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Mail, Lock, User, Phone, MapPin, Store, ArrowLeft, ShieldCheck, Loader2 } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const Register = () => {
  const navigate = useNavigate();
  
  const [role, setRole] = useState('customer');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  const [formData, setFormData] = useState({
    firstName: '',
    secondName: '',
    email: '',
    password: '',
    confirmPassword: '',
    phoneNumber: '',
    address: '',
    storeName: '',
  });

  const validatePhone = (phone) => {
    if (!phone) return true; 
    const regex = /^01[0125][0-9]{8}$/;
    return regex.test(phone);
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');

    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match!');
      return;
    }

    if (formData.phoneNumber && !validatePhone(formData.phoneNumber)) {
      setError('Invalid phone number. Must start with 01 followed by 8 digits.');
      return;
    }

    setIsSubmitting(true);

    try {
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

      await axiosInstance.post(endpoint, payload);

      if (role === 'vendor') {
        alert("Vendor account created successfully! Your account is now pending admin approval.");
      } else {
        alert("Account created successfully! You can now sign in.");
      }
      navigate('/login');

    } catch (err) {
      console.error("Registration error:", err);
      setError(err.response?.data?.message || err.response?.data?.errors?.[0] || 'Registration failed. Please verify your data.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4 font-sans py-10" dir="ltr">
      <div className="max-w-6xl w-full bg-white rounded-3xl shadow-xl overflow-hidden flex flex-col md:flex-row">
        
        {/* Left Visual Branding */}
        <div className="hidden md:flex md:w-5/12 bg-emerald-600 p-12 flex-col justify-between relative overflow-hidden">
          <div className="relative z-10">
            <Link to="/" className="flex items-center gap-3 mb-12 w-fit hover:opacity-80 transition-opacity">
              <div className="bg-white w-10 h-10 rounded-xl flex items-center justify-center text-emerald-600 font-bold text-2xl">V</div>
              <span className="text-2xl font-black text-white tracking-tight">VendorHub</span>
            </Link>
            <h1 className="text-4xl font-black text-white mb-6 leading-tight">
              {role === 'customer' ? 'Start a Fun &\nSecure Shopping Journey' : 'Join Us as a Vendor\n& Grow Your Business'}
            </h1>
            <p className="text-emerald-100 text-lg leading-relaxed">
              {role === 'customer' 
                ? 'Create your account now to buy, track orders, and save your favorite products.' 
                : 'VendorHub provides all the tools you need to manage your products and sales professionally.'}
            </p>
          </div>
          <div className="absolute -bottom-24 -right-24 w-96 h-96 bg-emerald-500 rounded-full mix-blend-multiply filter blur-3xl opacity-50 animate-blob"></div>
        </div>

        {/* Right Form */}
        <div className="w-full md:w-7/12 p-8 sm:p-12">
          <Link to="/" className="text-gray-400 hover:text-emerald-600 flex items-center gap-2 w-fit mb-6 transition-colors">
            <ArrowLeft size={20} /> Back to Home
          </Link>

          <h2 className="text-3xl font-black text-gray-800 mb-6">Create New Account</h2>
          
          <div className="flex bg-gray-100 p-1 rounded-xl mb-8">
            <button 
              type="button"
              onClick={() => { setRole('customer'); setError(''); }}
              className={`flex-1 py-3 text-sm font-bold rounded-lg transition-all ${role === 'customer' ? 'bg-white text-emerald-600 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
            >
              Customer Account
            </button>
            <button 
              type="button"
              onClick={() => { setRole('vendor'); setError(''); }}
              className={`flex-1 py-3 text-sm font-bold rounded-lg transition-all ${role === 'vendor' ? 'bg-white text-emerald-600 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
            >
              Vendor Account
            </button>
          </div>

          {error && (
            <div className="bg-red-50 text-red-600 p-4 rounded-xl mb-6 font-medium text-sm">
              {error}
            </div>
          )}

          <form onSubmit={handleRegister} className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">First Name</label>
                <div className="relative">
                  <User size={18} className="absolute left-3.5 top-3.5 text-gray-400" />
                  <input 
                    type="text" 
                    required 
                    value={formData.firstName}
                    onChange={(e) => setFormData({...formData, firstName: e.target.value})}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-emerald-500 outline-none" 
                    placeholder="John" 
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">Last Name</label>
                <div className="relative">
                  <User size={18} className="absolute left-3.5 top-3.5 text-gray-400" />
                  <input 
                    type="text" 
                    required 
                    value={formData.secondName}
                    onChange={(e) => setFormData({...formData, secondName: e.target.value})}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-emerald-500 outline-none" 
                    placeholder="Doe" 
                  />
                </div>
              </div>
            </div>

            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1">Email Address</label>
              <div className="relative">
                <Mail size={18} className="absolute left-3.5 top-3.5 text-gray-400" />
                <input 
                  type="email" 
                  required 
                  value={formData.email}
                  onChange={(e) => setFormData({...formData, email: e.target.value})}
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-emerald-500 outline-none" 
                  placeholder="example@email.com" 
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1">Phone Number</label>
              <div className="relative">
                <Phone size={18} className="absolute left-3.5 top-3.5 text-gray-400" />
                <input 
                  type="tel" 
                  required
                  value={formData.phoneNumber}
                  onChange={(e) => setFormData({...formData, phoneNumber: e.target.value})}
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-emerald-500 outline-none" 
                  placeholder="01012345678" 
                />
              </div>
            </div>

            {role === 'customer' ? (
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">Delivery Address</label>
                <div className="relative">
                  <MapPin size={18} className="absolute left-3.5 top-3.5 text-gray-400" />
                  <input 
                    type="text" 
                    required 
                    value={formData.address}
                    onChange={(e) => setFormData({...formData, address: e.target.value})}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-emerald-500 outline-none" 
                    placeholder="123 Main Street, City" 
                  />
                </div>
              </div>
            ) : (
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">Store Name</label>
                <div className="relative">
                  <Store size={18} className="absolute left-3.5 top-3.5 text-gray-400" />
                  <input 
                    type="text" 
                    required 
                    value={formData.storeName}
                    onChange={(e) => setFormData({...formData, storeName: e.target.value})}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-emerald-500 outline-none" 
                    placeholder="My Awesome Store" 
                  />
                </div>
              </div>
            )}

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">Password</label>
                <div className="relative">
                  <Lock size={18} className="absolute left-3.5 top-3.5 text-gray-400" />
                  <input 
                    type="password" 
                    required 
                    value={formData.password}
                    onChange={(e) => setFormData({...formData, password: e.target.value})}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-emerald-500 outline-none" 
                    placeholder="••••••••" 
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">Confirm Password</label>
                <div className="relative">
                  <Lock size={18} className="absolute left-3.5 top-3.5 text-gray-400" />
                  <input 
                    type="password" 
                    required 
                    value={formData.confirmPassword}
                    onChange={(e) => setFormData({...formData, confirmPassword: e.target.value})}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-emerald-500 outline-none" 
                    placeholder="••••••••" 
                  />
                </div>
              </div>
            </div>

            <button 
              type="submit" 
              disabled={isSubmitting}
              className="w-full bg-emerald-600 text-white py-3.5 rounded-xl font-bold text-base hover:bg-emerald-700 transition-colors shadow-lg shadow-emerald-600/30 mt-4 flex justify-center items-center gap-2 disabled:opacity-70"
            >
              {isSubmitting ? <Loader2 size={20} className="animate-spin" /> : 'Create Account'}
            </button>
          </form>

          <div className="mt-6 text-center text-sm text-gray-600">
            Already have an account? <Link to="/login" className="font-bold text-emerald-600 hover:underline">Sign In</Link>
          </div>
        </div>

      </div>
    </div>
  );
};

export default Register;