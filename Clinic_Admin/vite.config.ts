import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  ssr: {
    noExternal: ['vuetify'],
  },
  optimizeDeps: {
    exclude: ['vuetify'],
  },
})