// src/pages/Favorites.jsx
import React from 'react';
import { useSelector } from 'react-redux';
import ProductCard from '../components/ProductCard';
import { Link } from 'react-router-dom';
import { Heart, ArrowRight } from 'lucide-react';

const Favorites = () => {
  const favoriteItems = useSelector((state) => state.favorite.favoriteItems);

  if (favoriteItems.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-20 w-full flex flex-col items-center justify-center text-center animate-fade-in-down">
        <div className="bg-red-50 w-32 h-32 rounded-full flex items-center justify-center mb-6 text-red-400">
          <Heart size={64} />
        </div>
        <h2 className="text-3xl font-black text-gray-800 mb-4">قائمة المفضلة فارغة</h2>
        <p className="text-gray-500 mb-8">لم تقم بإضافة أي منتجات إلى قائمتك المفضلة حتى الآن.</p>
        <Link to="/" className="bg-dokany text-white px-8 py-3 rounded-xl font-bold hover:bg-dokany-dark transition-colors flex items-center gap-2">
          <ArrowRight size={20} />
          العودة للتسوق
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 w-full animate-fade-in-down">
      <div className="flex items-center gap-3 mb-8">
        <Heart className="text-red-500" size={28} />
        <h1 className="text-2xl font-bold text-gray-800">قائمتي المفضلة ({favoriteItems.length})</h1>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
        {favoriteItems.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </div>
  );
};

export default Favorites;