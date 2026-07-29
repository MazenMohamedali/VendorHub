import React, { useState, useEffect, useRef } from 'react';
import { Search, ShoppingCart, Heart, X, Bell, User, LogOut, LayoutDashboard, Shield, Package } from 'lucide-react';
import { useSelector, useDispatch } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import axiosInstance from '../api/axiosConfig';
import { logout } from '../store/authSlice';
import signalRService from '../Services/signalRService';
import { notificationApi } from '../api';
import { getImageUrl } from '../utils/imageUtils';

const Navbar = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [searchCategoryId, setSearchCategoryId] = useState('');
  const [categories, setCategories] = useState([]);
  const [searchResults, setSearchResults] = useState([]);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const searchRef = useRef(null);

  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);

  const userMenuRef = useRef(null);
  const notificationRef = useRef(null);

  const dispatch = useDispatch();
  const navigate = useNavigate();

  const cartTotalQuantity = useSelector((state) => state.cart.cartTotalQuantity);
  const favoriteItems = useSelector((state) => state.favorite.favoriteItems);
  const { isAuthenticated, user } = useSelector((state) => state.auth);

  const userRoles = Array.isArray(user?.roles)
    ? user.roles
    : user?.role
    ? [user.role]
    : [];

  const isVendor = userRoles.includes('Vendor');
  const isAdmin = userRoles.includes('Admin');

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const response = await axiosInstance.get('/Category/active');
        const rawCategories = response.data?.data;
        const activeCategories = Array.isArray(rawCategories) ? rawCategories : (rawCategories?.items || []);
        setCategories(activeCategories);
      } catch (error) {
        setCategories([]);
      }
    };
    fetchCategories();
  }, []);

  useEffect(() => {
    const canReceiveNotifications = isAuthenticated;

    if (canReceiveNotifications) {
      signalRService.startConnection();

      const fetchUnreadNotifications = async () => {
        try {
          const res = await notificationApi.getUnread();
          const list = Array.isArray(res.data?.data) ? res.data.data : [];
          setNotifications(list);
          setUnreadCount(list.length);
        } catch (err) {
          setNotifications([]);
          setUnreadCount(0);
        }
      };

      fetchUnreadNotifications();

      // Subscribe to real-time notification events
      const unsubscribe = signalRService.onNotification((newNotify) => {
        console.log("⚡ Real-time notification received in Navbar:", newNotify);
        setNotifications(prev => [newNotify, ...prev]);
        setUnreadCount(prev => prev + 1);
      });

      return () => {
        unsubscribe();
      };
    } else {
    }
  }, [isAuthenticated, user]);

  const getNotificationText = (n) => {
    const type = (n.type || n.Type || '').toString();
    const orderId = n.orderId || n.OrderId;
    const status = n.status || n.Status;

    if (type === 'NewPurchase' || type === '0') {
      return {
        title: 'New Order Received',
        message: orderId ? `New order #${orderId} received.` : 'You have received a new customer order.'
      };
    }
    if (type === 'StatusUpdate' || type === '1') {
      return {
        title: 'Order Status Updated',
        message: orderId ? `Order #${orderId} status changed${status ? ' to ' + status : ''}.` : 'Your order status has been updated.'
      };
    }
    if (type === 'ProductApproved' || type === '4') {
      return {
        title: 'Product Approved',
        message: 'Your product submission has been approved.'
      };
    }
    if (type === 'LowStock' || type === '2') {
      return {
        title: 'Low Stock Warning',
        message: 'One of your products is low on stock.'
      };
    }
    return {
      title: n.title || n.Title || 'System Notification',
      message: n.message || n.Message || 'You have a new notification update.'
    };
  };

  const handleNotificationClick = async (n) => {
    try {
      if (!n.isRead && n.id && n.id !== 0) {
        await notificationApi.markAsRead(n.id);
        setUnreadCount(prev => Math.max(0, prev - 1));
        setNotifications(prev => prev.map(item => item.id === n.id ? { ...item, isRead: true } : item));
      }
    } catch (err) {
      console.error("Error marking notification as read:", err);
    }

    setIsNotificationsOpen(false);

    if (n.orderId || n.OrderId) {
      if (isVendor) {
        navigate('/vendor/orders');
      } else {
        navigate('/my-orders');
      }
    } else if (n.productId || n.ProductId) {
      navigate(`/product/${n.productId || n.ProductId}`);
    } else if (isVendor) {
      navigate('/vendor/orders');
    } else if (isAdmin) {
      navigate('/admin');
    } else {
      navigate('/my-orders');
    }
  };

  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      if (searchTerm.trim() !== '' || searchCategoryId) {
        performSearch();
      } else {
        setSearchResults([]);
        setIsSearchOpen(false);
      }
    }, 400);

    return () => clearTimeout(delayDebounce);
  }, [searchTerm, searchCategoryId]);

  const performSearch = async () => {
    setIsLoading(true);
    try {
      let response;
      if (searchCategoryId) {
        const categoryId = parseInt(searchCategoryId, 10);
        response = await axiosInstance.get(`/Product/category/${categoryId}`);
      } else {
        response = await axiosInstance.get(`/Product/search-name`, {
          params: { name: searchTerm }
        });
      }
      const rawProducts = response.data?.data;
      const products = Array.isArray(rawProducts) ? rawProducts : (rawProducts?.items || []);
      setSearchResults(products);
      setIsSearchOpen(true);
    } catch (error) {
      console.error("Search error:", error);
      setSearchResults([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCategoryChange = (e) => {
    const newCategoryId = e.target.value;
    setSearchCategoryId(newCategoryId);
    if (newCategoryId) setSearchTerm('');
  };

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (searchRef.current && !searchRef.current.contains(event.target)) {
        setIsSearchOpen(false);
      }
      if (userMenuRef.current && !userMenuRef.current.contains(event.target)) {
        setIsUserMenuOpen(false);
      }
      if (notificationRef.current && !notificationRef.current.contains(event.target)) {
        setIsNotificationsOpen(false);
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

  const handleLogout = () => {
    localStorage.removeItem('token');
    dispatch(logout());
    setIsUserMenuOpen(false);
    navigate('/login');
  };

  const handleMarkAllRead = async () => {
    try {
      await notificationApi.markAllAsRead();
      setUnreadCount(0);
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    } catch (err) {
      console.error(err);
    }
  };

  const selectedCategoryName = categories.find(cat => cat.id === parseInt(searchCategoryId))?.name || 'All Categories';

  return (
    <nav className="bg-white shadow-sm sticky top-0 z-50" dir="ltr">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-20">
          
          {/* Logo */}
          <Link to="/" className="flex items-center gap-3 cursor-pointer hover:opacity-90 transition-opacity shrink-0">
            <div className="bg-emerald-600 w-10 h-10 rounded-xl flex items-center justify-center text-white font-bold text-2xl shadow-md shadow-emerald-500/20">
              V
            </div>
            <span className="text-2xl font-black text-emerald-600 tracking-tight">VendorHub</span>
          </Link>

          {/* Search Bar */}
          <div ref={searchRef} className="hidden md:flex flex-1 mx-8 relative">
            <div className="flex w-full bg-gray-100 rounded-full border border-gray-200 focus-within:ring-2 focus-within:ring-emerald-500 transition-all overflow-hidden h-12">
              <select
                value={searchCategoryId}
                onChange={handleCategoryChange}
                className="bg-gray-200 text-gray-700 text-sm font-medium px-4 outline-none cursor-pointer border-r border-gray-300 hover:bg-gray-300 transition-colors w-36"
              >
                <option value="">All Categories</option>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>{cat.name}</option>
                ))}
              </select>

              <input
                type="text"
                placeholder="Search for products..."
                className="flex-1 bg-transparent px-4 outline-none text-gray-800 text-sm"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onFocus={() => (searchTerm.trim() || searchCategoryId) && setIsSearchOpen(true)}
              />

              <button 
                onClick={(searchTerm || searchCategoryId) ? clearSearch : undefined}
                className={`w-14 flex items-center justify-center transition-colors ${
                  (searchTerm || searchCategoryId) 
                    ? 'bg-transparent text-gray-400 hover:text-red-500' 
                    : 'bg-emerald-600 text-white hover:bg-emerald-700'
                }`}
              >
                {(searchTerm || searchCategoryId) ? <X size={18} /> : <Search size={18} />}
              </button>
            </div>

            {/* Search Dropdown */}
            {isSearchOpen && (
              <div className="absolute top-full mt-2 w-full bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden z-50 max-h-[400px] overflow-y-auto animate-fade-in-up" dir="ltr">
                {isLoading ? (
                  <div className="p-8 text-center text-gray-500">
                    <div className="animate-spin w-6 h-6 border-2 border-emerald-600 border-t-transparent rounded-full mx-auto mb-2"></div>
                    Searching...
                  </div>
                ) : searchResults.length > 0 ? (
                  <div className="flex flex-col">
                    <div className="px-4 py-2 bg-gray-50 text-xs font-bold text-gray-500">
                      Results in {selectedCategoryName}
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
                            src={getImageUrl(product.imgUrl, 'Products')} 
                            alt={product.name} 
                            className="w-full h-full object-contain mix-blend-multiply"
                            onError={(e) => e.target.src = 'https://placehold.co/100x100?text=No+Image'}
                          />
                        </div>
                        <div className="flex-1">
                          <h4 className="font-bold text-gray-800 text-sm line-clamp-1">{product.name}</h4>
                          <span className="text-xs text-gray-500">{product.categoryName || 'Product'}</span>
                        </div>
                        <span className="font-bold text-emerald-600">{product.price} EGP</span>
                      </Link>
                    ))}
                  </div>
                ) : (
                  <div className="p-8 text-center text-gray-500">
                    <Search size={32} className="mx-auto text-gray-300 mb-2" />
                    No products matching "{searchTerm || 'this category'}"
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Action Icons */}
          <div className="flex items-center gap-5 shrink-0" dir="ltr">
            
            {/* Notification Bell */}
            {isAuthenticated && (
              <div ref={notificationRef} className="relative">
                <button
                  onClick={() => setIsNotificationsOpen(!isNotificationsOpen)}
                  className="relative p-2 text-gray-600 hover:text-emerald-600 transition-colors rounded-full hover:bg-gray-100"
                >
                  <Bell size={22} />
                  {unreadCount > 0 && (
                    <span className="absolute top-1 right-1 bg-red-500 text-white text-[10px] w-4 h-4 rounded-full flex items-center justify-center font-bold">
                      {unreadCount}
                    </span>
                  )}
                </button>

                {/* Notifications Dropdown */}
                {isNotificationsOpen && (
                  <div className="absolute right-0 mt-3 w-80 bg-white rounded-2xl shadow-xl border border-gray-100 z-50 overflow-hidden animate-fade-in-up">
                    <div className="p-4 border-b border-gray-100 flex justify-between items-center bg-gray-50">
                      <span className="font-bold text-sm text-gray-800">Notifications ({unreadCount})</span>
                      {unreadCount > 0 && (
                        <button onClick={handleMarkAllRead} className="text-xs text-emerald-600 font-bold hover:underline">
                          Mark all as read
                        </button>
                      )}
                    </div>
                    <div className="max-h-80 overflow-y-auto divide-y divide-gray-50">
                      {notifications.length > 0 ? (
                        notifications.map((n, idx) => {
                          const { title, message } = getNotificationText(n);
                          const date = n.createdAt || n.CreatedAt || n.timestamp || n.Timestamp;
                          const isRead = Boolean(n.isRead || n.IsRead);

                          return (
                            <div 
                              key={n.id || idx} 
                              onClick={() => handleNotificationClick(n)}
                              className={`p-3.5 transition-all text-left cursor-pointer border-l-4 ${
                                isRead 
                                  ? 'border-transparent bg-white hover:bg-gray-50' 
                                  : 'border-emerald-500 bg-emerald-50/40 hover:bg-emerald-50'
                              }`}
                            >
                              <div className="flex items-center justify-between gap-2 mb-1">
                                <p className="font-bold text-xs text-gray-800 line-clamp-1">{title}</p>
                                {!isRead && <span className="w-2 h-2 bg-emerald-500 rounded-full shrink-0"></span>}
                              </div>
                              <p className="text-xs text-gray-600 leading-snug line-clamp-2">{message}</p>
                              <span className="text-[10px] text-gray-400 mt-1 block">
                                {date ? new Date(date).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) : 'Just now'}
                              </span>
                            </div>
                          );
                        })
                      ) : (
                        <div className="p-8 text-center text-xs text-gray-400">No new notifications</div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Wishlist */}
            <Link to="/favorites" className="relative p-2 text-gray-600 hover:text-emerald-600 transition-colors rounded-full hover:bg-gray-100">
              <Heart size={22} />
              {favoriteItems.length > 0 && (
                <span className="absolute top-1 right-1 bg-red-500 text-white text-[10px] w-4 h-4 rounded-full flex items-center justify-center font-bold">
                  {favoriteItems.length}
                </span>
              )}
            </Link>

            {/* Cart */}
            <Link to="/cart" className="relative p-2 text-gray-600 hover:text-emerald-600 transition-colors rounded-full hover:bg-gray-100">
              <ShoppingCart size={22} />
              {cartTotalQuantity > 0 && (
                <span className="absolute top-1 right-1 bg-emerald-600 text-white text-[10px] w-4 h-4 rounded-full flex items-center justify-center font-bold">
                  {cartTotalQuantity}
                </span>
              )}
            </Link>

            {/* User Profile Dropdown */}
            {isAuthenticated ? (
              <div ref={userMenuRef} className="relative">
                <button
                  onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                  className="flex items-center gap-2 p-1.5 rounded-2xl hover:bg-gray-100 transition-colors border border-gray-200"
                >
                  <div className="w-8 h-8 bg-emerald-600 text-white rounded-xl flex items-center justify-center font-bold text-sm">
                    {user?.firstName?.[0] || 'V'}
                  </div>
                  <div className="hidden lg:flex flex-col text-left">
                    <span className="text-xs font-bold text-gray-800 line-clamp-1">{user?.firstName || 'My Account'}</span>
                    <span className="text-[10px] text-emerald-600 font-semibold">
                      {isAdmin ? 'System Admin' : isVendor ? 'Verified Vendor' : 'Customer'}
                    </span>
                  </div>
                </button>

                {/* Dropdown Menu */}
                {isUserMenuOpen && (
                  <div className="absolute right-0 mt-3 w-56 bg-white rounded-2xl shadow-xl border border-gray-100 z-50 overflow-hidden animate-fade-in-up py-2" dir="ltr">
                    <div className="px-4 py-3 border-b border-gray-100 bg-gray-50/50">
                      <p className="font-bold text-sm text-gray-800">{user?.firstName} {user?.secondName}</p>
                      <p className="text-xs text-gray-400 truncate">{user?.email}</p>
                    </div>

                    <Link
                      to="/my-orders"
                      onClick={() => setIsUserMenuOpen(false)}
                      className="flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-gray-700 hover:bg-emerald-50 hover:text-emerald-600 transition-colors"
                    >
                      <Package size={18} /> My Orders
                    </Link>

                    {isVendor && (
                      <Link
                        to="/vendor"
                        onClick={() => setIsUserMenuOpen(false)}
                        className="flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-emerald-600 hover:bg-emerald-50 transition-colors"
                      >
                        <LayoutDashboard size={18} /> Vendor Dashboard
                      </Link>
                    )}

                    {isAdmin && (
                      <Link
                        to="/admin"
                        onClick={() => setIsUserMenuOpen(false)}
                        className="flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-purple-600 hover:bg-purple-50 transition-colors"
                      >
                        <Shield size={18} /> Admin Panel
                      </Link>
                    )}

                    <div className="border-t border-gray-100 my-1"></div>

                    <button
                      onClick={handleLogout}
                      className="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-red-600 hover:bg-red-50 transition-colors text-left"
                    >
                      <LogOut size={18} /> Sign Out
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <Link
                to="/login"
                className="bg-emerald-600 text-white px-5 py-2.5 rounded-xl font-bold text-sm hover:bg-emerald-700 transition-all shadow-md shadow-emerald-500/20"
              >
                Sign In
              </Link>
            )}

          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;