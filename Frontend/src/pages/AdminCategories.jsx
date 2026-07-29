import { getImageUrl } from '../utils/imageUtils';
import React, { useState, useEffect, useRef } from 'react';
import { Plus, Edit, Trash2, PowerOff, X, FolderTree, Image as ImageIcon, Loader2, Upload } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';

const AdminCategories = () => {
  const [categories, setCategories] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState('add');
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const [formData, setFormData] = useState({
    id: '',
    name: '',
    isActive: true,
    imageFile: null,
    imagePreview: ''
  });

  const fileInputRef = useRef(null);

  const fetchCategories = async () => {
    try {
      setIsLoading(true);
      const response = await axiosInstance.get('/Category/admin/all');
      const rawData = response.data?.data;
      setCategories(Array.isArray(rawData) ? rawData : (rawData?.items || []));
    } catch (error) {
      console.error("Error fetching categories:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchCategories();
  }, []);

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setFormData({
        ...formData,
        imageFile: file,
        imagePreview: URL.createObjectURL(file)
      });
    }
  };

  const handleOpenAdd = () => {
    setModalMode('add');
    setFormData({ id: '', name: '', isActive: true, imageFile: null, imagePreview: '' });
    setIsModalOpen(true);
  };

  const handleOpenEdit = (category) => {
    setModalMode('edit');
    setFormData({ 
      id: category.id, 
      name: category.name, 
      isActive: category.isActive,
      imageFile: null,
      imagePreview: getImageUrl(category.imageUrl, 'Categories')
    });
    setIsModalOpen(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    
    try {
      const submitData = new FormData();
      submitData.append("Name", formData.name ? formData.name.trim() : "");
      
      if (formData.imageFile) {
        submitData.append("ImageFile", formData.imageFile);
      }
      
      if (modalMode !== 'add') {
        submitData.append("IsActive", formData.isActive);
      }

      if (modalMode === 'add') {
         await axiosInstance.post('/Category', submitData);
         alert("Category added successfully.");
      } else {
         await axiosInstance.put(`/Category/${formData.id}`, submitData);
         alert("Category updated successfully.");
      }
      fetchCategories();
      setIsModalOpen(false);
    } catch (error) {
      console.error("Error saving category:", error);
      alert(error.response?.data?.message || "Error saving category.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleToggleActive = async (id, currentStatus) => {
    const actionName = currentStatus ? 'deactivate' : 'activate';
    if (window.confirm(`Are you sure you want to ${actionName} this category?`)) {
        try {
            const categoryToUpdate = categories.find(c => c.id === id);
            
            const submitData = new FormData();
            submitData.append("Name", categoryToUpdate.name);
            submitData.append("IsActive", !currentStatus);

            await axiosInstance.put(`/Category/${id}`, submitData);
            fetchCategories();
        } catch (error) {
            alert(`Error trying to ${actionName} category.`);
        }
    }
  };

  const handleHardDelete = async (id) => {
    if (window.confirm('Are you sure you want to permanently delete this category? This action cannot be undone and will delete all associated products.')) {
      try {
        await axiosInstance.delete(`/Category/${id}/hard`);
        alert("Category permanently deleted.");
        fetchCategories();
      } catch (error) {
        console.error("Error hard deleting category:", error);
        alert("Error deleting category.");
      }
    }
  };

  return (
    <div className="animate-fade-in-down relative" dir="ltr">
      
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-800 mb-2 flex items-center gap-2">
            <FolderTree className="text-emerald-600" size={28} />
            Categories Management
          </h1>
          <p className="text-gray-500 text-sm">Add new categories and control their visibility to customers.</p>
        </div>
        <button onClick={handleOpenAdd} className="bg-emerald-600 text-white px-6 py-3 rounded-xl font-bold hover:bg-emerald-700 transition-colors flex items-center gap-2 shadow-lg shadow-emerald-500/30">
          <Plus size={20} /> New Category
        </button>
      </div>

      {/* Categories Table */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-x-auto">
        {isLoading ? (
            <div className="flex justify-center p-10 text-emerald-600"><Loader2 className="animate-spin" size={32} /></div>
        ) : (
            <table className="w-full text-left whitespace-nowrap">
            <thead className="bg-gray-50 text-gray-600 font-bold border-b border-gray-100">
                <tr>
                <th className="p-4">Category</th>
                <th className="p-4">Status</th>
                <th className="p-4 text-center">Products Count</th>
                <th className="p-4 text-center">Actions</th>
                </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
                {categories.length > 0 ? categories.map((category) => (
                <tr key={category.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="p-4">
                    <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-gray-50 border border-gray-100 rounded-lg flex items-center justify-center p-1 shrink-0">
                        <img 
                          src={getImageUrl(category.imageUrl, 'Categories')} 
                          alt={category.name} 
                          className="max-h-full mix-blend-multiply object-contain rounded" 
                          onError={(e) => e.target.src = "https://placehold.co/100x100?text=Error"} 
                        />
                        </div>
                        <div>
                        <p className="font-bold text-gray-800">{category.name}</p>
                        <p className="text-xs text-gray-400 mt-1">Created: {category.createdAt ? new Date(category.createdAt).toLocaleDateString('en-US') : 'N/A'}</p>
                        </div>
                    </div>
                    </td>
                    <td className="p-4">
                    {category.isActive ? (
                        <span className="bg-emerald-100 text-emerald-600 text-xs px-3 py-1.5 rounded-full font-bold">Active</span>
                    ) : (
                        <span className="bg-gray-100 text-gray-500 text-xs px-3 py-1.5 rounded-full font-bold">Disabled (Soft Deleted)</span>
                    )}
                    </td>
                    <td className="p-4 text-center">
                    <span className="font-bold text-gray-800 bg-gray-50 px-3 py-1 rounded-lg border border-gray-200">
                        {category.productCount || 0} Products
                    </span>
                    </td>
                    <td className="p-4">
                    <div className="flex items-center justify-center gap-2">
                        <button onClick={() => handleOpenEdit(category)} className="p-2 text-blue-500 hover:bg-blue-50 rounded-lg transition-colors" title="Edit">
                        <Edit size={18} />
                        </button>
                        <button onClick={() => handleToggleActive(category.id, category.isActive)} className={`p-2 rounded-lg transition-colors ${category.isActive ? 'text-amber-500 hover:bg-amber-50' : 'text-emerald-500 hover:bg-emerald-50'}`} title={category.isActive ? 'Deactivate' : 'Activate'}>
                        <PowerOff size={18} />
                        </button>
                        <button onClick={() => handleHardDelete(category.id)} className="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors" title="Permanent Delete">
                        <Trash2 size={18} />
                        </button>
                    </div>
                    </td>
                </tr>
                )) : (
                    <tr><td colSpan="4" className="p-8 text-center text-gray-500">No categories found.</td></tr>
                )}
            </tbody>
            </table>
        )}
      </div>

      {/* Add / Edit Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4" dir="ltr">
          <div className="bg-white rounded-3xl w-full max-w-md p-8 shadow-2xl animate-fade-in-up">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-xl font-bold text-gray-800">
                {modalMode === 'add' ? 'Add New Category' : 'Edit Category'}
              </h2>
              <button onClick={() => setIsModalOpen(false)} className="text-gray-400 hover:text-red-500"><X size={24} /></button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6">
              
              {/* Category Image Upload */}
              <div className="flex flex-col items-center justify-center w-full">
                <input 
                  type="file" 
                  accept="image/*" 
                  ref={fileInputRef}
                  onChange={handleImageChange} 
                  className="hidden" 
                  required={modalMode === 'add'}
                />
                <div 
                  onClick={() => fileInputRef.current.click()}
                  className={`w-full h-40 border-2 border-dashed rounded-2xl flex flex-col items-center justify-center cursor-pointer transition-colors overflow-hidden relative ${
                    formData.imagePreview ? 'border-emerald-200 bg-emerald-50/30' : 'border-gray-200 bg-gray-50 hover:bg-gray-100 hover:border-gray-300'
                  }`}
                >
                  {formData.imagePreview ? (
                      <img src={formData.imagePreview} alt="Preview" className="h-full object-contain mix-blend-multiply p-2" />
                    ) : (
                    <>
                      <div className="w-12 h-12 bg-white rounded-full flex items-center justify-center shadow-sm text-gray-400 mb-2">
                        <Upload size={24} />
                      </div>
                      <p className="font-bold text-gray-600 text-sm">Click here to upload category image</p>
                      <p className="text-xs text-gray-400 mt-1">JPG, PNG (Max 2MB)</p>
                    </>
                  )}
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">Category Name (2-100 characters)</label>
                <input 
                  type="text" 
                  required 
                  minLength="2" 
                  maxLength="100" 
                  value={formData.name} 
                  onChange={(e) => setFormData({...formData, name: e.target.value})} 
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl px-4 py-3 focus:ring-2 focus:ring-emerald-500 outline-none text-left" 
                  placeholder="e.g. Home Appliances" 
                />
              </div>

              {modalMode === 'edit' && (
                <div className="flex items-center gap-3 bg-gray-50 p-4 rounded-xl border border-gray-200">
                  <input 
                    type="checkbox" 
                    id="isActive" 
                    checked={formData.isActive} 
                    onChange={(e) => setFormData({...formData, isActive: e.target.checked})} 
                    className="w-5 h-5 accent-emerald-600 rounded cursor-pointer" 
                  />
                  <label htmlFor="isActive" className="font-bold text-gray-700 cursor-pointer select-none">
                    Active (Visible to customers)
                  </label>
                </div>
              )}

              <div className="pt-4 flex gap-4">
                <button type="submit" disabled={isSubmitting} className="flex-1 bg-emerald-600 text-white py-3 rounded-xl font-bold hover:bg-emerald-700 transition-colors flex justify-center items-center">
                  {isSubmitting ? <Loader2 className="animate-spin" size={20} /> : (modalMode === 'add' ? 'Add Category' : 'Save Changes')}
                </button>
                <button type="button" onClick={() => setIsModalOpen(false)} className="bg-gray-100 text-gray-700 px-6 py-3 rounded-xl font-bold hover:bg-gray-200 transition-colors">
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

export default AdminCategories;