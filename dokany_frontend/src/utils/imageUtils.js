// const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44342';

// export const getImageUrl = (path, subfolder = 'Products') => {
//   if (!path) return 'https://placehold.co/600x400?text=No+Image';
  
//   // If it's already an absolute URL (starts with http/https), return it as-is
//   if (path.startsWith('http://') || path.startsWith('https://')) {
//     return path;
//   }
  
//   // If it starts with '/Images/', just prepend the base URL (avoid double slash)
//   if (path.startsWith('/Images/')) {
//     return `${API_BASE_URL}${path}`;
//   }
  
//   // Otherwise, treat as filename and build full path
//   const cleanPath = path.replace(/^\/+/, '');
//   return `${API_BASE_URL}/Images/${subfolder}/${cleanPath}`;
// };

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || ''; // No longer needed for images

export const getImageUrl = (path, subfolder = 'Products') => {
  if (!path) return 'https://placehold.co/600x400?text=No+Image';
  
  // If already an absolute URL, replace with proxied path
  if (path.startsWith('http://') || path.startsWith('https://')) {
    // Extract the relative part after the host
    const match = path.match(/\/Images\/(.+)$/);
    if (match) {
      return `/Images/${match[1]}`;
    }
    // Fallback: return as is (might still cause issues)
    return path;
  }
  
  // If relative path starting with /Images/, return as is
  if (path.startsWith('/Images/')) {
    return path;
  }
  
  // Otherwise, treat as filename
  const cleanPath = path.replace(/^\/+/, '');
  return `/Images/${subfolder}/${cleanPath}`;
};