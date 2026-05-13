// Central configuration for image base URL
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:44342';

/**
 * Returns a full URL for an image.
 * - If the path is already absolute (starts with http:// or https://), returns it as‑is.
 * - Otherwise, prepends the API base URL and appropriate subfolder.
 * @param {string} path - image filename or relative path
 * @param {string} subfolder - 'Products' or 'Categories'
 */
export const getImageUrl = (path, subfolder = 'Products') => {
  if (!path) return 'https://placehold.co/600x400?text=No+Image';
  if (path.startsWith('http://') || path.startsWith('https://')) return path;
  // Remove any leading slash to avoid double slashes
  const cleanPath = path.replace(/^\/+/, '');
  return `${API_BASE_URL}/Images/${subfolder}/${cleanPath}`;
};