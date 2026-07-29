import { getImageUrl } from '../utils/imageUtils';
import React, { useState, useEffect } from 'react';
import { Plus, Package, Edit, Trash2, Loader2, ImagePlus, X } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';
import { jwtDecode } from 'jwt-decode';

const VendorProducts = () => {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState('add');
  const [editingProductId, setEditingProductId] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [formData, setFormData] = useState({
    name: '',
    price: '',
    quantity: '',
    categoryId: '',
    productionDate: '', 
    expireDate: '',     
    imageFile: null,
    imagePreview: null
  });

  const getVendorId = () => {
    try {
      const token = localStorage.getItem('token');
      if (!token) return null;
      const decoded = jwtDecode(token);
      const dotnetNameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
      return decoded[dotnetNameIdentifier] || decoded.nameid || decoded.sub;
    } catch (error) {
      return null;
    }
  };

  const fetchData = async () => {
    try {
      setIsLoading(true);
      
      const productsRes = await axiosInstance.get('/Product/my-products');
      const rawProducts = productsRes.data?.data;
      const productList = Array.isArray(rawProducts) ? rawProducts : (rawProducts?.items || []);
      setProducts(productList);

      const categoriesRes = await axiosInstance.get('/Category/active');
      const rawCategories = categoriesRes.data?.data;
      const categoryList = Array.isArray(rawCategories) ? rawCategories : (rawCategories?.items || []);
      setCategories(categoryList);

    } catch (error) {
      console.error("Error fetching data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      const previewUrl = URL.createObjectURL(file);
      setFormData({ 
        ...formData, 
        imageFile: file,
        imagePreview: previewUrl
      });
    }
  };

  const handleOpenAdd = () => {
    setModalMode('add');
    setEditingProductId(null);
    setFormData({
      name: '',
      price: '',
      quantity: '',
      categoryId: '',
      productionDate: '',
      expireDate: '',
      imageFile: null,
      imagePreview: null
    });
    setIsModalOpen(true);
  };

  const handleOpenEdit = (product) => {
    setModalMode('edit');
    setEditingProductId(product.id);
    setFormData({
      name: product.name || '',
      price: product.price || '',
      quantity: (product.unitsInStock !== undefined && product.unitsInStock !== null) 
        ? product.unitsInStock 
        : (product.quantity !== undefined && product.quantity !== null ? product.quantity : ''),
      categoryId: product.categoryId || '',
      productionDate: product.productionDate ? product.productionDate.split('T')[0] : '',
      expireDate: product.expireDate ? product.expireDate.split('T')[0] : '',
      imageFile: null,
      imagePreview: getImageUrl(product.imgUrl, 'Products')
    });
    setIsModalOpen(true);
  };

  const handleDelete = async (productId) => {
    if (window.confirm("Are you sure you want to delete this product?")) {
      try {
        await axiosInstance.delete(`/Product/${productId}`);
        alert("Product deleted successfully.");
        fetchData();
      } catch (error) {
        console.error("Delete product error:", error);
        alert(error.response?.data?.message || "Failed to delete product.");
      }
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSaving(true);

    try {
      const vendorId = getVendorId();
      if (!vendorId) {
        alert("Session expired. Please sign in again.");
        setIsSaving(false);
        return;
      }

      if (!formData.categoryId) {
        alert("Please select a category for the product.");
        setIsSaving(false);
        return;
      }

      const data = new FormData();
      data.append('Name', formData.name);
      data.append('Price', formData.price);
      data.append('Quantity', formData.quantity);
      data.append('CategoryId', formData.categoryId);
      data.append('VendorId', vendorId);
      if (formData.imageFile) data.append('ImageFile', formData.imageFile);
      
      if (formData.productionDate) data.append('ProductionDate', formData.productionDate);
      if (formData.expireDate) data.append('ExpireDate', formData.expireDate);

      if (modalMode === 'add') {
        await axiosInstance.post('/Product', data);
        alert("Product added successfully!");
      } else {
        await axiosInstance.put(`/Product/${editingProductId}`, data);
        alert("Product updated successfully!");
      }

      handleCloseModal();
      fetchData(); 
      
    } catch (error) {
      const backendError = error.response?.data;
      let errorMsg = backendError?.message || (modalMode === 'add' ? "Failed to add product" : "Failed to update product");
      if (backendError?.errors) errorMsg = Object.values(backendError.errors).flat().join('\n');
      alert(errorMsg);
    } finally {
      setIsSaving(false);
    }
  };

  const getProductImageUrl = (imgUrl) => {
    return getImageUrl(imgUrl, 'Products');
  };

  const handleCloseModal = () => {
    if (formData.imagePreview && formData.imageFile) {
      URL.revokeObjectURL(formData.imagePreview);
    }
    setIsModalOpen(false);
    setEditingProductId(null);
    setFormData({ 
      name: '', 
      price: '', 
      quantity: '', 
      categoryId: '', 
      productionDate: '', 
      expireDate: '', 
      imageFile: null,
      imagePreview: null
    });
  };

  return (
    <div className="p-6" dir="ltr">
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-2xl font-black text-gray-800 flex items-center gap-2">
          <Package className="text-emerald-600" /> My Products ({products.length})
        </h1>
        <button 
          onClick={handleOpenAdd}
          className="bg-emerald-600 text-white px-6 py-3 rounded-2xl font-bold flex items-center gap-2 hover:bg-emerald-700 transition-all shadow-lg shadow-emerald-600/20"
        >
          <Plus size={20} /> Add New Product
        </button>
      </div>

      {isLoading ? (
        <div className="flex justify-center p-20"><Loader2 className="animate-spin text-emerald-600" size={40} /></div>
      ) : products.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {products.map((product) => (
            <div key={product.id} className="bg-white rounded-3xl p-4 border border-gray-100 shadow-sm hover:shadow-md transition-all flex flex-col">
              <div className="relative mb-4">
                <img 
                  src={getProductImageUrl(product.imgUrl)}
                  alt={product.name}
                  className="w-full h-48 object-contain mix-blend-multiply rounded-2xl bg-gray-50 p-2"
                  onError={(e) => { e.target.src = 'https://placehold.co/400x400?text=No+Image'; }}
                />
                <span className="absolute top-2 right-2 bg-gray-900/80 backdrop-blur-sm text-white text-[10px] font-bold px-2.5 py-1 rounded-full">
                  Stock: {product.unitsInStock ?? product.quantity ?? 'N/A'}
                </span>
              </div>

              <h3 className="font-bold text-gray-800 mb-1 line-clamp-1">{product.name}</h3>
              <p className="text-xs text-gray-400 mb-2">{product.categoryName || 'General'}</p>
              <p className="text-emerald-600 font-black text-lg mb-4 mt-auto">{product.price} EGP</p>
              
              <div className="flex gap-2">
                <button 
                  onClick={() => handleOpenEdit(product)}
                  className="flex-1 bg-blue-50 text-blue-600 p-2.5 rounded-xl hover:bg-blue-600 hover:text-white transition-all font-bold text-xs flex items-center justify-center gap-1.5"
                  title="Edit Product"
                >
                  <Edit size={16} /> Edit
                </button>
                <button 
                  onClick={() => handleDelete(product.id)}
                  className="flex-1 bg-rose-50 text-rose-600 p-2.5 rounded-xl hover:bg-rose-600 hover:text-white transition-all font-bold text-xs flex items-center justify-center gap-1.5"
                  title="Delete Product"
                >
                  <Trash2 size={16} /> Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-center py-20 bg-white rounded-3xl border border-gray-100">
          <Package size={48} className="mx-auto text-gray-300 mb-3" />
          <h3 className="text-lg font-bold text-gray-800 mb-1">No Products Found</h3>
          <p className="text-gray-500 text-sm max-w-md mx-auto mb-6">You haven't uploaded any products yet. Click the button below to add your first product.</p>
          <button 
            onClick={handleOpenAdd}
            className="bg-emerald-600 text-white px-6 py-2.5 rounded-xl font-bold text-sm shadow-md"
          >
            Add First Product
          </button>
        </div>
      )}

      {/* Add / Edit Product Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 overflow-y-auto" dir="ltr">
          <div className="bg-white rounded-[2.5rem] w-full max-w-lg p-8 animate-fade-in-up my-8 shadow-2xl relative">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-xl font-black text-gray-800">
                {modalMode === 'add' ? 'Add New Product' : 'Edit Product'}
              </h2>
              <button onClick={handleCloseModal} className="text-gray-400 hover:text-rose-600"><X size={24} /></button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              
              <div>
                <label className="block text-sm font-bold mb-2 text-gray-700">Product Name</label>
                <input 
                  required 
                  type="text" 
                  className="w-full p-4 bg-gray-50 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-emerald-500 text-left outline-none text-sm" 
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})} 
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-bold mb-2 text-gray-700">Price (EGP)</label>
                  <input 
                    required 
                    type="number" 
                    step="0.01" 
                    className="w-full p-4 bg-gray-50 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-emerald-500 text-left outline-none text-sm" 
                    value={formData.price}
                    onChange={(e) => setFormData({...formData, price: e.target.value})} 
                  />
                </div>
                <div>
                  <label className="block text-sm font-bold mb-2 text-gray-700">Quantity</label>
                  <input 
                    required 
                    type="number" 
                    className="w-full p-4 bg-gray-50 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-emerald-500 text-left outline-none text-sm" 
                    value={formData.quantity}
                    onChange={(e) => setFormData({...formData, quantity: e.target.value})} 
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold mb-2 text-gray-700">Category</label>
                <select 
                  required 
                  className="w-full p-4 bg-gray-50 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-emerald-500 appearance-none cursor-pointer text-left outline-none text-sm font-bold"
                  value={formData.categoryId}
                  onChange={(e) => setFormData({...formData, categoryId: e.target.value})}
                >
                  <option value="" disabled>-- Select Category --</option>
                  {categories.map((cat) => (
                    <option key={cat.id} value={cat.id}>{cat.name}</option>
                  ))}
                </select>
                {categories.length === 0 && <p className="text-xs text-amber-600 mt-1 ml-2">Loading categories...</p>}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-bold mb-2 text-gray-600 text-xs">Production Date (Optional)</label>
                  <input 
                    type="date" 
                    className="w-full p-4 bg-gray-50 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-emerald-500 text-xs outline-none" 
                    value={formData.productionDate}
                    onChange={(e) => setFormData({...formData, productionDate: e.target.value})} 
                  />
                </div>
                <div>
                  <label className="block text-sm font-bold mb-2 text-gray-600 text-xs">Expiration Date (Optional)</label>
                  <input 
                    type="date" 
                    className="w-full p-4 bg-gray-50 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-emerald-500 text-xs outline-none" 
                    value={formData.expireDate}
                    onChange={(e) => setFormData({...formData, expireDate: e.target.value})} 
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold mb-2 text-gray-700">Product Image</label>
                <div className="relative h-36 w-full bg-gray-50 rounded-2xl border-2 border-dashed border-gray-200 flex items-center justify-center overflow-hidden cursor-pointer hover:border-emerald-600 transition-colors">
                  {formData.imagePreview ? (
                    <img 
                      src={formData.imagePreview}
                      alt="Product Preview"
                      className="w-full h-full object-contain p-2 mix-blend-multiply"
                    />
                  ) : (
                    <div className="text-center text-gray-400">
                      <ImagePlus className="mx-auto mb-1" size={32} />
                      <span className="text-xs">Click or drag image here to upload</span>
                    </div>
                  )}
                  <input 
                    type="file" 
                    accept="image/*" 
                    onChange={handleFileChange} 
                    className="absolute inset-0 opacity-0 cursor-pointer" 
                  />
                </div>
                {formData.imageFile && (
                  <p className="text-xs text-emerald-600 mt-2 font-medium">
                    ✓ New image selected: {formData.imageFile.name}
                  </p>
                )}
              </div>

              <div className="flex gap-4 mt-8 pt-4">
                <button 
                  type="submit" 
                  disabled={isSaving} 
                  className="flex-1 bg-emerald-600 text-white py-4 rounded-2xl font-bold hover:bg-emerald-700 transition-all disabled:bg-gray-400 flex justify-center items-center gap-2 shadow-lg shadow-emerald-600/20"
                >
                  {isSaving ? <Loader2 className="animate-spin" size={20}/> : (modalMode === 'add' ? 'Save Product' : 'Update Product')}
                </button>
                <button 
                  type="button" 
                  onClick={handleCloseModal} 
                  className="flex-1 bg-gray-100 text-gray-600 py-4 rounded-2xl font-bold hover:bg-gray-200 transition-all"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default VendorProducts;