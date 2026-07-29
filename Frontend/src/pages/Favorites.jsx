import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import ProductCard from '../components/ProductCard';
import { Link } from 'react-router-dom';
import { Heart, ArrowLeft, Loader2 } from 'lucide-react';
import { favoriteApi } from '../api';

const Favorites = () => {
  const dispatch = useDispatch();
  const reduxFavorites = useSelector((state) => state.favorite.favoriteItems);
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);

  const [backendFavorites, setBackendFavorites] = useState([]);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const fetchBackendFavorites = async () => {
      if (!isAuthenticated) return;
      try {
        setIsLoading(true);
        const res = await favoriteApi.getFavorites();
        const items = Array.isArray(res.data?.data) ? res.data.data : (res.data?.data?.items || []);
        setBackendFavorites(items);
      } catch (err) {
        console.error("Error fetching favorites:", err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchBackendFavorites();
  }, [isAuthenticated]);

  const displayItems = isAuthenticated && backendFavorites.length > 0 ? backendFavorites : reduxFavorites;

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[50vh] text-emerald-600">
        <Loader2 className="animate-spin mb-3" size={40} />
        <p className="font-bold text-gray-600">Loading favorites...</p>
      </div>
    );
  }

  if (displayItems.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-20 w-full flex flex-col items-center justify-center text-center animate-fade-in-down" dir="ltr">
        <div className="bg-red-50 w-32 h-32 rounded-full flex items-center justify-center mb-6 text-red-400">
          <Heart size={64} />
        </div>
        <h2 className="text-3xl font-black text-gray-800 mb-4">Your Favorites List is Empty</h2>
        <p className="text-gray-500 mb-8">You haven't added any products to your favorites list yet.</p>
        <Link to="/" className="bg-emerald-600 text-white px-8 py-3 rounded-xl font-bold hover:bg-emerald-700 transition-colors flex items-center gap-2">
          <ArrowLeft size={20} />
          Back to Shopping
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 w-full animate-fade-in-down" dir="ltr">
      <div className="flex items-center gap-3 mb-8">
        <Heart className="text-red-500" size={28} />
        <h1 className="text-2xl font-bold text-gray-800">My Favorites ({displayItems.length})</h1>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
        {displayItems.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </div>
  );
};

export default Favorites;