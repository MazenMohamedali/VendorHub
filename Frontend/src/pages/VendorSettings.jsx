import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Store, Phone, Mail, User, ShieldCheck, Loader2, MapPin, AlignLeft } from 'lucide-react';
import { vendorApi } from '../api';
import { useToast } from '../components/Toast';
import { login } from '../store/authSlice';

const VendorSettings = () => {
  const user = useSelector((state) => state.auth.user);
  const dispatch = useDispatch();
  const { showSuccess, showError } = useToast();

  const [isLoading, setIsLoading] = useState(false);
  const [isFetching, setIsFetching] = useState(true);

  const [formData, setFormData] = useState({
    storeName: '',
    description: '',
    phoneNumber: '',
    address: '',
    logoUrl: '',
    email: '',
  });

  const fetchProfileData = async () => {
    try {
      setIsFetching(true);
      const response = await vendorApi.getProfile();
      const data = response.data?.data || {};
      setFormData({
        storeName: data.storeName || user?.storeName || '',
        description: data.description || '',
        phoneNumber: data.phoneNumber || user?.phoneNumber || '',
        address: data.address || '',
        logoUrl: data.logoUrl || '',
        email: data.email || user?.email || '',
      });
    } catch (error) {
      console.error("Error fetching vendor profile:", error);
      if (user) {
        setFormData({
          storeName: user.storeName || '',
          description: '',
          phoneNumber: user.phoneNumber || '',
          address: '',
          logoUrl: '',
          email: user.email || '',
        });
      }
    } finally {
      setIsFetching(false);
    }
  };

  useEffect(() => {
    fetchProfileData();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);

    try {
      await vendorApi.updateProfile({
        storeName: formData.storeName,
        description: formData.description,
        phoneNumber: formData.phoneNumber,
        address: formData.address,
        logoUrl: formData.logoUrl,
      });

      showSuccess('Store settings updated successfully!');

      if (user) {
        dispatch(login({ ...user, storeName: formData.storeName, phoneNumber: formData.phoneNumber }));
      }
    } catch (error) {
      console.error("Error updating profile:", error);
      showError(error.response?.data?.message || 'Error saving settings.');
    } finally {
      setIsLoading(false);
    }
  };

  if (isFetching) {
    return (
      <div className="flex justify-center items-center min-h-[50vh] text-emerald-600">
        <Loader2 className="animate-spin" size={40} />
      </div>
    );
  }

  return (
    <div className="animate-fade-in-down" dir="ltr">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-2">Store Settings</h1>
        <p className="text-gray-500 text-sm">Manage your business profile and customer contact details.</p>
      </div>

      <div className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden max-w-4xl">
        <div className="p-8 md:p-12">
          <form onSubmit={handleSubmit} className="space-y-8">
            
            {/* Store Information */}
            <div>
              <h2 className="text-lg font-black text-gray-800 mb-6 flex items-center gap-2 border-b border-gray-100 pb-3">
                <Store className="text-emerald-600" size={20} />
                Store Identity
              </h2>
              <div className="grid grid-cols-1 gap-6">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">Store Name (Commercial)</label>
                  <input
                    type="text"
                    required
                    value={formData.storeName}
                    onChange={(e) => setFormData({ ...formData, storeName: e.target.value })}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 px-4 focus:ring-2 focus:ring-emerald-500 outline-none text-left"
                    placeholder="My Store Name"
                  />
                  <p className="text-xs text-gray-400 mt-2">Name displayed to customers on products and invoices.</p>
                </div>

                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">Store Description</label>
                  <div className="relative">
                    <AlignLeft size={20} className="absolute left-4 top-3.5 text-gray-400" />
                    <textarea
                      rows={3}
                      value={formData.description}
                      onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                      className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none resize-none text-left text-sm"
                      placeholder="Brief description of your store and products..."
                    ></textarea>
                  </div>
                </div>
              </div>
            </div>

            {/* Contact Details */}
            <div>
              <h2 className="text-lg font-black text-gray-800 mb-6 flex items-center gap-2 border-b border-gray-100 pb-3">
                <User className="text-emerald-600" size={20} />
                Contact Information & Address
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">Email Address</label>
                  <div className="relative">
                    <Mail size={20} className="absolute left-4 top-3.5 text-gray-400" />
                    <input
                      type="email"
                      disabled
                      value={formData.email}
                      className="w-full bg-gray-100 border border-gray-200 rounded-xl py-3 pl-12 pr-4 text-gray-500 cursor-not-allowed text-left text-sm"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">Phone Number</label>
                  <div className="relative">
                    <Phone size={20} className="absolute left-4 top-3.5 text-gray-400" />
                    <input
                      type="tel"
                      value={formData.phoneNumber}
                      onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                      className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none text-left text-sm"
                      placeholder="01012345678"
                    />
                  </div>
                </div>

                <div className="md:col-span-2">
                  <label className="block text-sm font-bold text-gray-700 mb-2">Store Address / Headquarters</label>
                  <div className="relative">
                    <MapPin size={20} className="absolute left-4 top-3.5 text-gray-400" />
                    <input
                      type="text"
                      value={formData.address}
                      onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                      className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none text-left text-sm"
                      placeholder="City, Street, Building No..."
                    />
                  </div>
                </div>
              </div>
            </div>

            <div className="pt-4 border-t border-gray-100 flex items-center gap-4">
              <button
                type="submit"
                disabled={isLoading}
                className="bg-emerald-600 text-white px-8 py-4 rounded-xl font-bold text-lg hover:bg-emerald-700 transition-colors shadow-lg shadow-emerald-600/30 flex items-center gap-2 disabled:opacity-70"
              >
                {isLoading ? <Loader2 size={24} className="animate-spin" /> : 'Save Settings'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default VendorSettings;