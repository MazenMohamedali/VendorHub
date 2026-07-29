import axios from 'axios';

// Connect directly to ASP.NET Core Web API backend
const getBaseURL = () => {
  const envUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5131';
  const cleanUrl = envUrl.replace(/\/+$/, '');
  return cleanUrl.endsWith('/api') ? cleanUrl : `${cleanUrl}/api`;
};

const axiosInstance = axios.create({
  baseURL: getBaseURL(),
});

axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      console.warn("Unauthorized access - clearing token");
      localStorage.removeItem('token');
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;