const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44342';

export const getImageUrl = (path, subfolder = 'Products') => {
  if (!path) return 'https://placehold.co/600x400?text=No+Image';
  // If it's already an absolute URL (starts with http), return as is
  if (path.startsWith('http://') || path.startsWith('https://')) return path;
  // Remove leading slash to avoid double slashes
  const cleanPath = path.replace(/^\/+/, '');
  return `${API_BASE_URL}/Images/${subfolder}/${cleanPath}`;
};