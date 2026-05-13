// src/pages/Home.jsx
import React, { useState, useEffect } from 'react';
import ProductCard from '../components/ProductCard';
import { ArrowLeft, Filter, DollarSign, Loader2 } from 'lucide-react';
import axiosInstance from '../api/axiosConfig'; // استدعاء محطة الاتصال بالباك إند

const Home = () => {
  // حالة المنتجات الحقيقية القادمة من الباك إند
  const [products, setProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  // حالات الفلترة (الأقسام والسعر)
  const [selectedCategory, setSelectedCategory] = useState('الكل');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [categories, setCategories] = useState(['الكل']);

  // جلب المنتجات من الباك إند عند فتح الصفحة
  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setIsLoading(true);
        // طلب المنتجات من مسار الـ API المخصص للعملاء
        const response = await axiosInstance.get('/Product/list');
        
        // البيانات تأتي داخل response.data.data حسب هيكلة الـ GeneralResponse
        const fetchedProducts = response.data.data || [];
        
        // تعديل روابط الصور لتكون كاملة (بناءً على ملاحظات الباك إند)
        const productsWithFullImages = fetchedProducts.map(product => ({
          ...product,
          // إذا كانت الصورة موجودة، نضع رابط السيرفر قبلها، وإلا نضع صورة افتراضية
          imgUrl: product.imgUrl || "https://placehold.co/600x400?text=No+Image"
        }));

        setProducts(productsWithFullImages);

        // استخراج الأقسام الفريدة من المنتجات القادمة لإضافتها في الفلتر
        const uniqueCategories = ['الكل', ...new Set(productsWithFullImages.map(p => p.categoryName || 'أخرى'))];
        setCategories(uniqueCategories);
        
      } catch (err) {
        console.error("Error fetching products:", err);
        setError('حدث خطأ أثناء جلب المنتجات. تأكد من تشغيل السيرفر.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchProducts();
  }, []); // مصفوفة فارغة تعني التنفيذ مرة واحدة عند فتح الصفحة

  // دالة الفلترة الديناميكية (بالقسم والسعر) على المنتجات الحقيقية
  const filteredProducts = products.filter((product) => {
    const matchCategory = selectedCategory === 'الكل' || product.categoryName === selectedCategory;
    const min = minPrice !== '' ? parseFloat(minPrice) : 0;
    const max = maxPrice !== '' ? parseFloat(maxPrice) : Infinity;
    const matchPrice = product.price >= min && product.price <= max;

    return matchCategory && matchPrice;
  });

  const clearPriceFilter = () => {
    setMinPrice('');
    setMaxPrice('');
  };

  // عرض حالة التحميل
  if (isLoading) {
    return (
      <div className="min-h-[60vh] flex flex-col items-center justify-center text-dokany">
        <Loader2 size={48} className="animate-spin mb-4" />
        <h2 className="text-xl font-bold">جاري تحميل المنتجات...</h2>
      </div>
    );
  }

  // عرض حالة الخطأ
  if (error) {
    return (
      <div className="min-h-[60vh] flex flex-col items-center justify-center text-red-500">
        <h2 className="text-xl font-bold mb-2">عذراً!</h2>
        <p>{error}</p>
      </div>
    );
  }

  return (
    <div className="w-full animate-fade-in-down" dir="rtl">
      
      {/* قسم البانر الإعلاني */}
      <div className="bg-dokany-light/30 border-b border-emerald-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 md:py-20 flex flex-col md:flex-row items-center justify-between gap-10">
          <div className="flex-1 animate-fade-in-up">
            <span className="bg-emerald-100 text-dokany px-4 py-1.5 rounded-full text-sm font-bold mb-6 inline-block">
              أفضل العروض لصيف 2026
            </span>
            <h1 className="text-4xl md:text-6xl font-black text-gray-800 mb-6 leading-tight">
              تسوق أحدث المنتجات <br />
              <span className="text-dokany">بأفضل الأسعار</span>
            </h1>
            <p className="text-gray-600 text-lg mb-8 max-w-lg leading-relaxed">
              اكتشف آلاف المنتجات من أفضل البائعين المعتمدين. جودة مضمونة، توصيل سريع، وتجربة تسوق لا تُنسى.
            </p>
            <button className="bg-dokany text-white px-8 py-4 rounded-xl font-bold text-lg hover:bg-dokany-dark transition-all duration-300 flex items-center gap-3">
              تسوق الآن
              <ArrowLeft size={20} />
            </button>
          </div>
          <div className="flex-1 flex justify-center md:justify-end animate-fade-in-down">
            <div className="w-72 h-72 md:w-96 md:h-96 bg-dokany rounded-full flex items-center justify-center text-white text-3xl font-bold shadow-2xl shadow-emerald-500/20 relative">
              <div className="absolute inset-0 border-4 border-dashed border-white/30 rounded-full animate-spin-slow"></div>
              <span className="text-4xl">🛒</span>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        
        {/* شريط الفلاتر (الأقسام + السعر) */}
        <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6 mb-10 bg-white p-4 rounded-2xl shadow-sm border border-gray-100">
          
          <div className="flex items-center gap-3 overflow-x-auto w-full lg:w-auto pb-2 lg:pb-0 hide-scrollbar">
            <Filter size={20} className="text-gray-400 shrink-0" />
            {categories.map((category, index) => (
              <button
                key={index}
                onClick={() => setSelectedCategory(category)}
                className={`whitespace-nowrap px-5 py-2 rounded-full font-bold text-sm transition-all ${
                  selectedCategory === category 
                    ? 'bg-dokany text-white shadow-md' 
                    : 'bg-gray-50 text-gray-600 hover:bg-gray-100 border border-gray-200'
                }`}
              >
                {category}
              </button>
            ))}
          </div>

          {/* فلتر السعر */}
          <div className="flex items-center gap-3 w-full lg:w-auto bg-gray-50 p-2 rounded-xl border border-gray-200 shrink-0">
            <DollarSign size={18} className="text-gray-400" />
            <span className="text-sm font-bold text-gray-600 shrink-0">السعر:</span>
            
            <input 
              type="number" 
              placeholder="من" 
              min="0"
              value={minPrice}
              onChange={(e) => setMinPrice(e.target.value)}
              className="w-20 bg-white border border-gray-200 rounded-lg py-1.5 text-center text-sm focus:ring-1 focus:ring-dokany outline-none"
            />
            <span className="text-gray-400">-</span>
            <input 
              type="number" 
              placeholder="إلى" 
              min="0"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value)}
              className="w-20 bg-white border border-gray-200 rounded-lg py-1.5 text-center text-sm focus:ring-1 focus:ring-dokany outline-none"
            />
            
            {(minPrice || maxPrice) && (
              <button 
                onClick={clearPriceFilter}
                className="text-xs text-red-500 hover:underline font-bold px-2 shrink-0"
              >
                مسح
              </button>
            )}
          </div>
        </div>

        {/* عنوان القسم */}
        <div className="mb-8">
          <h2 className="text-3xl font-black text-gray-800 mb-2">المنتجات المتاحة</h2>
          <p className="text-gray-500">تم العثور على ({filteredProducts.length}) منتج يطابق بحثك</p>
        </div>

        {/* شبكة المنتجات */}
        {filteredProducts.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
            {/* نقوم بتمرير بيانات المنتج الحقيقية لمكون ProductCard */}
            {filteredProducts.map((product) => (
              <ProductCard key={product.id} product={{
                id: product.id,
                title: product.name,
                price: product.price,
                images: [product.imgUrl],
                averageStars: product.averageStars || 0,
                viewersNo: product.viewersNo || 0,
                vendorName: product.storeName || 'بائع معتمد',
                category: product.categoryName || 'أخرى'
              }} />
            ))}
          </div>
        ) : (
          <div className="text-center py-20 bg-gray-50 rounded-3xl border border-gray-100">
            <Filter size={48} className="mx-auto text-gray-300 mb-4" />
            <h3 className="text-xl font-bold text-gray-800 mb-2">لا توجد منتجات حالياً</h3>
            <p className="text-gray-500 mb-6">قم بإضافة منتجات من لوحة تحكم البائع أو تأكد من الموافقة عليها من الإدارة.</p>
          </div>
        )}

      </div>
    </div>
  );
};

export default Home;