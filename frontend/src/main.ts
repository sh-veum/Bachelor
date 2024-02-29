// import './assets/main.css'
import './assets/index.css'

import { createApp } from 'vue'
import { DefaultApolloClient } from '@vue/apollo-composable'
import { ApolloClient, ApolloLink, createHttpLink, InMemoryCache } from '@apollo/client/core'
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

// HTTP connection to GraphQL
const httpLink = createHttpLink({
  uri: 'http://localhost:8088/graphql/'
})

// Middleware for appending the auth token to the headers of GraphQL apollo requests
const authLink = new ApolloLink((operation, forward) => {
  const authToken = localStorage.getItem('authToken')
  operation.setContext({
    headers: {
      Authorization: authToken ? `Bearer ${authToken}` : ''
    }
  })

  return forward(operation)
})

// Cache implementation
const cache = new InMemoryCache()

const apolloClient = new ApolloClient({
  link: authLink.concat(httpLink), // Chain the links
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
