import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

import tailwind from 'tailwindcss'
import autoprefixer from 'autoprefixer'
import path from 'node:path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  css: {
    postcss: {
      plugins: [tailwind(), autoprefixer()]
    }
  },
  resolve: {
    alias: {
      //   '@': fileURLToPath(new URL('./src', import.meta.url))
      '@': path.resolve(__dirname, './src')
    }
  },
  server: {
    host: true,
    port: 80,
    watch: {
      usePolling: true
    }
  }
})
