import React from 'react';
import { ShoppingCart, Eye, Heart } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { addToCart } from '../store/cartSlice';
import { toggleFavorite } from '../store/favoriteSlice';
import { getProductImageUrl } from '../utils/imageUtils';

const ProductCard = ({ product }) => {
  const dispatch = useDispatch();

  const favoriteItems = useSelector((state) => state.favorite.favoriteItems);
  const isFavorite = favoriteItems.some((item) => Number(item.id) === Number(product.id));

  const stockCount = product.unitsInStock ?? product.quantity ?? product.stockQuantity ?? 1;
  const isOutOfStock = stockCount <= 0;

  const title = product.title || product.name || 'Product';
  const price = product.price || 0;
  const category = product.category || product.categoryName || 'General';
  const vendorName = product.vendorName || product.storeName || 'Verified Vendor';
  const viewers = product.viewersNo || product.viewers || 0;

  const displayImage = Array.isArray(product.images) && product.images[0]
    ? product.images[0]
    : getProductImageUrl(product.imgUrl || product.imageUrl);

  const handleAddToCart = (e) => {
    e.preventDefault();
    e.stopPropagation();
    dispatch(addToCart({
      id: product.id,
      title,
      name: title,
      price,
      imgUrl: product.imgUrl || product.imageUrl,
      images: [displayImage],
      stockQuantity: stockCount,
      vendorName,
    }));
  };

  const handleToggleFavorite = (e) => {
    e.preventDefault();
    e.stopPropagation();
    dispatch(toggleFavorite({
      id: product.id,
      title,
      name: title,
      price,
      imgUrl: product.imgUrl || product.imageUrl,
      images: [displayImage],
      category,
      vendorName,
    }));
  };

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-all duration-300 flex flex-col justify-between relative group" dir="ltr">
      {/* Wishlist toggle button */}
      <button
        onClick={handleToggleFavorite}
        className="absolute top-3 right-3 z-10 p-2 bg-white/90 backdrop-blur rounded-full hover:bg-white text-gray-400 hover:text-red-500 transition-colors shadow-sm"
      >
        <Heart size={18} className={isFavorite ? 'fill-red-500 text-red-500' : ''} />
      </button>

      <div>
        {/* Image container */}
        <Link to={`/product/${product.id}`} className="block relative">
          <div className="bg-gray-50 h-52 w-full flex items-center justify-center p-4 overflow-hidden">
            <img
              src={displayImage}
              alt={title}
              className="max-h-full max-w-full object-contain mix-blend-multiply transition-transform duration-300 group-hover:scale-105"
              onError={(e) => {
                e.target.src = 'https://placehold.co/400x400?text=No+Image';
              }}
            />
            {isOutOfStock && (
              <span className="absolute top-3 left-3 bg-red-100 text-red-600 text-xs font-bold px-2.5 py-1 rounded-full shadow-sm">
                Out of Stock
              </span>
            )}
          </div>
        </Link>

        {/* Info */}
        <div className="p-4">
          <div className="flex justify-between items-center mb-1.5">
            <span className="text-xs text-emerald-600 font-bold bg-emerald-50 px-2 py-0.5 rounded">
              {category}
            </span>
            <div className="flex items-center text-xs text-gray-400 gap-1">
              <Eye size={14} />
              <span>{viewers}</span>
            </div>
          </div>

          <Link to={`/product/${product.id}`}>
            <h3 className="font-bold text-gray-800 text-base line-clamp-1 mb-1 hover:text-emerald-600 transition-colors">
              {title}
            </h3>
          </Link>

          <p className="text-xs text-gray-400 mb-3">
            Vendor: <span className="font-medium text-gray-600">{vendorName}</span>
          </p>

          <div className="flex justify-between items-center mb-2">
            <span className="text-xl font-black text-emerald-600">{price} <span className="text-xs">EGP</span></span>
          </div>
        </div>
      </div>

      {/* Add to cart */}
      <div className="p-4 pt-0">
        <button
          onClick={handleAddToCart}
          disabled={isOutOfStock}
          className={`w-full flex items-center justify-center gap-2 py-2.5 rounded-xl font-bold transition-all text-sm ${
            isOutOfStock
              ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
              : 'bg-emerald-600 text-white hover:bg-emerald-700 shadow-md shadow-emerald-500/20'
          }`}
        >
          <ShoppingCart size={16} />
          {isOutOfStock ? 'Currently Unavailable' : 'Add to Cart'}
        </button>
      </div>
    </div>
  );
};

export default ProductCard;