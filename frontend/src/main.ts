// import './assets/main.css'
import './assets/index.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import axios from 'axios'

// Axios interceptor to add the bearer token to every request
axios.interceptors.request.use(
  (config) => {
    const authToken = localStorage.getItem('authToken')
    if (authToken) {
      config.headers.Authorization = `Bearer ${authToken}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

const app = createApp(App)

app.use(router)

app.mount('#app')
