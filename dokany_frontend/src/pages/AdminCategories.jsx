// src/pages/AdminCategories.jsx
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
  
  // تحديث حالة الفورم لتدعم رفع الملفات بدلاً من الـ URL النصي [cite: 18, 30]
  const [formData, setFormData] = useState({
    id: '',
    name: '',
    isActive: true,
    imageFile: null,    // لتخزين ملف الصورة الحقيقي [cite: 16]
    imagePreview: ''    // لعرض معاينة للصورة في الواجهة
  });

  const fileInputRef = useRef(null);

  // 1. جلب الأقسام من الباك إند
  const fetchCategories = async () => {
    try {
      setIsLoading(true);
      const response = await axiosInstance.get('/Category/admin/all');
      setCategories(response.data.data || []);
    } catch (error) {
      console.error("Error fetching categories:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchCategories();
  }, []);

  // دالة التعامل مع اختيار الصورة من الجهاز [cite: 34]
  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setFormData({
        ...formData,
        imageFile: file,
        imagePreview: URL.createObjectURL(file) // إنشاء رابط مؤقت للمعاينة
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
      // عرض الصورة الحالية من السيرفر كمعاينة [cite: 43]
      imagePreview: category.imageUrl ? `http://localhost:44342/Images/Categories/${category.imageUrl}` : ''
    });
    setIsModalOpen(true);
  };

  // 2. دالة الإضافة والتعديل الفعلي باستخدام FormData [cite: 18, 19]
  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    
    try {
      // تجهيز البيانات كـ multipart/form-data [cite: 15, 20]
      const submitData = new FormData();
      submitData.append("name", formData.name); // [cite: 21]
      
      // إضافة ملف الصورة إذا تم اختياره [cite: 22]
      if (formData.imageFile) {
        submitData.append("imageFile", formData.imageFile);
      }
      
      if (modalMode !== 'add') {
          submitData.append("isActive", formData.isActive);
      }

      if (modalMode === 'add') {
         // إرسال الطلب للباك إند [cite: 12, 23]
         await axiosInstance.post('/Category', submitData);
         alert("تم إضافة القسم بنجاح.");
      } else {
         await axiosInstance.put(`/Category/${formData.id}`, submitData);
         alert("تم تعديل القسم بنجاح.");
      }
      fetchCategories(); // تحديث الجدول بعد النجاح
      setIsModalOpen(false);
    } catch (error) {
      console.error("Error saving category:", error);
      alert(error.response?.data?.message || "حدث خطأ أثناء حفظ القسم.");
    } finally {
      setIsSubmitting(false);
    }
  };

  // 3. دالة الإيقاف المؤقت (Soft Delete) 
  const handleToggleActive = async (id, currentStatus) => {
    const actionName = currentStatus ? 'إيقاف' : 'تفعيل';
    if (window.confirm(`هل أنت متأكد من ${actionName} هذا القسم؟`)) {
        try {
            const categoryToUpdate = categories.find(c => c.id === id);
            
            // نستخدم FormData هنا أيضاً لتوحيد طريقة الإرسال
            const submitData = new FormData();
            submitData.append("name", categoryToUpdate.name);
            submitData.append("isActive", !currentStatus);
            // لا نرسل صورة جديدة هنا، ليحتفظ بالصورة القديمة

            await axiosInstance.put(`/Category/${id}`, submitData);
            fetchCategories();
        } catch (error) {
            alert(`حدث خطأ أثناء ${actionName} القسم.`);
        }
    }
  };

  // 4. دالة الحذف النهائي (Hard Delete) 
  const handleHardDelete = async (id) => {
    if (window.confirm('هل أنت متأكد من حذف هذا القسم نهائياً؟ هذا الإجراء لا يمكن التراجع عنه وسيحذف جميع المنتجات المرتبطة به.')) {
      try {
        await axiosInstance.delete(`/Category/${id}/hard`);
        alert("تم حذف القسم نهائياً.");
        fetchCategories();
      } catch (error) {
        console.error("Error hard deleting category:", error);
        alert("حدث خطأ أثناء الحذف.");
      }
    }
  };

  return (
    <div className="animate-fade-in-down relative" dir="rtl">
      
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-800 mb-2 flex items-center gap-2">
            <FolderTree className="text-dokany" size={28} />
            إدارة الأقسام
          </h1>
          <p className="text-gray-500 text-sm">أضف أقساماً جديدة وتحكم في ظهورها للعملاء.</p>
        </div>
        <button onClick={handleOpenAdd} className="bg-dokany text-white px-6 py-3 rounded-xl font-bold hover:bg-dokany-dark transition-colors flex items-center gap-2 shadow-lg shadow-emerald-500/30">
          <Plus size={20} /> قسم جديد
        </button>
      </div>

      {/* جدول الأقسام */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-x-auto">
        {isLoading ? (
            <div className="flex justify-center p-10 text-dokany"><Loader2 className="animate-spin" size={32} /></div>
        ) : (
            <table className="w-full text-right whitespace-nowrap">
            <thead className="bg-gray-50 text-gray-600 font-bold border-b border-gray-100">
                <tr>
                <th className="p-4">القسم</th>
                <th className="p-4">الحالة</th>
                <th className="p-4 text-center">عدد المنتجات</th>
                <th className="p-4 text-center">الإجراءات</th>
                </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
                {categories.length > 0 ? categories.map((category) => (
                <tr key={category.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="p-4">
                    <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-gray-50 border border-gray-100 rounded-lg flex items-center justify-center p-1 shrink-0">
                        {/* عرض الصورة بالمسار الجديد بناءً على تقرير الباك إند [cite: 43] */}
                        <img 
                          src={getImageUrl(category.imageUrl, 'Categories')} 
                          alt={category.name} 
                          className="max-h-full mix-blend-multiply object-contain rounded" 
                          onError={(e) => e.target.src = "https://placehold.co/100x100?text=Error"} 
                        />
                        </div>
                        <div>
                        <p className="font-bold text-gray-800">{category.name}</p>
                        <p className="text-xs text-gray-400 mt-1">تاريخ الإضافة: {category.createdAt ? new Date(category.createdAt).toLocaleDateString('ar-EG') : 'غير متوفر'}</p>
                        </div>
                    </div>
                    </td>
                    <td className="p-4">
                    {category.isActive ? (
                        <span className="bg-emerald-100 text-emerald-600 text-xs px-3 py-1.5 rounded-full font-bold">نشط</span>
                    ) : (
                        <span className="bg-gray-100 text-gray-500 text-xs px-3 py-1.5 rounded-full font-bold">معطل (Soft Deleted)</span>
                    )}
                    </td>
                    <td className="p-4 text-center">
                    <span className="font-bold text-gray-800 bg-gray-50 px-3 py-1 rounded-lg border border-gray-200">
                        {category.productCount || 0} منتج
                    </span>
                    </td>
                    <td className="p-4">
                    <div className="flex items-center justify-center gap-2">
                        <button onClick={() => handleOpenEdit(category)} className="p-2 text-blue-500 hover:bg-blue-50 rounded-lg transition-colors" title="تعديل">
                        <Edit size={18} />
                        </button>
                        <button onClick={() => handleToggleActive(category.id, category.isActive)} className={`p-2 rounded-lg transition-colors ${category.isActive ? 'text-amber-500 hover:bg-amber-50' : 'text-emerald-500 hover:bg-emerald-50'}`} title={category.isActive ? 'إيقاف مؤقت' : 'تفعيل'}>
                        <PowerOff size={18} />
                        </button>
                        <button onClick={() => handleHardDelete(category.id)} className="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors" title="حذف نهائي">
                        <Trash2 size={18} />
                        </button>
                    </div>
                    </td>
                </tr>
                )) : (
                    <tr><td colSpan="4" className="p-8 text-center text-gray-500">لا توجد أقسام حالياً.</td></tr>
                )}
            </tbody>
            </table>
        )}
      </div>

      {/* نافذة الإضافة والتعديل */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-3xl w-full max-w-md p-8 shadow-2xl animate-fade-in-up">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-xl font-bold text-gray-800">
                {modalMode === 'add' ? 'إضافة قسم جديد' : 'تعديل بيانات القسم'}
              </h2>
              <button onClick={() => setIsModalOpen(false)} className="text-gray-400 hover:text-red-500"><X size={24} /></button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6">
              
              {/* قسم رفع صورة القسم من الجهاز بتصميم أنيق */}
              <div className="flex flex-col items-center justify-center w-full">
                <input 
                  type="file" 
                  accept="image/*" 
                  ref={fileInputRef}
                  onChange={handleImageChange} 
                  className="hidden" 
                  required={modalMode === 'add'} // إجباري عند الإضافة فقط
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
                      <p className="font-bold text-gray-600 text-sm">اضغط هنا لرفع صورة القسم</p>
                      <p className="text-xs text-gray-400 mt-1">JPG, PNG (الحد الأقصى 2MB)</p>
                    </>
                  )}
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">اسم القسم (2-100 حرف)</label>
                <input 
                  type="text" 
                  required 
                  minLength="2" 
                  maxLength="100" 
                  value={formData.name} 
                  onChange={(e) => setFormData({...formData, name: e.target.value})} 
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl px-4 py-3 focus:ring-2 focus:ring-dokany outline-none" 
                  placeholder="مثال: أجهزة منزلية" 
                />
              </div>

              {modalMode === 'edit' && (
                <div className="flex items-center gap-3 bg-gray-50 p-4 rounded-xl border border-gray-200">
                  <input 
                    type="checkbox" 
                    id="isActive" 
                    checked={formData.isActive} 
                    onChange={(e) => setFormData({...formData, isActive: e.target.checked})} 
                    className="w-5 h-5 accent-dokany rounded cursor-pointer" 
                  />
                  <label htmlFor="isActive" className="font-bold text-gray-700 cursor-pointer select-none">
                    حالة القسم (مرئي للعملاء)
                  </label>
                </div>
              )}

              <div className="pt-4 flex gap-4">
                <button type="submit" disabled={isSubmitting} className="flex-1 bg-dokany text-white py-3 rounded-xl font-bold hover:bg-dokany-dark transition-colors flex justify-center items-center">
                  {isSubmitting ? <Loader2 className="animate-spin" size={20} /> : (modalMode === 'add' ? 'إضافة القسم' : 'حفظ التعديلات')}
                </button>
                <button type="button" onClick={() => setIsModalOpen(false)} className="bg-gray-100 text-gray-700 px-6 py-3 rounded-xl font-bold hover:bg-gray-200 transition-colors">
                  إلغاء
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