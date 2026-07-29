import React, { useState, useEffect } from 'react';
import { Check, X, Ban, Loader2, Users, ShieldCheck, ShieldAlert, Key, XCircle, Search, RefreshCw, AlertCircle } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const AdminVendors = () => {
  const [vendors, setVendors] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('ALL');

  const [isPermissionModalOpen, setIsPermissionModalOpen] = useState(false);
  const [selectedVendor, setSelectedVendor] = useState(null);
  const [vendorPermissions, setVendorPermissions] = useState([]);
  const [isPermissionLoading, setIsPermissionLoading] = useState(false);
  const [updatingPermissionType, setUpdatingPermissionType] = useState(null);

  const availablePermissions = [
    { type: 'CanUploadProducts', label: 'Add Products', desc: 'Allows vendor to upload new products.' },
    { type: 'CanEditProducts', label: 'Edit Products', desc: 'Allows vendor to edit product details.' },
    { type: 'CanDeleteProducts', label: 'Delete Products', desc: 'Allows vendor to delete products.' },
    { type: 'CanViewProducts', label: 'View Products', desc: 'Allows vendor to view their product list.' },
    { type: 'CanViewOrders', label: 'View Orders', desc: 'Allows vendor to view customer orders.' },
    { type: 'CanUpdateOrderStatus', label: 'Update Order Status', desc: 'Allows vendor to update order status.' },
    { type: 'CanCancelOrders', label: 'Cancel Orders', desc: 'Allows vendor to cancel orders.' },
    { type: 'CanViewAnalytics', label: 'View Analytics', desc: 'Allows vendor to view sales reports.' },
    { type: 'CanManageInventory', label: 'Manage Inventory', desc: 'Allows vendor to track & update stock.' }
  ];

  const fetchVendors = async () => {
    try {
      setIsLoading(true);
      const response = await axiosInstance.get('/Vendor', { params: { pageSize: 100 } });
      const rawData = response.data?.data;
      
      const vendorList = Array.isArray(rawData) 
        ? rawData 
        : (rawData?.items || rawData?.Items || rawData?.data || []);
      
      setVendors(vendorList);
    } catch (error) {
      console.error("Error fetching vendors:", error);
      if (error.response?.status === 403) {
        alert("You are not authorized to view vendors. Please sign in as Admin.");
      } else {
        alert("Error loading vendors from server.");
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
      const res = await axiosInstance.get(`/Permission/vendor/${vendor.id}`);
      const permissionsList = res.data?.data || [];
      
      // Normalize permission objects to consistently use systemName & isEnabled
      const normalized = Array.isArray(permissionsList) ? permissionsList.map(p => ({
        systemName: p.systemName || p.permissionName || p.type,
        isEnabled: Boolean(p.isEnabled)
      })) : [];

      setVendorPermissions(normalized);
    } catch (error) {
      console.error("Error fetching permissions:", error);
      setVendorPermissions([]);
    } finally {
      setIsPermissionLoading(false);
    }
  };

  const togglePermission = async (permissionType, isCurrentlyEnabled) => {
    setUpdatingPermissionType(permissionType);
    try {
      const endpoint = isCurrentlyEnabled ? 'disable' : 'enable';
      await axiosInstance.post(`/Permission/vendor/${selectedVendor.id}/${endpoint}/${permissionType}`, {});

      const newStatus = !isCurrentlyEnabled;

      setVendorPermissions(prev => {
        const exists = prev.some(p => (p.systemName || p.permissionName) === permissionType);
        if (exists) {
          return prev.map(p =>
            (p.systemName === permissionType || p.permissionName === permissionType)
              ? { ...p, isEnabled: newStatus }
              : p
          );
        } else {
          return [...prev, { systemName: permissionType, isEnabled: newStatus }];
        }
      });
    } catch (error) {
      console.error("Toggle permission error:", error);
      const errorMsg = error.response?.data?.message || error.response?.data?.errors?.[0] || "Error updating permission.";
      alert(errorMsg);
    } finally {
      setUpdatingPermissionType(null);
    }
  };

  const handleApprove = async (id) => {
    try {
      await axiosInstance.patch(`/Account/approve-vendor/${id}`);
      alert("Vendor approved successfully. You can now configure their permissions.");
      fetchVendors();
    } catch (error) { alert("Error approving vendor."); }
  };

  const handleDeactivate = async (id) => {
    if (window.confirm("Are you sure you want to deactivate this vendor account?")) {
      try {
        await axiosInstance.delete(`/Account/deactivate/${id}`);
        fetchVendors();
      } catch (error) { alert("Error deactivating vendor."); }
    }
  };

  const filteredVendors = vendors.filter(vendor => {
    const status = (vendor.accountStatus || '').toUpperCase();
    const matchesStatus = filterStatus === 'ALL' || 
      (filterStatus === 'ACTIVE' && status === 'ACTIVE') ||
      (filterStatus === 'PENDING' && status !== 'ACTIVE');

    const fullName = `${vendor.firstName || ''} ${vendor.secondName || ''}`.toLowerCase();
    const email = (vendor.email || '').toLowerCase();
    const store = (vendor.storeName || '').toLowerCase();
    const query = searchTerm.toLowerCase();

    const matchesSearch = fullName.includes(query) || email.includes(query) || store.includes(query);

    return matchesStatus && matchesSearch;
  });

  return (
    <div className="animate-fade-in-down p-2 sm:p-4" dir="ltr">
      <div className="mb-6 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-black text-gray-800 mb-1 flex items-center gap-2">
            <ShieldCheck className="text-emerald-600" size={30} />
            Vendors & Permissions Management
          </h1>
          <p className="text-gray-500 text-sm">Approve vendors and manage their access permissions dynamically.</p>
        </div>

        <button 
          onClick={fetchVendors} 
          disabled={isLoading}
          className="self-start md:self-auto bg-white text-gray-700 border border-gray-200 px-4 py-2.5 rounded-xl text-sm font-bold flex items-center gap-2 hover:bg-gray-50 transition-all shadow-sm"
        >
          <RefreshCw size={16} className={isLoading ? 'animate-spin text-emerald-600' : ''} />
          Refresh List
        </button>
      </div>

      {/* Controls Bar */}
      <div className="bg-white rounded-2xl shadow-sm p-4 sm:p-6 mb-6 border border-gray-100 flex flex-col sm:flex-row gap-4 justify-between items-center">
        <div className="relative flex-1 w-full">
          <Search className="absolute left-4 top-3.5 text-gray-400" size={18} />
          <input
            type="text"
            placeholder="Search vendor by name, email, or store..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none text-sm"
          />
        </div>

        <div className="flex items-center gap-3 w-full sm:w-auto">
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="w-full sm:w-auto bg-gray-50 border border-gray-200 rounded-xl px-4 py-3 focus:ring-2 focus:ring-emerald-500 outline-none text-sm font-bold text-gray-700 cursor-pointer"
          >
            <option value="ALL">All Vendors</option>
            <option value="ACTIVE">Active</option>
            <option value="PENDING">Pending Review</option>
          </select>

          <span className="bg-emerald-50 text-emerald-700 font-bold text-xs px-3 py-3 rounded-xl border border-emerald-100 whitespace-nowrap">
            {filteredVendors.length} Total
          </span>
        </div>
      </div>

      <div className="bg-white rounded-3xl border border-gray-100 shadow-sm overflow-hidden">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center p-16 text-emerald-600">
            <Loader2 className="animate-spin mb-3" size={40} />
            <p className="font-bold text-gray-500 text-sm">Loading vendor list...</p>
          </div>
        ) : filteredVendors.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full text-left whitespace-nowrap">
              <thead className="bg-gray-50 text-gray-600 font-bold border-b border-gray-100">
                <tr>
                  <th className="p-5">Vendor</th>
                  <th className="p-5">Store</th>
                  <th className="p-5 text-center">Status</th>
                  <th className="p-5 text-center">Permissions</th>
                  <th className="p-5 text-center">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {filteredVendors.map((vendor) => {
                  const status = (vendor.accountStatus || 'PENDING').toUpperCase();
                  return (
                    <tr key={vendor.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="p-5">
                        <div className="font-bold text-gray-800">{vendor.firstName} {vendor.secondName}</div>
                        <div className="text-xs text-gray-400">{vendor.email}</div>
                      </td>
                      <td className="p-5 text-gray-600 font-medium">{vendor.storeName || '---'}</td>
                      <td className="p-5 text-center">
                        <span className={`px-3 py-1.5 rounded-full text-xs font-bold ${status === 'ACTIVE' ? 'bg-emerald-100 text-emerald-600' : 'bg-amber-100 text-amber-600'}`}>
                          {status === 'ACTIVE' ? 'Active' : 'Pending Review'}
                        </span>
                      </td>
                      <td className="p-5 text-center">
                        <button
                          onClick={() => handleOpenPermissions(vendor)}
                          className="bg-gray-100 hover:bg-emerald-600 hover:text-white text-gray-600 px-4 py-2 rounded-xl text-sm font-bold transition-all flex items-center gap-2 mx-auto shadow-sm"
                        >
                          <Key size={16} /> Manage Permissions
                        </button>
                      </td>
                      <td className="p-5">
                        <div className="flex items-center justify-center gap-2">
                          {status !== 'ACTIVE' ? (
                            <button 
                              onClick={() => handleApprove(vendor.id)} 
                              className="p-2 text-emerald-600 bg-emerald-50 rounded-lg hover:bg-emerald-600 hover:text-white transition-all font-bold text-xs flex items-center gap-1"
                              title="Approve Vendor"
                            >
                              <Check size={18} /> Approve
                            </button>
                          ) : (
                            <button 
                              onClick={() => handleDeactivate(vendor.id)} 
                              className="p-2 text-red-500 bg-red-50 rounded-lg hover:bg-red-600 hover:text-white transition-all font-bold text-xs flex items-center gap-1"
                              title="Deactivate Vendor"
                            >
                              <Ban size={18} /> Deactivate
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="p-16 text-center">
            <AlertCircle size={48} className="mx-auto text-gray-300 mb-3" />
            <h3 className="text-lg font-bold text-gray-800 mb-1">No Vendors Found</h3>
            <p className="text-gray-500 text-sm max-w-md mx-auto">
              {searchTerm || filterStatus !== 'ALL' 
                ? 'No vendors match your search criteria. Try adjusting your filters.' 
                : 'There are currently no vendors registered in the database.'}
            </p>
          </div>
        )}
      </div>

      {/* Permissions Modal */}
      {isPermissionModalOpen && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4" dir="ltr">
          <div className="bg-white rounded-[2.5rem] w-full max-w-lg overflow-hidden shadow-2xl animate-fade-in-up">
            <div className="p-8 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
              <div>
                <h2 className="text-xl font-black text-gray-800">Permissions: {selectedVendor?.storeName || selectedVendor?.firstName}</h2>
                <p className="text-sm text-gray-500">Configure what this vendor is allowed to perform on the platform.</p>
              </div>
              <button onClick={() => setIsPermissionModalOpen(false)} className="text-gray-400 hover:text-red-500 transition-colors"><XCircle size={32} /></button>
            </div>

            <div className="p-8 max-h-[60vh] overflow-y-auto custom-scrollbar">
              {isPermissionLoading ? (
                <div className="flex justify-center p-10"><Loader2 className="animate-spin text-emerald-600" size={32} /></div>
              ) : (
                <div className="space-y-4">
                  {availablePermissions.map((perm) => {
                    const isEnabled = vendorPermissions.some(
                      p => (p.systemName === perm.type || p.permissionName === perm.type) && p.isEnabled === true
                    );
                    const isMutatingThis = updatingPermissionType === perm.type;

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
                          disabled={isMutatingThis}
                          onClick={() => togglePermission(perm.type, isEnabled)}
                          className={`w-14 h-8 flex-shrink-0 rounded-full relative transition-colors p-1 flex items-center ${isEnabled ? 'bg-emerald-500' : 'bg-gray-300'} ${isMutatingThis ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
                        >
                          {isMutatingThis ? (
                            <Loader2 size={16} className="animate-spin text-white mx-auto" />
                          ) : (
                            <div className={`w-6 h-6 bg-white rounded-full transition-all duration-300 shadow-sm ${isEnabled ? 'translate-x-6' : 'translate-x-0'}`}></div>
                          )}
                        </button>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            <div className="p-8 bg-gray-50 border-t border-gray-100 flex justify-end">
              <button onClick={() => setIsPermissionModalOpen(false)} className="bg-gray-900 text-white px-8 py-3 rounded-xl font-bold hover:bg-black transition-all">Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminVendors;