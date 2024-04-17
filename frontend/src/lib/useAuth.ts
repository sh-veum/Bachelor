import axios from 'axios'
import { ref, computed } from 'vue'

const authToken = ref(localStorage.getItem('authToken'))
const refreshToken = ref(localStorage.getItem('refreshToken'))
const userRole = ref(localStorage.getItem('userRole'))
export const userId = ref(localStorage.getItem('userId'))

export function useAuth() {
  const isLoggedIn = computed(() => authToken.value !== null)
  const isRegistered = ref(false)
  const isAdmin = computed(() => userRole.value === 'ADMIN')
  const registrationErrors = ref({})
  const loginErrors = ref({})

  const login = async (email: string, password: string) => {
    try {
      const response = await axios.post(`${import.meta.env.VITE_VUE_APP_API_URL}/login`, {
        email,
        password
      })
      if (response.status === 200) {
        authToken.value = response.data.accessToken
        refreshToken.value = response.data.refreshToken
        if (authToken.value !== null) {
          localStorage.setItem('authToken', authToken.value)
        }
        if (refreshToken.value !== null) {
          localStorage.setItem('refreshToken', refreshToken.value)
        }
        loginErrors.value = {} // Reset login errors on successful login
        await fetchUserInfo()
      }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        if (error.response.status === 401) {
          loginErrors.value = { credentials: ['Email and password do not match.'] }
        } else if (error.response.status === 400) {
          loginErrors.value = error.response.data.errors || {}
        } else {
          loginErrors.value = { UnexpectedError: ['An unexpected error occurred.'] }
        }
      } else {
        loginErrors.value = { NetworkError: ['Could not connect to the server.'] }
      }
    }
  }

  const logout = async () => {
    authToken.value = null
    refreshToken.value = null
    userRole.value = null
    userId.value = null
    localStorage.removeItem('authToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('userRole')
    localStorage.removeItem('hasRefreshedTokens')
    localStorage.removeItem('userId')
  }

  const register = async (email: string, password: string) => {
    try {
      const response = await axios.post(`${import.meta.env.VITE_VUE_APP_API_URL}/register`, {
        email,
        password
      })
      if (response.status === 200) {
        isRegistered.value = true
      }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 400) {
        isRegistered.value = false
        registrationErrors.value = error.response.data.errors || {}
      } else {
        registrationErrors.value = { UnexpectedError: ['An unexpected error occurred.'] }
      }
    }
  }

  const fetchUserInfo = async () => {
    if (authToken.value) {
      try {
        const response = await axios.get(
          `${import.meta.env.VITE_VUE_APP_API_URL}/api/user/userinfo`,
          {
            headers: { Authorization: `Bearer ${authToken.value}` }
          }
        )
        if (response.status === 200) {
          userId.value = response.data.id
          localStorage.setItem('userId', response.data.id)

          userRole.value = response.data.role
          localStorage.setItem('userRole', response.data.role)
        }
      } catch (error) {
        console.error('Error fetching user info', error)
        await logout()
      }
    }
  }

  const refreshUserCredentials = async () => {
    // const hasRefreshed = localStorage.getItem('hasRefreshedTokens')

    // if (!hasRefreshed) {}
    console.log('refreshing user credentials')
    const localRefreshToken = localStorage.getItem('refreshToken')
    if (localRefreshToken) {
      try {
        const response = await axios.post(`${import.meta.env.VITE_VUE_APP_API_URL}/refresh`, {
          refreshToken: localRefreshToken
        })
        if (response.status === 200) {
          authToken.value = response.data.accessToken
          refreshToken.value = response.data.refreshToken

          localStorage.setItem('authToken', authToken.value ?? '')
          localStorage.setItem('refreshToken', refreshToken.value ?? '')

          // localStorage.setItem('hasRefreshedTokens', 'true')

          await fetchUserInfo()
        }
      } catch (error) {
        console.error('Error refreshing token', error)
        await logout()
      }
    }

    // Only refresh on page load
    // TODO: Get message from backend that token is expired
    // localStorage.setItem('hasRefreshedTokens', 'true')
  }

  return {
    login,
    logout,
    register,
    fetchUserInfo,
    isLoggedIn,
    isRegistered,
    isAdmin,
    refreshUserCredentials,
    registrationErrors,
    loginErrors
  }
}
