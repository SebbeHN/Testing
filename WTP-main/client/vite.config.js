import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
    server: {
      port: 3001,
      proxy: {
  
        "/api": "http://localhost:5000/"
  
      }
    },

    build: {
      outDir: '../server/wwwroot',
      emptyOutDir: true,
    }
   
    //proxy: {
      // '/api': {
      //   target: 'http://localhost:5000',
      //   changeOrigin: true,
      //   secure: false,
      //   rewrite: (path) => path.replace(/^\/api/, '/api')
      // }
})