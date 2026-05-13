// import { defineConfig } from 'vite'
// import react from '@vitejs/plugin-react'

// // https://vite.dev/config/
// export default defineConfig({
//   plugins: [react()],
//   server: {
//     proxy: {
//       '/api': {
//         target: 'https://localhost:44342',   // or http if HTTPS not needed
//         changeOrigin: true,
//         secure: false,  // ignore self-signed cert in dev
//       }
//     }
//   }
// })

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:44342',
        changeOrigin: true,
        secure: false,
      },
      '/Images': {               // ← Add this block
        target: 'https://localhost:44342',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})