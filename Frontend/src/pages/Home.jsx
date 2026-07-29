import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import ProductCard from '../components/ProductCard';
import { ArrowRight, Filter, DollarSign, Loader2, Flame, ShieldCheck, Truck, RefreshCw, Sparkles, Star, Award, Zap } from 'lucide-react';
import axiosInstance from '../api/axiosConfig';
import { productApi } from '../api';
import { getProductImageUrl } from '../utils/imageUtils';

const Home = () => {
  const [products, setProducts] = useState([]);
  const [hotProducts, setHotProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const [selectedCategory, setSelectedCategory] = useState('All');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [categories, setCategories] = useState(['All']);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        
        // Fetch Public Catalog
        const response = await axiosInstance.get('/Product/list');
        const rawData = response.data?.data;
        const fetchedProducts = Array.isArray(rawData) ? rawData : (rawData?.items || []);
        
        setProducts(fetchedProducts);

        const uniqueCategories = ['All', ...new Set(fetchedProducts.map(p => p.categoryName || 'Other'))];
        setCategories(uniqueCategories);

        // Fetch Hot Trending Products
        try {
          const hotRes = await productApi.getHotProducts(6);
          const hotList = Array.isArray(hotRes.data?.data) ? hotRes.data.data : [];
          setHotProducts(hotList);
        } catch (hotErr) {
          setHotProducts([]);
        }
        
      } catch (err) {
        if (err.response?.status === 404) {
          setProducts([]);
        } else {
          console.error("Error fetching products:", err);
          setError('Error loading products. Please check server connection.');
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  const filteredProducts = products.filter((product) => {
    const matchCategory = selectedCategory === 'All' || product.categoryName === selectedCategory;
    const min = minPrice !== '' ? parseFloat(minPrice) : 0;
    const max = maxPrice !== '' ? parseFloat(maxPrice) : Infinity;
    const matchPrice = product.price >= min && product.price <= max;

    return matchCategory && matchPrice;
  });

  const clearPriceFilter = () => {
    setMinPrice('');
    setMaxPrice('');
  };

  if (isLoading) {
    return (
      <div className="min-h-[70vh] flex flex-col items-center justify-center text-emerald-600">
        <Loader2 size={48} className="animate-spin mb-4" />
        <h2 className="text-xl font-bold text-gray-700">Loading catalog...</h2>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4" dir="ltr">
        <div className="bg-red-50 text-red-500 p-6 rounded-full mb-4">
          <Zap size={40} />
        </div>
        <h2 className="text-2xl font-black text-gray-800 mb-2">Service Unavailable</h2>
        <p className="text-gray-500 max-w-md">{error}</p>
      </div>
    );
  }

  return (
    <div className="w-full animate-fade-in-down font-sans text-gray-800" dir="ltr">
      
      {/* Hero Banner Section */}
      <div className="relative bg-gradient-to-br from-slate-900 via-slate-800 to-emerald-950 text-white overflow-hidden py-16 md:py-24">
        {/* Decorative Ambient Blurs */}
        <div className="absolute top-0 right-0 w-96 h-96 bg-emerald-500/10 rounded-full blur-3xl pointer-events-none"></div>
        <div className="absolute bottom-0 left-0 w-96 h-96 bg-teal-500/10 rounded-full blur-3xl pointer-events-none"></div>

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
          <div className="flex flex-col md:flex-row items-center justify-between gap-12">
            
            {/* Hero Left Text */}
            <div className="flex-1 animate-fade-in-up text-left">
              <div className="inline-flex items-center gap-2 bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 px-4 py-2 rounded-full text-xs sm:text-sm font-bold mb-6 backdrop-blur-md">
                <Sparkles size={16} />
                <span>Verified Multi-Vendor E-Commerce Platform</span>
              </div>
              
              <h1 className="text-4xl sm:text-5xl md:text-6xl font-black tracking-tight leading-tight mb-6">
                Discover Quality <br />
                <span className="bg-gradient-to-r from-emerald-400 to-teal-300 bg-clip-text text-transparent">
                  Products & Verified Vendors
                </span>
              </h1>

              <p className="text-slate-300 text-base sm:text-lg mb-8 max-w-xl leading-relaxed">
                Connect directly with certified vendors, browse curated collections, and experience seamless, secure shopping nationwide.
              </p>

              <div className="flex flex-wrap items-center gap-4">
                <a 
                  href="#catalog"
                  className="bg-gradient-to-r from-emerald-500 to-teal-600 text-white px-8 py-4 rounded-2xl font-black text-base hover:from-emerald-600 hover:to-teal-700 transition-all duration-300 flex items-center gap-3 shadow-lg shadow-emerald-500/30 hover:scale-105"
                >
                  Explore Catalog
                  <ArrowRight size={20} />
                </a>

                <div className="flex items-center gap-3 text-slate-400 text-xs sm:text-sm font-medium px-4 py-2 bg-slate-800/40 rounded-2xl border border-slate-700/50">
                  <ShieldCheck size={18} className="text-emerald-400" />
                  100% Protected Payments
                </div>
              </div>
            </div>

            {/* Hero Right Visual Card */}
            <div className="flex-1 flex justify-center w-full max-w-md relative">
              <div className="w-full bg-gradient-to-b from-slate-800/80 to-slate-900/90 border border-slate-700/60 rounded-3xl p-6 shadow-2xl backdrop-blur-xl relative">
                
                {/* Floating Hot Badge */}
                <div className="absolute -top-4 -right-4 bg-gradient-to-r from-rose-500 to-amber-500 text-white px-4 py-1.5 rounded-full font-black text-xs shadow-lg flex items-center gap-1.5 animate-bounce">
                  <Flame size={16} /> HOT OFFERS
                </div>

                <div className="flex items-center justify-between mb-4 border-b border-slate-700/60 pb-4">
                  <div className="flex items-center gap-2 text-emerald-400 font-bold text-sm">
                    <Award size={18} /> Top Rated Sellers
                  </div>
                  <span className="text-xs text-slate-400 font-medium">Daily Deals</span>
                </div>

                <div className="space-y-3">
                  <div className="flex items-center justify-between bg-slate-800/50 p-3 rounded-2xl border border-slate-700/40">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-emerald-500/20 text-emerald-400 rounded-xl flex items-center justify-center font-bold">🛒</div>
                      <div className="text-left">
                        <p className="text-xs font-bold text-slate-200">Verified Vendors</p>
                        <p className="text-[10px] text-slate-400">Direct Storefronts</p>
                      </div>
                    </div>
                    <span className="text-xs font-black text-emerald-400">500+ Active</span>
                  </div>

                  <div className="flex items-center justify-between bg-slate-800/50 p-3 rounded-2xl border border-slate-700/40">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-teal-500/20 text-teal-400 rounded-xl flex items-center justify-center font-bold">⚡</div>
                      <div className="text-left">
                        <p className="text-xs font-bold text-slate-200">Fast Delivery</p>
                        <p className="text-[10px] text-slate-400">Doorstep Shipping</p>
                      </div>
                    </div>
                    <span className="text-xs font-black text-teal-400">24-48 Hours</span>
                  </div>
                </div>
              </div>
            </div>

          </div>
        </div>
      </div>

      {/* Feature Strip */}
      <div className="bg-white border-b border-gray-100 py-6">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6 text-center">
            <div className="flex items-center justify-center gap-3 p-3">
              <Truck size={24} className="text-emerald-600 shrink-0" />
              <div className="text-left">
                <p className="font-black text-slate-900 text-sm">Fast Shipping</p>
                <p className="text-xs text-slate-400">Countrywide delivery</p>
              </div>
            </div>

            <div className="flex items-center justify-center gap-3 p-3">
              <ShieldCheck size={24} className="text-emerald-600 shrink-0" />
              <div className="text-left">
                <p className="font-black text-slate-900 text-sm">Verified Vendors</p>
                <p className="text-xs text-slate-400">Authentic storefronts</p>
              </div>
            </div>

            <div className="flex items-center justify-center gap-3 p-3">
              <RefreshCw size={24} className="text-emerald-600 shrink-0" />
              <div className="text-left">
                <p className="font-black text-slate-900 text-sm">Easy Returns</p>
                <p className="text-xs text-slate-400">14-day return policy</p>
              </div>
            </div>

            <div className="flex items-center justify-center gap-3 p-3">
              <Award size={24} className="text-emerald-600 shrink-0" />
              <div className="text-left">
                <p className="font-black text-slate-900 text-sm">Quality Products</p>
                <p className="text-xs text-slate-400">Admin vetted catalog</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Trending & Hot Products Showcase */}
      {hotProducts.length > 0 && (
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pt-12 pb-6">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-rose-50 text-rose-500 rounded-xl">
                <Flame size={24} />
              </div>
              <div>
                <h2 className="text-2xl font-black text-slate-900 tracking-tight">Hot & Trending Deals</h2>
                <p className="text-xs text-slate-400 font-medium">Most viewed & top customer favorites</p>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {hotProducts.map((hot) => (
              <Link 
                key={hot.id} 
                to={`/product/${hot.id}`}
                className="bg-white rounded-3xl p-4 border border-rose-100 shadow-sm hover:shadow-md transition-all flex items-center gap-4 relative overflow-hidden group cursor-pointer"
              >
                <div className="absolute top-2 left-2 bg-rose-500 text-white text-[10px] font-black px-2 py-0.5 rounded-full z-10 shadow-sm">
                  TRENDING
                </div>
                <img 
                  src={getProductImageUrl(hot.imgUrl)} 
                  alt={hot.name} 
                  className="w-24 h-24 object-contain mix-blend-multiply bg-slate-50 p-2 rounded-2xl shrink-0 group-hover:scale-105 transition-transform" 
                  onError={(e) => { e.target.src = 'https://placehold.co/200x200?text=Hot'; }}
                />
                <div className="flex-1 min-w-0">
                  <h3 className="font-bold text-slate-900 text-sm truncate mb-1 group-hover:text-emerald-600 transition-colors">{hot.name}</h3>
                  <div className="flex items-center gap-2 text-xs text-slate-400 mb-2">
                    <span className="flex items-center text-amber-500 font-bold gap-0.5">
                      <Star size={12} fill="currentColor" /> {hot.averageStars?.toFixed(1) || '5.0'}
                    </span>
                    <span>•</span>
                    <span>{hot.viewersNo || 0} views</span>
                  </div>
                  <p className="text-base font-black text-emerald-600">{hot.price} EGP</p>
                </div>
              </Link>
            ))}
          </div>
        </div>
      )}

      {/* Main Catalog & Filters Section */}
      <div id="catalog" className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        
        {/* Filters Bar */}
        <div className="bg-white p-6 rounded-3xl shadow-sm border border-slate-100 mb-10 space-y-6">
          
          <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
            <div>
              <h2 className="text-2xl font-black text-slate-900 flex items-center gap-2">
                <Filter className="text-emerald-600" size={24} /> Explore All Products
              </h2>
              <p className="text-xs text-slate-400 mt-0.5">Filter by category or price range to find your preferred products</p>
            </div>

            <span className="bg-emerald-50 text-emerald-700 font-bold text-xs px-4 py-2 rounded-full border border-emerald-100">
              Showing {filteredProducts.length} Items
            </span>
          </div>

          {/* Category Chips */}
          <div className="flex items-center gap-2 overflow-x-auto pb-2 custom-scrollbar">
            {categories.map((category, index) => (
              <button
                key={index}
                onClick={() => setSelectedCategory(category)}
                className={`whitespace-nowrap px-5 py-2.5 rounded-2xl font-bold text-xs sm:text-sm transition-all cursor-pointer ${
                  selectedCategory === category 
                    ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-600/30' 
                    : 'bg-slate-50 text-slate-600 hover:bg-slate-100 border border-slate-200/80'
                }`}
              >
                {category}
              </button>
            ))}
          </div>

          {/* Price Range Filter Inputs */}
          <div className="flex flex-wrap items-center gap-3 pt-4 border-t border-slate-100">
            <div className="flex items-center gap-2 bg-slate-50 px-4 py-2 rounded-2xl border border-slate-200/80 text-xs font-bold text-slate-600">
              <DollarSign size={16} className="text-emerald-600" /> Price Filter:
            </div>
            
            <input 
              type="number" 
              placeholder="Min EGP" 
              min="0"
              value={minPrice}
              onChange={(e) => setMinPrice(e.target.value)}
              className="w-28 bg-slate-50 border border-slate-200 rounded-xl py-2 px-3 text-center text-xs font-bold focus:ring-2 focus:ring-emerald-500 outline-none"
            />
            <span className="text-slate-400 font-bold">-</span>
            <input 
              type="number" 
              placeholder="Max EGP" 
              min="0"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value)}
              className="w-28 bg-slate-50 border border-slate-200 rounded-xl py-2 px-3 text-center text-xs font-bold focus:ring-2 focus:ring-emerald-500 outline-none"
            />
            
            {(minPrice || maxPrice) && (
              <button 
                onClick={clearPriceFilter}
                className="text-xs text-rose-600 bg-rose-50 hover:bg-rose-100 px-3 py-2 rounded-xl font-bold transition-all cursor-pointer"
              >
                Reset Price
              </button>
            )}
          </div>
        </div>

        {/* Products Grid */}
        {filteredProducts.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
            {filteredProducts.map((product) => (
              <ProductCard key={product.id} product={{
                id: product.id,
                title: product.name,
                price: product.price,
                imgUrl: product.imgUrl,
                images: [getProductImageUrl(product.imgUrl)],
                averageStars: product.averageStars || 0,
                viewersNo: product.viewersNo || 0,
                vendorName: product.storeName || 'Verified Vendor',
                category: product.categoryName || 'Other'
              }} />
            ))}
          </div>
        ) : (
          <div className="text-center py-20 bg-white rounded-3xl border border-slate-100 shadow-sm">
            <Filter size={48} className="mx-auto text-slate-300 mb-4" />
            <h3 className="text-xl font-bold text-slate-800 mb-2">No products found</h3>
            <p className="text-slate-500 text-sm mb-6 max-w-md mx-auto">No products currently match your selected category or price range.</p>
            <button onClick={() => { setSelectedCategory('All'); clearPriceFilter(); }} className="bg-emerald-600 text-white px-6 py-2.5 rounded-xl font-bold text-sm">
              Reset Filters
            </button>
          </div>
        )}

      </div>
    </div>
  );
};

export default Home;