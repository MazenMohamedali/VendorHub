// src/components/Navbar.jsx
import React, { useState, useEffect, useRef } from 'react';
import { Search, ShoppingCart, Heart, X } from 'lucide-react';
import { useSelector } from 'react-redux';
import { Link } from 'react-router-dom';
import axiosInstance from '../api/axiosConfig';

const Navbar = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [searchCategoryId, setSearchCategoryId] = useState(''); // selected category ID (as string from select)
  const [categories, setCategories] = useState([]);
  const [searchResults, setSearchResults] = useState([]);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const searchRef = useRef(null);
  
  // Redux data
  const cartTotalQuantity = useSelector((state) => state.cart.cartTotalQuantity);
  const favoriteItems = useSelector((state) => state.favorite.favoriteItems);
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
  const user = useSelector((state) => state.auth.user);

  // Fetch categories from backend
  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const response = await axiosInstance.get('/Category/active');
        const activeCategories = response.data.data || [];
        setCategories(activeCategories);
      } catch (error) {
        console.error("Error fetching categories:", error);
      }
    };
    fetchCategories();
  }, []);

  // Perform search when searchTerm or selected category changes
  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      // If searchTerm is empty, we don't show results unless a category is selected
      if (searchTerm.trim() !== '' || searchCategoryId) {
        performSearch();
      } else {
        setSearchResults([]);
        setIsSearchOpen(false);
      }
    }, 500);

    return () => clearTimeout(delayDebounce);
  }, [searchTerm, searchCategoryId]);

  const performSearch = async () => {
    setIsLoading(true);
    try {
      let response;
      // If a category is selected (and it's not "all")
      if (searchCategoryId) {
        const categoryId = parseInt(searchCategoryId, 10);
        console.log(`🔍 Searching products in category ID ${categoryId}`);
        response = await axiosInstance.get(`/Product/category/${categoryId}`);
      } else {
        // Search by name only
        console.log(`🔍 Searching products by name: ${searchTerm}`);
        response = await axiosInstance.get(`/Product/search-name`, {
          params: { name: searchTerm }
        });
      }
      const products = response.data.data || [];
      console.log("✅ Search results:", products);
      setSearchResults(products);
      setIsSearchOpen(true);
    } catch (error) {
      console.error("Search error:", error);
      setSearchResults([]);
    } finally {
      setIsLoading(false);
    }
  };

  // Handle category change – clear the search term to see all products of that category
  const handleCategoryChange = (e) => {
    const newCategoryId = e.target.value;
    setSearchCategoryId(newCategoryId);
    // If a category is selected, optionally clear the search term
    // (so that the user sees all products of that category immediately)
    if (newCategoryId) {
      setSearchTerm('');
    }
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (searchRef.current && !searchRef.current.contains(event.target)) {
        setIsSearchOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const clearSearch = () => {
    setSearchTerm('');
    setSearchCategoryId('');
    setSearchResults([]);
    setIsSearchOpen(false);
  };

  const selectedCategoryName = categories.find(cat => cat.id === parseInt(searchCategoryId))?.name || 'جميع الأقسام';

  return (
    <nav className="bg-white shadow-sm sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-20">
          
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2 cursor-pointer hover:opacity-90 transition-opacity shrink-0">
            <div className="bg-dokany w-10 h-10 rounded-xl flex items-center justify-center text-white font-bold text-2xl">د</div>
            <span className="text-2xl font-black text-dokany tracking-tight">دكاني</span>
          </Link>

          {/* Search Bar */}
          <div ref={searchRef} className="hidden md:flex flex-1 mx-8 relative">
            <div className="flex w-full bg-gray-100 rounded-full border border-gray-200 focus-within:ring-2 focus-within:ring-dokany transition-all overflow-hidden h-12">
              
              {/* Category selector */}
              <select
                value={searchCategoryId}
                onChange={handleCategoryChange}
                className="bg-gray-200 text-gray-700 text-sm font-medium px-4 outline-none cursor-pointer border-l border-gray-300 hover:bg-gray-300 transition-colors w-36"
              >
                <option value="">جميع الأقسام</option>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>{cat.name}</option>
                ))}
              </select>

              {/* Search input */}
              <input
                type="text"
                placeholder="ابحث عن منتجات، ماركات..."
                className="flex-1 bg-transparent border-none px-4 outline-none text-right rtl"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onFocus={() => (searchTerm.trim() || searchCategoryId) && setIsSearchOpen(true)}
              />

              {/* Search/Clear button */}
              <button 
                onClick={(searchTerm || searchCategoryId) ? clearSearch : undefined}
                className={`w-14 flex items-center justify-center transition-colors ${
                  (searchTerm || searchCategoryId) 
                    ? 'bg-transparent text-gray-400 hover:text-red-500' 
                    : 'bg-dokany text-white hover:bg-dokany-dark'
                }`}
              >
                {(searchTerm || searchCategoryId) ? <X size={18} /> : <Search size={18} />}
              </button>
            </div>

            {/* Search results dropdown */}
            {isSearchOpen && (
              <div className="absolute top-full mt-2 w-full bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden z-50 max-h-[400px] overflow-y-auto animate-fade-in-up">
                {isLoading ? (
                  <div className="p-8 text-center text-gray-500">
                    <div className="animate-spin w-6 h-6 border-2 border-dokany border-t-transparent rounded-full mx-auto mb-2"></div>
                    جاري البحث...
                  </div>
                ) : searchResults.length > 0 ? (
                  <div className="flex flex-col">
                    <div className="px-4 py-2 bg-gray-50 text-xs font-bold text-gray-500">
                      النتائج في {selectedCategoryName}
                    </div>
                    {searchResults.map((product) => (
                      <Link 
                        key={product.id} 
                        to={`/product/${product.id}`}
                        onClick={clearSearch}
                        className="flex items-center gap-4 p-4 hover:bg-emerald-50 transition-colors border-b border-gray-50 last:border-0"
                      >
                        <div className="w-12 h-12 bg-gray-100 rounded-lg p-1 shrink-0">
                          <img 
                            src={product.imgUrl?.startsWith('http') ? product.imgUrl : `/Images/Products/${product.imgUrl}`} 
                            alt={product.name} 
                            className="w-full h-full object-contain mix-blend-multiply"
                            onError={(e) => e.target.src = 'https://placehold.co/100x100?text=No+Image'}
                          />
                        </div>
                        <div className="flex-1">
                          <h4 className="font-bold text-gray-800 text-sm line-clamp-1">{product.name}</h4>
                          <span className="text-xs text-gray-500">{product.categoryName || 'منتج'}</span>
                        </div>
                        <span className="font-bold text-dokany">{product.price} ج.م</span>
                      </Link>
                    ))}
                  </div>
                ) : (
                  <div className="p-8 text-center text-gray-500">
                    <Search size={32} className="mx-auto text-gray-300 mb-2" />
                    لا توجد منتجات تطابق "{searchTerm || 'هذا القسم'}" في {selectedCategoryName}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Right side icons */}
          <div className="flex items-center gap-6 shrink-0">
            {isAuthenticated ? (
              <Link to="/my-orders" className="hidden lg:flex flex-col items-end text-sm text-gray-600 group cursor-pointer">
                <span className="text-xs text-gray-400">مرحباً، {user?.firstName || user?.name || 'أهلاً بك'}</span>
                <span className="font-bold group-hover:text-dokany transition-colors">حسابي وطلباتي</span>
              </Link>
            ) : (
              <Link to="/login" className="hidden lg:flex flex-col items-end text-sm text-gray-600 group cursor-pointer">
                <span className="text-xs text-gray-400">أهلاً بك، سجل الدخول</span>
                <span className="font-bold group-hover:text-dokany transition-colors">حسابي / دخول</span>
              </Link>
            )}
            
            <Link to="/favorites" className="relative text-gray-600 hover:text-dokany transition-colors">
              <Heart size={24} />
              {favoriteItems.length > 0 && (
                <span className="absolute -top-1 -right-1 bg-red-500 text-white text-[10px] w-4 h-4 rounded-full flex items-center justify-center">
                  {favoriteItems.length}
                </span>
              )}
            </Link>

            <Link to="/cart" className="relative text-gray-600 hover:text-dokany transition-colors">
              <ShoppingCart size={24} />
              {cartTotalQuantity > 0 && (
                <span className="absolute -top-1 -right-1 bg-dokany text-white text-[10px] w-4 h-4 rounded-full flex items-center justify-center">
                  {cartTotalQuantity}
                </span>
              )}
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;