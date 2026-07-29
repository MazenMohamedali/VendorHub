import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { ShoppingCart, Heart, ArrowLeft, ShieldCheck, Truck, RotateCcw, Loader2, XCircle } from 'lucide-react';
import { addToCart } from '../store/cartSlice';
import { toggleFavorite } from '../store/favoriteSlice';
import { productApi } from '../api';
import { getProductImageUrl } from '../utils/imageUtils';
import ProductReviews from './ProductReview';
import { useToast } from '../components/Toast';

const ProductDetails = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { showSuccess } = useToast();

  const [product, setProduct] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [quantity, setQuantity] = useState(1);

  const favoriteItems = useSelector((state) => state.favorite.favoriteItems);
  const cartItems = useSelector((state) => state.cart.cartItems);
  
  const isFavorite = favoriteItems.some((item) => Number(item.id) === Number(id));
  const itemInCart = cartItems.find((item) => Number(item.id) === Number(id));
  const currentCartQty = itemInCart ? itemInCart.cartQuantity : 0;

  useEffect(() => {
    const fetchProductDetails = async () => {
      try {
        setIsLoading(true);
        const response = await productApi.getProductDetailsCustomer(id);
        const productData = response.data?.data;

        if (!productData) {
          setError('Product not found.');
          return;
        }

        const stock = productData.unitsInStock ?? productData.quantity ?? 0;
        const formattedProduct = {
          id: productData.id,
          title: productData.name,
          name: productData.name,
          price: productData.price,
          description: productData.description || 'No description available for this product.',
          category: productData.categoryName || 'Uncategorized',
          vendorName: productData.storeName || 'Verified Vendor',
          stockQuantity: stock,
          imageUrl: getProductImageUrl(productData.imgUrl),
          images: [getProductImageUrl(productData.imgUrl)],
        };

        setProduct(formattedProduct);
      } catch (err) {
        console.error("Error fetching product details:", err);
        setError('Product not found or connection failed.');
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
      showSuccess(`Added ${quantity} of "${product.title}" to cart`);
    }
  };

  const handleToggleFavorite = () => {
    if (product) {
      dispatch(toggleFavorite(product));
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-[70vh] flex flex-col items-center justify-center text-emerald-600">
        <Loader2 className="animate-spin mb-4" size={48} />
        <h2 className="text-xl font-bold">Loading product details...</h2>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="min-h-[70vh] flex flex-col items-center justify-center text-center px-4" dir="ltr">
        <div className="bg-red-50 text-red-500 w-24 h-24 rounded-full flex items-center justify-center mb-6">
          <ShieldCheck size={48} />
        </div>
        <h2 className="text-3xl font-black text-gray-800 mb-4">Sorry, product not available</h2>
        <p className="text-gray-500 mb-8">{error}</p>
        <button onClick={() => navigate('/')} className="bg-emerald-600 text-white px-8 py-3 rounded-xl font-bold flex items-center gap-2 hover:bg-emerald-700">
          <ArrowLeft size={20} /> Back to Home
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 animate-fade-in-down" dir="ltr">
      
      {/* Breadcrumbs */}
      <div className="flex items-center gap-2 text-sm text-gray-500 mb-8">
        <Link to="/" className="hover:text-emerald-600 transition-colors">Home</Link>
        <span>/</span>
        <span className="hover:text-emerald-600 transition-colors cursor-pointer">{product.category}</span>
        <span>/</span>
        <span className="text-gray-800 font-bold truncate">{product.title}</span>
      </div>

      <div className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden mb-10">
        <div className="flex flex-col md:flex-row">
          
          {/* Image */}
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
              className="max-h-[450px] object-contain mix-blend-multiply transition-transform duration-500 group-hover:scale-105" 
              onError={(e) => { e.target.src = 'https://placehold.co/600x600?text=No+Image'; }}
            />
          </div>

          {/* Product Info */}
          <div className="w-full md:w-1/2 p-8 lg:p-12 flex flex-col justify-center">
            <div className="mb-3">
              <span className="bg-emerald-100 text-emerald-700 text-xs px-3 py-1.5 rounded-full font-bold">
                {product.category}
              </span>
            </div>
            
            <h1 className="text-3xl lg:text-4xl font-black text-gray-800 mb-3 leading-tight">
              {product.title}
            </h1>
            
            <p className="text-sm text-gray-500 mb-6 font-medium">
              Vendor: <span className="text-emerald-600 font-bold">{product.vendorName}</span>
            </p>

            <div className="text-4xl font-black text-emerald-600 mb-6">
              {product.price} <span className="text-xl">EGP</span>
            </div>

            <p className="text-gray-600 leading-relaxed mb-8 text-sm">
              {product.description}
            </p>

            <div className="mb-8">
              <p className="text-xs font-bold mb-2 text-gray-400">Stock Status:</p>
              {product.stockQuantity > 0 ? (
                <span className="text-emerald-600 font-bold flex items-center gap-2 text-sm">
                  <ShieldCheck size={20} /> In Stock ({product.stockQuantity} items available)
                </span>
              ) : (
                <span className="text-red-500 font-bold flex items-center gap-2 text-sm">
                  <XCircle size={20} /> Out of Stock
                </span>
              )}
            </div>

            {/* Quantity selector & Add to cart */}
            <div className="flex flex-col sm:flex-row gap-4 mb-8">
              <div className="flex items-center justify-between bg-gray-50 border border-gray-200 rounded-xl p-2 w-full sm:w-36 shrink-0">
                <button 
                  onClick={() => setQuantity(prev => prev > 1 ? prev - 1 : 1)}
                  className="w-10 h-10 flex items-center justify-center bg-white rounded-lg shadow-sm font-bold text-gray-700 hover:text-red-500"
                >
                  -
                </button>
                <span className="font-bold text-gray-800">{quantity}</span>
                <button 
                  onClick={() => setQuantity(prev => prev + 1)}
                  disabled={quantity + currentCartQty >= product.stockQuantity}
                  className="w-10 h-10 flex items-center justify-center bg-white rounded-lg shadow-sm font-bold text-gray-700 hover:text-emerald-600 disabled:opacity-40"
                >
                  +
                </button>
              </div>

              <button 
                onClick={handleAddToCart}
                disabled={product.stockQuantity === 0 || currentCartQty >= product.stockQuantity}
                className="flex-1 bg-emerald-600 text-white py-4 rounded-xl font-bold text-base hover:bg-emerald-700 transition-all shadow-lg shadow-emerald-600/30 flex items-center justify-center gap-3 disabled:opacity-50 disabled:cursor-not-allowed disabled:shadow-none"
              >
                <ShoppingCart size={22} />
                {product.stockQuantity === 0 
                  ? 'Out of Stock' 
                  : currentCartQty >= product.stockQuantity 
                    ? 'Max Quantity in Cart' 
                    : 'Add to Cart'}
              </button>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-6 border-t border-gray-100">
              <div className="flex items-center gap-3 text-gray-600">
                <div className="w-10 h-10 bg-emerald-50 text-emerald-600 flex items-center justify-center rounded-lg">
                  <Truck size={20} />
                </div>
                <div className="text-sm">
                  <p className="font-bold text-gray-800">Fast Delivery</p>
                  <p className="text-xs text-gray-400">Nationwide shipping</p>
                </div>
              </div>
              <div className="flex items-center gap-3 text-gray-600">
                <div className="w-10 h-10 bg-blue-50 text-blue-500 flex items-center justify-center rounded-lg">
                  <RotateCcw size={20} />
                </div>
                <div className="text-sm">
                  <p className="font-bold text-gray-800">Free Returns</p>
                  <p className="text-xs text-gray-400">Within 14 days</p>
                </div>
              </div>
            </div>

          </div>
        </div>
      </div>

      <ProductReviews productId={product.id} />
    </div>
  );
};

export default ProductDetails;