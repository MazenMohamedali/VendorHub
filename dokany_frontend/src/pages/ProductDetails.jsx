// src/pages/ProductDetails.jsx
import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { ShoppingCart, Heart, ArrowRight, ShieldCheck, Truck, RotateCcw, Loader2, XCircle } from 'lucide-react'; // ✅ أضفت XCircle
import { addToCart } from '../store/cartSlice';
import { toggleFavorite } from '../store/favoriteSlice';
import axiosInstance from '../api/axiosConfig';

const ProductDetails = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const [product, setProduct] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [quantity, setQuantity] = useState(1);

  // 1. ✅ جلب بيانات السلة والمفضلات
  const favoriteItems = useSelector((state) => state.favorite.favoriteItems);
  const cartItems = useSelector((state) => state.cart.cartItems); // ✅ مضاف
  
  const isFavorite = favoriteItems.some((item) => item.id === parseInt(id));

  // 2. ✅ حساب الكمية الموجودة في السلة لهذا المنتج تحديداً
  const itemInCart = cartItems.find(item => Number(item.id) === Number(id));
  const currentCartQty = itemInCart ? itemInCart.cartQuantity : 0;

  useEffect(() => {
    const fetchProductDetails = async () => {
      try {
        setIsLoading(true);
        const response = await axiosInstance.get(`/Product/${id}/customer`);
        const productData = response.data.data;

        const formattedProduct = {
          id: productData.id,
          title: productData.name,
          price: productData.price,
          description: productData.description || 'لا يوجد وصف متاح لهذا المنتج.',
          category: productData.categoryName || 'غير محدد',
          vendorName: productData.storeName || 'بائع معتمد',
          stockQuantity: productData.quantity || productData.unitsInStock || 0,
          imageUrl: productData.imgUrl || "https://placehold.co/600x600?text=No+Image",
          images: productData.imgUrl ? [productData.imgUrl] : ["https://placehold.co/600x600?text=No+Image"],
        };

        setProduct(formattedProduct);
      } catch (err) {
        console.error("Error fetching product details:", err);
        setError('لم يتم العثور على المنتج أو حدث خطأ في الاتصال.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchProductDetails();
    window.scrollTo(0, 0);
  }, [id]);

  const handleAddToCart = () => {
    if (product) {
      dispatch(addToCart({ ...product, cartQuantity: quantity }));
    }
  };

  const handleToggleFavorite = () => {
    if (product) {
      dispatch(toggleFavorite(product));
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-[70vh] flex flex-col items-center justify-center text-dokany">
        <Loader2 className="animate-spin mb-4" size={48} />
        <h2 className="text-2xl font-bold">جاري تحميل تفاصيل المنتج...</h2>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="min-h-[70vh] flex flex-col items-center justify-center text-center px-4">
        <div className="bg-red-50 text-red-500 w-24 h-24 rounded-full flex items-center justify-center mb-6">
          <ShieldCheck size={48} />
        </div>
        <h2 className="text-3xl font-black text-gray-800 mb-4">عذراً، المنتج غير متوفر</h2>
        <p className="text-gray-500 mb-8">{error}</p>
        <button onClick={() => navigate('/')} className="bg-dokany text-white px-8 py-3 rounded-xl font-bold flex items-center gap-2 hover:bg-dokany-dark">
          <ArrowRight size={20} /> العودة للرئيسية
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 animate-fade-in-down" dir="rtl">
      
      {/* Breadcrumb ... (يبقى كما هو) */}
      <div className="flex items-center gap-2 text-sm text-gray-500 mb-8">
        <Link to="/" className="hover:text-dokany transition-colors">الرئيسية</Link>
        <span>/</span>
        <span className="hover:text-dokany transition-colors cursor-pointer">{product.category}</span>
        <span>/</span>
        <span className="text-gray-800 font-bold truncate">{product.title}</span>
      </div>

      <div className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="flex flex-col md:flex-row">
          
          {/* قسم الصورة ... (يبقى كما هو) */}
          <div className="w-full md:w-1/2 bg-gray-50 p-8 flex justify-center items-center relative group">
            <button 
              onClick={handleToggleFavorite}
              className={`absolute top-6 right-6 w-12 h-12 rounded-full flex items-center justify-center transition-all shadow-md z-10 ${isFavorite ? 'bg-red-50 text-red-500' : 'bg-white text-gray-400 hover:text-red-500'}`}
            >
              <Heart size={24} fill={isFavorite ? "currentColor" : "none"} />
            </button>
            <img 
              src={product.imageUrl} 
              alt={product.title} 
              className="max-h-[500px] object-contain mix-blend-multiply transition-transform duration-500 group-hover:scale-105" 
            />
          </div>

          <div className="w-full md:w-1/2 p-8 lg:p-12 flex flex-col justify-center">
            <div className="mb-2">
              <span className="bg-emerald-100 text-emerald-700 text-xs px-3 py-1.5 rounded-full font-bold">
                {product.category}
              </span>
            </div>
            
            <h1 className="text-3xl lg:text-4xl font-black text-gray-800 mb-4 leading-tight">
              {product.title}
            </h1>
            
            <p className="text-sm text-gray-500 mb-6 font-medium">
              البائع: <span className="text-dokany font-bold">{product.vendorName}</span>
            </p>

            <div className="text-4xl font-black text-dokany mb-6">
              {product.price} <span className="text-xl">ج.م</span>
            </div>

            <p className="text-gray-600 leading-relaxed mb-8">
              {product.description}
            </p>

            <div className="mb-8">
              <p className="text-sm font-bold mb-2 text-gray-700">حالة التوفر:</p>
              {product.stockQuantity > 0 ? (
                <span className="text-emerald-600 font-bold flex items-center gap-2">
                  <ShieldCheck size={20} /> متوفر في المخزون ({product.stockQuantity} قطعة)
                </span>
              ) : (
                <span className="text-red-500 font-bold flex items-center gap-2">
                  <XCircle size={20} /> نفدت الكمية
                </span>
              )}
            </div>

            <div className="flex flex-col sm:flex-row gap-4 mb-10">
              <div className="flex items-center justify-between bg-gray-50 border border-gray-200 rounded-xl p-2 w-full sm:w-32 shrink-0">
                <button 
                  onClick={() => setQuantity(prev => prev + 1)}
                  // ✅ تعديل: تعطيل زر الزيادة إذا كانت الكمية المختارة + الموجودة في السلة تساوي المخزون
                  disabled={quantity + currentCartQty >= product.stockQuantity}
                  className="w-10 h-10 flex items-center justify-center bg-white rounded-lg shadow-sm hover:text-dokany disabled:opacity-50"
                >
                  +
                </button>
                <span className="font-bold text-gray-800">{quantity}</span>
                <button 
                  onClick={() => setQuantity(prev => prev > 1 ? prev - 1 : 1)}
                  className="w-10 h-10 flex items-center justify-center bg-white rounded-lg shadow-sm hover:text-red-500"
                >
                  -
                </button>
              </div>

              {/* 3. ✅ تعديل الزر النهائي */}
              <button 
                onClick={handleAddToCart}
                // ✅ تعطيل الزر إذا كان المنتج غير متوفر أو الكمية في السلة وصلت للحد الأقصى
                disabled={product.stockQuantity === 0 || currentCartQty >= product.stockQuantity}
                className="flex-1 bg-dokany text-white py-4 rounded-xl font-bold text-lg hover:bg-dokany-dark transition-all shadow-lg shadow-emerald-500/30 flex items-center justify-center gap-3 disabled:opacity-50 disabled:cursor-not-allowed disabled:shadow-none"
              >
                <ShoppingCart size={24} />
                {product.stockQuantity === 0 
                  ? 'نفدت الكمية' 
                  : currentCartQty >= product.stockQuantity 
                    ? 'وصلت للحد الأقصى بالسلة' 
                    : 'إضافة إلى السلة'}
              </button>
            </div>

            {/* مميزات إضافية ... (تبقى كما هي) */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-8 border-t border-gray-100">
               {/* ... */}
               <div className="flex items-center gap-3 text-gray-600">
                <div className="w-10 h-10 bg-emerald-50 text-dokany flex items-center justify-center rounded-lg">
                  <Truck size={20} />
                </div>
                <div className="text-sm">
                  <p className="font-bold text-gray-800">توصيل سريع</p>
                  <p className="text-xs">لجميع المحافظات</p>
                </div>
              </div>
              <div className="flex items-center gap-3 text-gray-600">
                <div className="w-10 h-10 bg-blue-50 text-blue-500 flex items-center justify-center rounded-lg">
                  <RotateCcw size={20} />
                </div>
                <div className="text-sm">
                  <p className="font-bold text-gray-800">استرجاع مجاني</p>
                  <p className="text-xs">خلال 14 يوم</p>
                </div>
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductDetails;