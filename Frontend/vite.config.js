import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const target = env.VITE_API_BASE_URL || 'http://localhost:5131';

  return {
    plugins: [react()],
    server: {
      proxy: {
        '/api': {
          target: target,
          changeOrigin: true,
          secure: false,
        },
        '/notificationHub': {
          target: target,
          ws: true,
          changeOrigin: true,
          secure: false,
        },
        '/Images': {
          target: target,
          changeOrigin: true,
          secure: false,
        }
      }
    }
  };
});