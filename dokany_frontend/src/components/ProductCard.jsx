// src/components/ProductCard.jsx
import React from 'react';
import { ShoppingCart, Eye, Star, Heart } from 'lucide-react';
import { Link } from 'react-router-dom';

// 1. استدعاء مكتبات Redux
import { useDispatch, useSelector } from 'react-redux';
import { addToCart } from '../store/cartSlice';
import { toggleFavorite } from '../store/favoriteSlice'; // استدعاء دالة المفضلة

const ProductCard = ({ product }) => {
  const isOutOfStock = product.availableUnits === 0;
  
  // 2. تفعيل الـ Dispatch
  const dispatch = useDispatch();

  // 3. قراءة المنتجات المفضلة لمعرفة ما إذا كان هذا المنتج بداخلها
  const favoriteItems = useSelector((state) => state.favorite.favoriteItems);
  const isFavorite = favoriteItems.some((item) => item.id === product.id);

  // 4. دالة إضافة المنتج للسلة
  const handleAddToCart = () => {
    dispatch(addToCart(product));
  };

  // 5. دالة الإضافة للمفضلة
  const handleToggleFavorite = (e) => {
    e.preventDefault(); // لمنع تفعيل رابط صفحة المنتج عند الضغط على القلب
    dispatch(toggleFavorite(product));
  };

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-shadow duration-300 flex flex-col justify-between relative">
      
      {/* زر المفضلة المطلق (Absolute) فوق الصورة */}
      <button 
        onClick={handleToggleFavorite}
        className="absolute top-3 left-3 z-10 p-2 bg-white/80 backdrop-blur rounded-full hover:bg-white text-gray-400 hover:text-red-500 transition-colors shadow-sm"
      >
        <Heart size={18} className={isFavorite ? 'fill-red-500 text-red-500' : ''} />
      </button>

      <div>
        {/* منطقة صورة المنتج */}
        <Link to={`/product/${product.id}`} className="block relative">
          <div className="bg-dokany-light h-48 w-full flex items-center justify-center p-4">
            <img 
              src={product.images[0]} 
              alt={product.title} 
              className="max-h-full object-contain mix-blend-multiply"
            />
            {/* بادج حالة المخزون */}
            {isOutOfStock && (
              <span className="absolute top-3 right-3 bg-red-100 text-red-600 text-xs font-bold px-2 py-1 rounded shadow-sm">
                نفذ من المخزون
              </span>
            )}
          </div>
        </Link>

        {/* تفاصيل المنتج */}
        <div className="p-4">
          <div className="flex justify-between items-start mb-2">
            <p className="text-xs text-gray-500">{product.category}</p>
            <div className="flex items-center text-xs text-gray-500 gap-1">
              <Eye size={14} />
              <span>{product.viewers}</span>
            </div>
          </div>

          <Link to={`/product/${product.id}`}>
            <h3 className="font-bold text-gray-800 text-lg line-clamp-1 mb-1 hover:text-dokany transition-colors">
              {product.title}
            </h3>
          </Link>
          
          <p className="text-xs text-gray-400 mb-3">
            البائع: <span className="font-medium text-gray-600">{product.vendorName}</span>
          </p>

          <div className="flex justify-between items-center mb-4">
            <span className="text-xl font-bold text-dokany">{product.price} ج.م</span>
          </div>
        </div>
      </div>

      {/* زر الإضافة للسلة */}
      <div className="p-4 pt-0">
        <button 
          onClick={handleAddToCart}
          disabled={isOutOfStock}
          className={`w-full flex items-center justify-center gap-2 py-2 rounded-lg font-medium transition-colors duration-200 ${
            isOutOfStock 
              ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
              : 'bg-dokany text-white hover:bg-dokany-dark'
          }`}
        >
          <ShoppingCart size={18} />
          {isOutOfStock ? 'غير متاح حالياً' : 'أضف إلى السلة'}
        </button>
      </div>
    </div>
  );
};

export default ProductCard;