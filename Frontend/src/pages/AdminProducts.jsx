import React, { useState, useEffect } from 'react';
import { Check, X, Eye, Search, Filter, Package, Loader2, AlertCircle } from 'lucide-react';
import { getCategoryImageUrl, getProductImageUrl } from '../utils/imageUtils';
import axiosInstance from '../api/axiosConfig';

const AdminProducts = () => {
  const [products, setProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('PENDING');
  const [isApproving, setIsApproving] = useState(false);

  const fetchAdminProducts = async () => {
    try {
      setIsLoading(true);
      const response = await axiosInstance.get('/Admin/admin/all');
      const rawData = response.data?.data;
      setProducts(Array.isArray(rawData) ? rawData : (rawData?.items || []));
    } catch (error) {
      setProducts([]);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchAdminProducts();
  }, []);

  const filteredProducts = products.filter((product) => {
    const matchesStatus = 
      filterStatus === 'ALL' || 
      product.status === filterStatus || 
      (filterStatus === 'PENDING' && (product.status === 0 || product.status === 'PENDING')) ||
      (filterStatus === 'REVIEWED' && (product.status === 1 || product.status === 'REVIEWED')) ||
      (filterStatus === 'REJECTED' && (product.status === 2 || product.status === 'REJECTED'));

    const matchesSearch = 
      product.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.storeName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.vendorName?.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesStatus && matchesSearch;
  });

  const handleApprove = async (productId) => {
    try {
      setIsApproving(true);
      await axiosInstance.patch(`/Admin/${productId}/approve`);
      alert('Product approved successfully!');
      fetchAdminProducts();
      setSelectedProduct(null);
    } catch (error) {
      alert(error.response?.data?.message || 'Failed to approve product');
    } finally {
      setIsApproving(false);
    }
  };

  const handleReject = async (productId) => {
    if (!window.confirm('Are you sure you want to reject this product?')) return;

    try {
      setIsApproving(true);
      await axiosInstance.patch(`/Admin/${productId}/reject`);
      alert('Product rejected successfully');
      fetchAdminProducts();
      setSelectedProduct(null);
    } catch (error) {
      alert(error.response?.data?.message || 'Failed to reject product');
    } finally {
      setIsApproving(false);
    }
  };

  const getStatusBadge = (status) => {
    if (status === 'PENDING' || status === 0) {
      return <span className="bg-amber-100 text-amber-700 px-3 py-1 rounded-full text-xs font-bold">Pending Review</span>;
    }
    if (status === 'REVIEWED' || status === 1) {
      return <span className="bg-emerald-100 text-emerald-700 px-3 py-1 rounded-full text-xs font-bold">Approved</span>;
    }
    if (status === 'REJECTED' || status === 2) {
      return <span className="bg-red-100 text-red-700 px-3 py-1 rounded-full text-xs font-bold">Rejected</span>;
    }
    return <span className="bg-gray-100 text-gray-700 px-3 py-1 rounded-full text-xs font-bold">{status}</span>;
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-screen text-emerald-600">
        <Loader2 className="animate-spin" size={48} />
      </div>
    );
  }

  return (
    <div className="p-6 bg-gray-50 min-h-screen" dir="ltr">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-black text-gray-800 mb-2 flex items-center gap-3">
            <Package className="text-emerald-600" />
            Product Review & Approvals
          </h1>
          <p className="text-gray-600">Inspect and approve vendor product submissions before publishing</p>
        </div>

        {/* Controls */}
        <div className="bg-white rounded-2xl shadow-sm p-6 mb-6 border border-gray-100">
          <div className="flex flex-col lg:flex-row gap-4">
            {/* Search */}
            <div className="flex-1 relative">
              <Search className="absolute left-4 top-3.5 text-gray-400" size={20} />
              <input
                type="text"
                placeholder="Search by product name or store..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none text-left text-sm"
              />
            </div>

            {/* Filter */}
            <select
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              className="bg-gray-50 border border-gray-200 rounded-xl px-4 py-3 focus:ring-2 focus:ring-emerald-500 outline-none cursor-pointer min-w-[170px] text-sm font-medium"
            >
              <option value="ALL">All Statuses</option>
              <option value="PENDING">Pending Review</option>
              <option value="REVIEWED">Approved</option>
              <option value="REJECTED">Rejected</option>
            </select>
          </div>
        </div>

        {/* Products Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredProducts.length > 0 ? (
            filteredProducts.map((product) => (
              <div
                key={product.id}
                className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-all"
              >
                {/* Product Image */}
                <div className="w-full h-48 bg-gray-100 overflow-hidden relative">
                  <img
                    src={getProductImageUrl(product.imgUrl)}
                    alt={product.name}
                    className="w-full h-full object-cover"
                    onError={(e) => {
                      e.target.src = 'https://placehold.co/400x300?text=No+Image';
                    }}
                  />
                  <div className="absolute top-3 left-3">
                    {getStatusBadge(product.status)}
                  </div>
                </div>

                {/* Product Info */}
                <div className="p-4">
                  <h3 className="font-bold text-gray-800 mb-2 line-clamp-2">
                    {product.name}
                  </h3>

                  {/* Vendor & Price */}
                  <div className="mb-3 pb-3 border-b border-gray-100">
                    <p className="text-xs text-gray-500 mb-1">Store: {product.storeName || 'Verified Store'}</p>
                    <p className="text-lg font-black text-emerald-600">{product.price} EGP</p>
                  </div>

                  {/* Category & Quantity */}
                  <div className="mb-4 text-xs text-gray-600 space-y-1">
                    <p>Category: <span className="font-bold">{product.categoryName || 'Uncategorized'}</span></p>
                    <p>Quantity: <span className="font-bold">{product.quantity}</span></p>
                    <p>Views: <span className="font-bold">{product.viewersNo || 0}</span></p>
                  </div>

                  {/* Actions */}
                  <div className="flex gap-2">
                    <button
                      onClick={() => setSelectedProduct(product)}
                      className="flex-1 bg-gray-50 hover:bg-blue-50 text-gray-600 hover:text-blue-600 p-2.5 rounded-lg transition-all text-sm font-bold flex justify-center items-center"
                    >
                      <Eye size={18} />
                    </button>

                    {(product.status === 'PENDING' || product.status === 0) && (
                      <>
                        <button
                          onClick={() => handleApprove(product.id)}
                          disabled={isApproving}
                          className="flex-1 bg-emerald-50 hover:bg-emerald-100 text-emerald-600 p-2.5 rounded-lg transition-all text-sm font-bold disabled:opacity-50 flex justify-center items-center"
                        >
                          <Check size={18} />
                        </button>
                        <button
                          onClick={() => handleReject(product.id)}
                          disabled={isApproving}
                          className="flex-1 bg-red-50 hover:bg-red-100 text-red-600 p-2.5 rounded-lg transition-all text-sm font-bold disabled:opacity-50 flex justify-center items-center"
                        >
                          <X size={18} />
                        </button>
                      </>
                    )}
                  </div>
                </div>
              </div>
            ))
          ) : (
            <div className="col-span-full text-center py-12 bg-white rounded-2xl border border-gray-100">
              <AlertCircle size={48} className="mx-auto text-gray-300 mb-3" />
              <p className="text-gray-600 font-medium">No products match your search criteria</p>
            </div>
          )}
        </div>

        {/* Product Details Modal */}
        {selectedProduct && (
          <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4" dir="ltr">
            <div className="bg-white rounded-3xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
              {/* Modal Header */}
              <div className="sticky top-0 bg-gray-50 border-b border-gray-100 p-6 flex justify-between items-center">
                <h2 className="text-2xl font-black text-gray-800">Product Details</h2>
                <button
                  onClick={() => setSelectedProduct(null)}
                  className="text-gray-400 hover:text-red-500 transition-colors"
                >
                  <X size={28} />
                </button>
              </div>

              {/* Modal Content */}
              <div className="p-6 space-y-6">
                {/* Image */}
                <div className="w-full h-72 bg-gray-100 rounded-2xl overflow-hidden">
                  <img
                    src={getProductImageUrl(selectedProduct.imgUrl)}
                    alt={selectedProduct.name}
                    className="w-full h-full object-cover"
                    onError={(e) => {
                      e.target.src = 'https://placehold.co/600x400?text=No+Image';
                    }}
                  />
                </div>

                {/* Basic Info */}
                <div>
                  <h3 className="text-2xl font-black text-gray-800 mb-4">
                    {selectedProduct.name}
                  </h3>

                  <div className="grid grid-cols-2 gap-4 mb-4">
                    <div className="bg-gray-50 p-4 rounded-xl">
                      <p className="text-xs text-gray-500 mb-1">Price</p>
                      <p className="text-xl font-black text-emerald-600">
                        {selectedProduct.price} EGP
                      </p>
                    </div>
                    <div className="bg-gray-50 p-4 rounded-xl">
                      <p className="text-xs text-gray-500 mb-1">Available Quantity</p>
                      <p className="text-xl font-black text-gray-800">
                        {selectedProduct.quantity}
                      </p>
                    </div>
                    <div className="bg-gray-50 p-4 rounded-xl">
                      <p className="text-xs text-gray-500 mb-1">Vendor Store</p>
                      <p className="text-lg font-bold text-gray-800">
                        {selectedProduct.storeName || 'Verified Store'}
                      </p>
                    </div>
                    <div className="bg-gray-50 p-4 rounded-xl">
                      <p className="text-xs text-gray-500 mb-1">Category</p>
                      <p className="text-lg font-bold text-gray-800">
                        {selectedProduct.categoryName || 'Uncategorized'}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Description */}
                {selectedProduct.description && (
                  <div>
                    <h4 className="font-bold text-gray-800 mb-2">Description</h4>
                    <p className="text-gray-700 leading-relaxed">
                      {selectedProduct.description}
                    </p>
                  </div>
                )}

                {/* Status & Actions */}
                <div className="pt-4 border-t border-gray-100">
                  <div className="mb-4">
                    <p className="text-sm text-gray-600 mb-2">Current Status:</p>
                    {getStatusBadge(selectedProduct.status)}
                  </div>

                  {(selectedProduct.status === 'PENDING' || selectedProduct.status === 0) && (
                    <div className="flex gap-3">
                      <button
                        onClick={() => handleApprove(selectedProduct.id)}
                        disabled={isApproving}
                        className="flex-1 bg-emerald-600 hover:bg-emerald-700 text-white p-3 rounded-xl font-bold transition-all flex items-center justify-center gap-2 disabled:opacity-50"
                      >
                        <Check size={20} />
                        Approve Product
                      </button>
                      <button
                        onClick={() => handleReject(selectedProduct.id)}
                        disabled={isApproving}
                        className="flex-1 bg-red-500 hover:bg-red-600 text-white p-3 rounded-xl font-bold transition-all flex items-center justify-center gap-2 disabled:opacity-50"
                      >
                        <X size={20} />
                        Reject Product
                      </button>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminProducts;