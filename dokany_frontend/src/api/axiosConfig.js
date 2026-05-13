// import axios from 'axios';

// const baseURL = '/api';

// const axiosInstance = axios.create({
//   baseURL: baseURL,
// });

// // Request interceptor (existing)
// axiosInstance.interceptors.request.use(
//   (config) => {
//     const token = localStorage.getItem('token');
//     if (token) {
//       config.headers['Authorization'] = `Bearer ${token}`;
//     }
//     return config;
//   },
//   (error) => Promise.reject(error)
// );

// // ✅ Response interceptor to handle 401/403
// axiosInstance.interceptors.response.use(
//   (response) => response,
//   (error) => {
//     if (error.response?.status === 401 || error.response?.status === 403) {
//       // Token expired or insufficient permissions
//       localStorage.removeItem('token');
//       window.location.href = '/login';
//     }
//     return Promise.reject(error);
//   }
// );

// export default axiosInstance;

import axios from 'axios';

const axiosInstance = axios.create({
  baseURL: '/api',   // ← must be '/api'
});

axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    // Debug: log the final URL
    console.log('Request URL:', config.baseURL + config.url);
    return config;
  },
  (error) => Promise.reject(error)
);

export default axiosInstance;