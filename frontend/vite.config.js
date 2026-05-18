import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  test: {
    globals:     true,
    environment: 'jsdom',
    setupFiles:  './src/__tests__/setup.js',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
      exclude:  ['node_modules/', 'src/__tests__/']
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:62468',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
