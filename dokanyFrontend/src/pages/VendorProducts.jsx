// src/pages/VendorProducts.jsx
import { getImageUrl } from '../utils/imageUtils';
import React, { useState, useEffect } from 'react';
import { Plus, Package, Edit, Trash2, Loader2, ImagePlus } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';
import { jwtDecode } from 'jwt-decode';

const VendorProducts = () => {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]); // لتخزين الأقسام القادمة من الداتا بيز
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [formData, setFormData] = useState({
    name: '',
    price: '',
    quantity: '',
    categoryId: '', // سيتم تخزين الـ ID هنا عند اختيار الاسم
    productionDate: '', 
    expireDate: '',     
    imageFile: null
  });

  // دالة استخراج الـ ID من التوكن
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

  // دالة جلب البيانات (المنتجات + الأقسام)
  // دالة جلب البيانات (المنتجات + الأقسام)
  const fetchData = async () => {
    try {
      setIsLoading(true);
      
      // التعديل هنا: استخدام المسار الصحيح لجلب منتجات البائع
      const productsRes = await axiosInstance.get('/Product/my-products');
      setProducts(Array.isArray(productsRes.data?.data) ? productsRes.data.data : []);

      // جلب الأقسام من الداتا بيز
      const categoriesRes = await axiosInstance.get('/Category/active');
      console.log("🔥 الأقسام المستلمة:", categoriesRes.data.data);
      setCategories(Array.isArray(categoriesRes.data?.data) ? categoriesRes.data.data : []);

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
    setFormData({ ...formData, imageFile: e.target.files[0] });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSaving(true);

    try {
      const vendorId = getVendorId();
      if (!vendorId) {
        alert("انتهت الجلسة، يرجى تسجيل الدخول.");
        setIsSaving(false);
        return;
      }

      if (!formData.categoryId) {
        alert("يرجى اختيار قسم للمنتج.");
        setIsSaving(false);
        return;
      }

      const data = new FormData();
      data.append('Name', formData.name);
      data.append('Price', formData.price);
      data.append('Quantity', formData.quantity);
      data.append('CategoryId', formData.categoryId);
      data.append('VendorId', vendorId);
      data.append('ImageFile', formData.imageFile);
      
      if (formData.productionDate) data.append('ProductionDate', formData.productionDate);
      if (formData.expireDate) data.append('ExpireDate', formData.expireDate);

      await axiosInstance.post('/Product', data);

      alert("تم إضافة المنتج بنجاح!");
      setIsModalOpen(false);
      fetchData(); 
      setFormData({ name: '', price: '', quantity: '', categoryId: '', productionDate: '', expireDate: '', imageFile: null });
      
    } catch (error) {
      const backendError = error.response?.data;
      let errorMsg = backendError?.message || "فشلت عملية الإضافة";
      if (backendError?.errors) errorMsg = Object.values(backendError.errors).flat().join('\n');
      alert(errorMsg);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="p-6" dir="rtl">
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-2xl font-black text-gray-800 flex items-center gap-2">
          <Package className="text-dokany" /> منتجاتي
        </h1>
        <button 
          onClick={() => setIsModalOpen(true)}
          className="bg-dokany text-white px-6 py-3 rounded-2xl font-bold flex items-center gap-2 hover:bg-black transition-all shadow-lg shadow-dokany/20"
        >
          <Plus size={20} /> إضافة منتج جديد
        </button>
      </div>

      {isLoading ? (
        <div className="flex justify-center p-20"><Loader2 className="animate-spin text-dokany" size={40} /></div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {products.map((product) => (
            <div key={product.id} className="bg-white rounded-3xl p-4 border border-gray-100 shadow-sm hover:shadow-md transition-all">
            <img 
              src={product.imgUrl ? `http://localhost:44342${product.imgUrl}` : 
                  product.imageUrl ? `http://localhost:44342${product.imageUrl}` :
                  'https://placehold.co/400x400?text=No+Image'} 
              alt={product.name} 
            />
              <h3 className="font-bold text-gray-800 mb-1">{product.name}</h3>
              <p className="text-dokany font-black mb-4">{product.price} ج.م</p>
              <div className="flex gap-2">
                <button className="flex-1 bg-gray-50 text-gray-600 p-2 rounded-xl hover:bg-blue-50 hover:text-blue-600 transition-all"><Edit size={18} className="mx-auto"/></button>
                <button className="flex-1 bg-gray-50 text-gray-600 p-2 rounded-xl hover:bg-red-50 hover:text-red-600 transition-all"><Trash2 size={18} className="mx-auto"/></button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* نافذة الإضافة المحدثة بالقائمة المنسدلة */}
      {isModalOpen && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 overflow-y-auto">
          <div className="bg-white rounded-[2.5rem] w-full max-w-lg p-8 animate-fade-in-up my-8">
            <h2 className="text-xl font-black mb-6">إضافة منتج جديد</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              
              <div>
                <label className="block text-sm font-bold mb-2">اسم المنتج</label>
                <input required type="text" className="w-full p-4 bg-gray-50 rounded-2xl border-none focus:ring-2 focus:ring-dokany" 
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})} />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-bold mb-2">السعر</label>
                  <input required type="number" step="0.01" className="w-full p-4 bg-gray-50 rounded-2xl border-none focus:ring-2 focus:ring-dokany" 
                    value={formData.price}
                    onChange={(e) => setFormData({...formData, price: e.target.value})} />
                </div>
                <div>
                  <label className="block text-sm font-bold mb-2">الكمية</label>
                  <input required type="number" className="w-full p-4 bg-gray-50 rounded-2xl border-none focus:ring-2 focus:ring-dokany" 
                    value={formData.quantity}
                    onChange={(e) => setFormData({...formData, quantity: e.target.value})} />
                </div>
              </div>

              {/* القائمة المنسدلة لجلب الأقسام بالاسم */}
              <div>
                <label className="block text-sm font-bold mb-2">القسم</label>
                <select 
                  required 
                  className="w-full p-4 bg-gray-50 rounded-2xl border-none focus:ring-2 focus:ring-dokany appearance-none cursor-pointer"
                  value={formData.categoryId}
                  onChange={(e) => setFormData({...formData, categoryId: e.target.value})}
                >
                  <option value="" disabled>-- اختر القسم المناسب --</option>
                  {categories.map((cat) => (
                    <option key={cat.id} value={cat.id}>{cat.name}</option>
                  ))}
                </select>
                {categories.length === 0 && <p className="text-xs text-amber-600 mt-1 mr-2">جاري تحميل الأقسام من السيرفر...</p>}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-bold mb-2 text-gray-600 text-xs">تاريخ الإنتاج (اختياري)</label>
                  <input type="date" className="w-full p-4 bg-gray-50 rounded-2xl border-none focus:ring-2 focus:ring-dokany text-sm" 
                    value={formData.productionDate}
                    onChange={(e) => setFormData({...formData, productionDate: e.target.value})} />
                </div>
                <div>
                  <label className="block text-sm font-bold mb-2 text-gray-600 text-xs">تاريخ الانتهاء (اختياري)</label>
                  <input type="date" className="w-full p-4 bg-gray-50 rounded-2xl border-none focus:ring-2 focus:ring-dokany text-sm" 
                    value={formData.expireDate}
                    onChange={(e) => setFormData({...formData, expireDate: e.target.value})} />
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold mb-2">صورة المنتج</label>
                <div className="relative h-32 w-full bg-gray-50 rounded-2xl border-2 border-dashed border-gray-200 flex items-center justify-center overflow-hidden">
                  {formData.imageFile ? (
                    <img src={getImageUrl(product.imgUrl, 'Products')} alt={product.name} />
                  ) : (
                    <div className="text-center text-gray-400">
                      <ImagePlus className="mx-auto mb-1" />
                      <span className="text-xs">اضغط لرفع صورة</span>
                    </div>
                  )}
                  <input type="file" accept="image/*" onChange={handleFileChange} className="absolute inset-0 opacity-0 cursor-pointer" />
                </div>
              </div>

              <div className="flex gap-4 mt-8 pt-4">
                <button type="submit" disabled={isSaving} className="flex-1 bg-dokany text-white py-4 rounded-2xl font-bold hover:bg-black transition-all disabled:bg-gray-400 flex justify-center items-center gap-2">
                  {isSaving ? <Loader2 className="animate-spin" size={20}/> : "حفظ المنتج"}
                </button>
                <button type="button" onClick={() => setIsModalOpen(false)} className="flex-1 bg-gray-100 text-gray-600 py-4 rounded-2xl font-bold hover:bg-gray-200 transition-all">إلغاء</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default VendorProducts;