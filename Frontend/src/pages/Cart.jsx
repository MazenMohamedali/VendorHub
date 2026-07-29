import { getImageUrl } from '../utils/imageUtils';
import React, { useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import { Trash2, Plus, Minus, ArrowLeft, ShoppingBag, ShieldCheck, X, MapPin, Phone, Loader2 } from 'lucide-react';
import { removeFromCart, increaseQuantity, decreaseQuantity, clearCart } from '../store/cartSlice';
import axiosInstance from '../api/axiosConfig';

const Cart = () => {
  const cartItems = useSelector((state) => state.cart.cartItems);
  const cartTotalAmount = useSelector((state) => state.cart.cartTotalAmount);
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
  
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const [isCheckoutModalOpen, setIsCheckoutModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [checkoutData, setCheckoutData] = useState({
    deliveryAddress: '',
    phoneNumber: ''
  });
  const [phoneError, setPhoneError] = useState('');

  const validatePhone = (phone) => {
    const regex = /^01[0125][0-9]{8}$/;
    return regex.test(phone);
  };

  const handleOpenCheckout = () => {
    if (!isAuthenticated) {
      alert("Please sign in first to complete your purchase!");
      navigate('/login');
    } else {
      setIsCheckoutModalOpen(true);
    }
  };

  const handleFinalSubmit = async (e) => {
    e.preventDefault();
    
    if (!validatePhone(checkoutData.phoneNumber)) {
      setPhoneError("Invalid phone number. Must start with 01 followed by 8 digits.");
      return;
    }

    setIsSubmitting(true);

    const orderPayload = {
      deliveryAddress: checkoutData.deliveryAddress,
      phoneNumber: checkoutData.phoneNumber,
      cartItems: cartItems.map(item => ({
        productId: Number(item.id),
        productName: item.title || item.name || 'Product',
        price: Number(item.price),
        quantity: item.cartQuantity,
        imageUrl: item.images?.[0] || item.imgUrl || ''
      }))
    };

    try {
      await axiosInstance.post('/Order', orderPayload);

      alert("Order placed successfully! Thank you for shopping with VendorHub.");
      dispatch(clearCart());
      setIsCheckoutModalOpen(false);
      navigate('/my-orders');

    } catch (error) {
      console.error("Error submitting order:", error);
      alert(error.response?.data?.message || "Error completing your order. Please try again later.");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (cartItems.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-20 w-full flex flex-col items-center justify-center text-center animate-fade-in-down" dir="ltr">
        <div className="bg-gray-100 w-32 h-32 rounded-full flex items-center justify-center mb-6 text-gray-400">
          <ShoppingBag size={64} />
        </div>
        <h2 className="text-3xl font-black text-gray-800 mb-4">Your Cart is Empty</h2>
        <p className="text-gray-500 mb-8">You haven't added any products to your cart yet.</p>
        <Link to="/" className="bg-emerald-600 text-white px-8 py-3 rounded-xl font-bold hover:bg-emerald-700 transition-colors flex items-center gap-2">
          <ArrowLeft size={20} />
          Back to Shopping
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 animate-fade-in-down w-full relative" dir="ltr">
      <h1 className="text-3xl font-black text-gray-800 mb-8">Shopping Cart</h1>

      <div className="flex flex-col lg:flex-row gap-8">
        {/* Cart Items List */}
        <div className="w-full lg:w-2/3 bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="p-6 border-b border-gray-100 flex justify-between items-center">
            <span className="font-bold text-gray-800">Products ({cartItems.length})</span>
            <button onClick={() => dispatch(clearCart())} className="text-red-500 text-sm font-medium hover:underline">Clear Cart</button>
          </div>
          <div className="divide-y divide-gray-100">
            {cartItems.map((item) => (
              <div key={item.id} className="p-6 flex flex-col sm:flex-row items-center gap-6">
                <div className="w-24 h-24 bg-gray-50 rounded-xl flex items-center justify-center p-2 shrink-0 border border-gray-100">
                  <img src={item.images?.[0] || getImageUrl(item.imgUrl, 'Products') || 'https://placehold.co/100x100?text=No+Image'} alt={item.title} className="max-h-full object-contain" />
                </div>
                <div className="flex-1 text-center sm:text-left">
                  <Link to={`/product/${item.id}`} className="font-bold text-gray-800 hover:text-emerald-600 transition-colors line-clamp-1 mb-1">{item.title}</Link>
                  <p className="text-sm text-gray-500 mb-2">Vendor: {item.vendorName || 'Verified Vendor'}</p>
                  <p className="font-bold text-emerald-600 text-lg">{item.price} EGP</p>
                </div>
                <div className="flex flex-col items-center gap-4 shrink-0">
                  <div className="flex items-center gap-3 bg-gray-50 border border-gray-200 rounded-lg p-1">
                    <button onClick={() => dispatch(decreaseQuantity(item))} className="w-8 h-8 flex items-center justify-center bg-white rounded shadow-sm hover:text-red-500"><Minus size={16} /></button>
                    <span className="font-bold text-gray-800 w-4 text-center">{item.cartQuantity}</span>
                    <button onClick={() => dispatch(increaseQuantity(item))} className="w-8 h-8 flex items-center justify-center bg-white rounded shadow-sm hover:text-emerald-600"><Plus size={16} /></button>
                  </div>
                  <button onClick={() => dispatch(removeFromCart(item))} className="text-gray-400 hover:text-red-500 flex items-center gap-1 text-sm"><Trash2 size={16} /> Delete</button>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Order Summary */}
        <div className="w-full lg:w-1/3">
          <div className="bg-white rounded-3xl shadow-sm border border-gray-100 p-6 sticky top-28">
            <h2 className="text-xl font-bold text-gray-800 mb-6 border-b border-gray-100 pb-4">Order Summary</h2>
            <div className="flex justify-between items-center mb-8">
              <span className="text-lg font-bold text-gray-800">Grand Total</span>
              <span className="text-2xl font-black text-emerald-600">{cartTotalAmount} EGP</span>
            </div>
            <button onClick={handleOpenCheckout} className="w-full bg-emerald-600 text-white py-4 rounded-xl font-bold text-lg hover:bg-emerald-700 shadow-lg shadow-emerald-500/30 mb-4 flex items-center justify-center gap-2">
              <ShieldCheck size={20} /> Proceed to Checkout
            </button>
          </div>
        </div>
      </div>

      {/* Checkout Modal */}
      {isCheckoutModalOpen && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4" dir="ltr">
          <div className="bg-white rounded-3xl w-full max-w-md p-8 shadow-2xl animate-fade-in-up">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-2xl font-black text-gray-800">Delivery Information</h2>
              <button onClick={() => setIsCheckoutModalOpen(false)} className="text-gray-400 hover:text-red-500"><X size={24} /></button>
            </div>
            
            <form onSubmit={handleFinalSubmit} className="space-y-6">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">Detailed Delivery Address</label>
                <div className="relative">
                  <MapPin size={20} className="absolute left-4 top-3.5 text-gray-400" />
                  <textarea 
                    required 
                    maxLength={500}
                    placeholder="e.g. 123 Main Street, Apt 4B, New York..."
                    value={checkoutData.deliveryAddress}
                    onChange={(e) => setCheckoutData({...checkoutData, deliveryAddress: e.target.value})}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none resize-none text-left"
                    rows={3}
                  ></textarea>
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">Phone Number</label>
                <div className="relative">
                  <Phone size={20} className="absolute left-4 top-3.5 text-gray-400" />
                  <input 
                    type="tel" 
                    required
                    placeholder="01012345678"
                    value={checkoutData.phoneNumber}
                    onChange={(e) => {
                      setCheckoutData({...checkoutData, phoneNumber: e.target.value});
                      setPhoneError('');
                    }}
                    className={`w-full bg-gray-50 border ${phoneError ? 'border-red-500' : 'border-gray-200'} rounded-xl py-3 pl-12 pr-4 focus:ring-2 focus:ring-emerald-500 outline-none text-left`}
                  />
                </div>
                {phoneError && <p className="text-red-500 text-xs mt-2 font-medium">{phoneError}</p>}
              </div>

              <div className="bg-emerald-50 p-4 rounded-xl border border-emerald-100 mb-6">
                <div className="flex justify-between items-center font-bold text-emerald-600">
                  <span>Total Cash on Delivery:</span>
                  <span>{cartTotalAmount} EGP</span>
                </div>
              </div>

              <button type="submit" disabled={isSubmitting} className="w-full bg-emerald-600 text-white py-4 rounded-xl font-bold text-lg hover:bg-emerald-700 transition-all flex justify-center items-center gap-2 disabled:opacity-70">
                {isSubmitting ? <Loader2 className="animate-spin" size={24} /> : 'Confirm Order Now'}
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Cart;