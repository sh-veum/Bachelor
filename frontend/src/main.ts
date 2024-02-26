// import './assets/main.css'
import './assets/index.css'

import { createApp } from 'vue'
import { DefaultApolloClient } from '@vue/apollo-composable'
import { ApolloClient, createHttpLink, InMemoryCache } from '@apollo/client/core'
import App from './App.vue'
import router from './router'
import axios from 'axios'
import { useAuth } from './lib/useAuth'

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

// HTTP connection to the API
const httpLink = createHttpLink({
  uri: 'http://localhost:8088/graphql/'
})

// Cache implementation
const cache = new InMemoryCache()

// Create the apollo client
const apolloClient = new ApolloClient({
  link: httpLink,
  cache
})

const app = createApp(App)

const { refreshUserCredentials } = useAuth()
refreshUserCredentials().then(() => {
  app.use(router)
  app.provide(DefaultApolloClient, apolloClient)
  app.mount('#app')
})

export { apolloClient }
