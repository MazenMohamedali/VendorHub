// src/components/Navbar.jsx
import React, { useState, useEffect, useRef } from 'react';
import { Search, ShoppingCart, Heart, Menu, X } from 'lucide-react';
import { useSelector } from 'react-redux';
import { Link } from 'react-router-dom';
import { mockProducts } from '../data/mockApi'; 

const Navbar = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [searchCategory, setSearchCategory] = useState('جميع الأقسام'); // حالة القسم المحدد
  const [searchResults, setSearchResults] = useState([]);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const searchRef = useRef(null); 
  
  // قراءة البيانات من Redux
  const cartTotalQuantity = useSelector((state) => state.cart.cartTotalQuantity);
  const favoriteItems = useSelector((state) => state.favorite.favoriteItems);
  
  // قراءة حالة تسجيل الدخول وبيانات المستخدم
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
  const user = useSelector((state) => state.auth.user);

  // استخراج الأقسام المتاحة من المنتجات (مثل أمازون)
  const categories = ['جميع الأقسام', ...new Set(mockProducts.map(p => p.category))];

  // دالة البحث الحي (تراقب تغيير الكلمة أو القسم)
  useEffect(() => {
    if (searchTerm.trim() !== '') {
      const filtered = mockProducts.filter(product => {
        // التحقق من مطابقة الاسم
        const matchTerm = product.title.toLowerCase().includes(searchTerm.toLowerCase()) || 
                          product.description.toLowerCase().includes(searchTerm.toLowerCase());
        // التحقق من مطابقة القسم (إذا لم يكن "جميع الأقسام")
        const matchCategory = searchCategory === 'جميع الأقسام' || product.category === searchCategory;
        
        return matchTerm && matchCategory;
      });
      setSearchResults(filtered);
      setIsSearchOpen(true);
    } else {
      setSearchResults([]);
      setIsSearchOpen(false);
    }
  }, [searchTerm, searchCategory]); // يتم تنفيذ البحث كلما تغيرت الكلمة أو القسم

  // إغلاق قائمة البحث عند الضغط خارجها
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
    setSearchResults([]);
    setIsSearchOpen(false);
  };

  return (
    <nav className="bg-white shadow-sm sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-20">
          
          {/* اللوجو */}
          <Link to="/" className="flex items-center gap-2 cursor-pointer hover:opacity-90 transition-opacity shrink-0">
            <div className="bg-dokany w-10 h-10 rounded-xl flex items-center justify-center text-white font-bold text-2xl">د</div>
            <span className="text-2xl font-black text-dokany tracking-tight">دكاني</span>
          </Link>

          {/* شريط البحث المتقدم (Amazon Style) */}
          <div ref={searchRef} className="hidden md:flex flex-1 mx-8 relative">
            <div className="flex w-full bg-gray-100 rounded-full border border-gray-200 focus-within:ring-2 focus-within:ring-dokany transition-all overflow-hidden h-12">
              
              {/* 1. قائمة اختيار القسم */}
              <select
                value={searchCategory}
                onChange={(e) => setSearchCategory(e.target.value)}
                className="bg-gray-200 text-gray-700 text-sm font-medium px-4 outline-none cursor-pointer border-l border-gray-300 hover:bg-gray-300 transition-colors w-36"
              >
                {categories.map((cat, idx) => (
                  <option key={idx} value={cat}>{cat}</option>
                ))}
              </select>

              {/* 2. حقل كتابة البحث */}
              <input
                type="text"
                placeholder="ابحث عن منتجات، ماركات..."
                className="flex-1 bg-transparent border-none px-4 outline-none text-right rtl"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onFocus={() => searchTerm.trim() && setIsSearchOpen(true)}
              />

              {/* 3. زر البحث / المسح */}
              <button 
                onClick={searchTerm ? clearSearch : undefined}
                className={`w-14 flex items-center justify-center transition-colors ${
                  searchTerm ? 'bg-transparent text-gray-400 hover:text-red-500' : 'bg-dokany text-white hover:bg-dokany-dark'
                }`}
              >
                {searchTerm ? <X size={18} /> : <Search size={18} />}
              </button>
            </div>

            {/* القائمة المنسدلة لنتائج البحث */}
            {isSearchOpen && (
              <div className="absolute top-full mt-2 w-full bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden z-50 max-h-[400px] overflow-y-auto animate-fade-in-up">
                {searchResults.length > 0 ? (
                  <div className="flex flex-col">
                    <div className="px-4 py-2 bg-gray-50 text-xs font-bold text-gray-500">
                      النتائج في {searchCategory}
                    </div>
                    {searchResults.map((product) => (
                      <Link 
                        key={product.id} 
                        to={`/product/${product.id}`}
                        onClick={clearSearch}
                        className="flex items-center gap-4 p-4 hover:bg-emerald-50 transition-colors border-b border-gray-50 last:border-0"
                      >
                        <div className="w-12 h-12 bg-gray-100 rounded-lg p-1 shrink-0">
                          <img src={product.images[0]} alt={product.title} className="w-full h-full object-contain mix-blend-multiply" />
                        </div>
                        <div className="flex-1">
                          <h4 className="font-bold text-gray-800 text-sm line-clamp-1">{product.title}</h4>
                          <span className="text-xs text-gray-500">{product.category}</span>
                        </div>
                        <span className="font-bold text-dokany">{product.price} ج.م</span>
                      </Link>
                    ))}
                  </div>
                ) : (
                  <div className="p-8 text-center text-gray-500">
                    <Search size={32} className="mx-auto text-gray-300 mb-2" />
                    لا توجد منتجات تطابق "{searchTerm}" في {searchCategory}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* الروابط والأيقونات */}
          <div className="flex items-center gap-6 shrink-0">
            
            {/* التحقق من حالة تسجيل الدخول لتغيير رابط حسابي */}
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