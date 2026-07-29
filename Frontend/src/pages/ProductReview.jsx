import React, { useState, useEffect } from 'react';
import { Star, MessageSquare, Loader2, Send } from 'lucide-react';
import { useSelector } from 'react-redux';
import { reviewApi } from '../api';
import { useToast } from '../components/Toast';

const ProductReviews = ({ productId }) => {
  const user = useSelector((state) => state.auth.user);
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
  const { showSuccess, showError } = useToast();

  const [reviews, setReviews] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmittingReview, setIsSubmittingReview] = useState(false);
  const [showReviewForm, setShowReviewForm] = useState(false);

  const [formData, setFormData] = useState({
    rating: 5,
    comment: '',
  });

  const fetchReviews = async () => {
    try {
      setIsLoading(true);
      const response = await reviewApi.getProductReviews(productId);
      const rawData = response.data?.data;
      const reviewsList = Array.isArray(rawData) ? rawData : (rawData?.items || []);
      setReviews(reviewsList);
    } catch (error) {
      console.error("Error fetching reviews:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (productId) fetchReviews();
  }, [productId]);

  const handleSubmitReview = async (e) => {
    e.preventDefault();
    setIsSubmittingReview(true);

    try {
      await reviewApi.addReview(productId, {
        rating: parseInt(formData.rating, 10),
        comment: formData.comment,
      });

      showSuccess('Thank you for your review! Your feedback was saved.');
      setFormData({ rating: 5, comment: '' });
      setShowReviewForm(false);
      fetchReviews();
    } catch (error) {
      console.error("Review submission error:", error);
      showError(error.response?.data?.message || 'Failed to submit review.');
    } finally {
      setIsSubmittingReview(false);
    }
  };

  const renderStars = (rating) => (
    <div className="flex items-center gap-1">
      {[1, 2, 3, 4, 5].map((star) => (
        <Star
          key={star}
          size={16}
          className={star <= rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}
        />
      ))}
    </div>
  );

  const averageRating = reviews.length > 0
    ? (reviews.reduce((sum, r) => sum + (r.rating || 5), 0) / reviews.length).toFixed(1)
    : 0;

  return (
    <div className="w-full mt-10 border-t border-gray-100 pt-8" dir="ltr">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h2 className="text-2xl font-black text-gray-800 flex items-center gap-3">
          <MessageSquare className="text-emerald-600" size={24} />
          Customer Reviews ({reviews.length})
        </h2>

        {isAuthenticated && (
          <button
            onClick={() => setShowReviewForm(!showReviewForm)}
            className="bg-emerald-600 text-white px-5 py-2.5 rounded-xl font-bold hover:bg-emerald-700 transition-all flex items-center gap-2 text-sm shadow-md shadow-emerald-500/20"
          >
            <Star size={18} />
            {showReviewForm ? 'Cancel' : 'Write a Review'}
          </button>
        )}
      </div>

      {/* Average Rating Banner */}
      <div className="bg-gradient-to-r from-emerald-50 to-teal-50/50 p-6 rounded-2xl border border-emerald-100 mb-8 flex items-center gap-6">
        <div>
          <div className="text-4xl font-black text-emerald-600">{averageRating}</div>
          <div className="text-xs text-gray-500 font-bold">out of 5 stars</div>
        </div>
        <div>
          {renderStars(Math.round(averageRating))}
          <p className="text-xs text-gray-500 mt-1">Based on {reviews.length} reviews</p>
        </div>
      </div>

      {/* Review Form */}
      {showReviewForm && (
        <div className="bg-white p-6 rounded-2xl border border-gray-200 mb-8 shadow-sm animate-fade-in-down">
          <h3 className="text-base font-bold mb-4 text-gray-800">Share your opinion on this product</h3>
          <form onSubmit={handleSubmitReview} className="space-y-4">
            <div>
              <label className="block text-xs font-bold mb-2 text-gray-600">Select Rating</label>
              <div className="flex items-center gap-2">
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    key={star}
                    type="button"
                    onClick={() => setFormData({ ...formData, rating: star })}
                    className="focus:outline-none transition-transform hover:scale-110"
                  >
                    <Star
                      size={28}
                      className={star <= formData.rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}
                    />
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold mb-2 text-gray-600">Your Comment (Optional)</label>
              <textarea
                maxLength={500}
                value={formData.comment}
                onChange={(e) => setFormData({ ...formData, comment: e.target.value })}
                placeholder="Write your feedback about product quality and delivery..."
                className="w-full p-4 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-emerald-500 outline-none resize-none text-sm"
                rows={3}
              />
            </div>

            <button
              type="submit"
              disabled={isSubmittingReview}
              className="bg-emerald-600 text-white px-6 py-3 rounded-xl font-bold hover:bg-emerald-700 transition-all flex items-center justify-center gap-2 disabled:opacity-70 text-sm"
            >
              {isSubmittingReview ? (
                <>
                  <Loader2 className="animate-spin" size={18} />
                  Submitting...
                </>
              ) : (
                <>
                  <Send size={18} />
                  Submit Review
                </>
              )}
            </button>
          </form>
        </div>
      )}

      {/* Reviews List */}
      <div className="space-y-4">
        {isLoading ? (
          <div className="flex justify-center p-8">
            <Loader2 className="animate-spin text-emerald-600" size={32} />
          </div>
        ) : reviews.length > 0 ? (
          reviews.map((review) => (
            <div key={review.id || Math.random()} className="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm">
              <div className="flex items-start justify-between mb-2">
                <div>
                  <h4 className="font-bold text-gray-800 text-sm">{review.customerName || 'Verified Customer'}</h4>
                  <span className="text-[10px] text-gray-400">
                    {review.createdAt ? new Date(review.createdAt).toLocaleDateString('en-US') : ''}
                  </span>
                </div>
                {renderStars(review.rating || 5)}
              </div>
              {review.comment && (
                <p className="text-gray-600 text-xs leading-relaxed mt-2">{review.comment}</p>
              )}
            </div>
          ))
        ) : (
          <div className="text-center p-8 bg-gray-50 rounded-2xl border border-gray-100">
            <p className="text-gray-500 text-sm">No reviews yet. Be the first to share your feedback!</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProductReviews;