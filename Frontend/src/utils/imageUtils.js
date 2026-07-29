// File: src/utils/imageUtils.js
/**
 * ✅ COMPREHENSIVE IMAGE URL BUILDER
 * Handles products, categories, and any other images
 */

export const getImageUrl = (imgUrl, type = 'Products') => {
  if (!imgUrl) {
    return `https://placehold.co/400x400?text=No+${type}`;
  }

  if (imgUrl.startsWith('http://') || imgUrl.startsWith('https://') || imgUrl.startsWith('blob:') || imgUrl.startsWith('data:')) {
    return imgUrl;
  }

  let baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5131';
  baseUrl = baseUrl.replace(/\/$/, '');

  const cleanPath = imgUrl.replace(/^\/+/, '');

  if (cleanPath.toLowerCase().startsWith('images/')) {
    return `${baseUrl}/${cleanPath}`;
  }

  if (cleanPath.toLowerCase().startsWith(`${type.toLowerCase()}/`)) {
    return `${baseUrl}/Images/${cleanPath}`;
  }

  return `${baseUrl}/Images/${type}/${cleanPath}`;
};

/**
 * Get product image URL
 */
export const getProductImageUrl = (imgUrl) => {
  return getImageUrl(imgUrl, 'Products');
};

/**
 * Get category image URL
 */
export const getCategoryImageUrl = (imgUrl) => {
  return getImageUrl(imgUrl, 'Categories');
};

/**
 * Get profile/user image URL
 */
export const getUserImageUrl = (imgUrl) => {
  return getImageUrl(imgUrl, 'Users');
};

/**
 * Validate if URL is accessible
 */
export const isImageUrlValid = async (url) => {
  try {
    const response = await fetch(url, { method: 'HEAD' });
    return response.ok;
  } catch (error) {
    return false;
  }
};

/**
 * Get image with fallback
 */
export const getImageWithFallback = (primaryUrl, fallbackUrl = null) => {
  if (!primaryUrl) {
    return fallbackUrl || `https://placehold.co/400x400?text=No+Image`;
  }
  return primaryUrl;
};

/**
 * Convert file to base64 (for local preview)
 */
export const fileToBase64 = (file) => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
};

/**
 * Create image preview URL from File object
 */
export const createImagePreview = (file) => {
  return URL.createObjectURL(file);
};

/**
 * Cleanup image preview URL
 */
export const revokeImagePreview = (url) => {
  if (url && url.startsWith('blob:')) {
    URL.revokeObjectURL(url);
  }
};