import axios from 'axios'
import { ref, computed } from 'vue'

const authToken = ref(localStorage.getItem('authToken'))
const refreshToken = ref(localStorage.getItem('refreshToken'))
const userRole = ref<string | null>(null)

export function useAuth() {
  const isLoggedIn = computed(() => authToken.value !== null)
  const isRegistered = ref(false)
  const isAdmin = computed(
    () => userRole.value === 'ADMIN' || localStorage.getItem('userRole') === 'ADMIN'
  )

  const login = async (email: string, password: string) => {
    const response = await axios.post('http://localhost:8088/login', { email, password })
    if (response.status === 200) {
      authToken.value = response.data.accessToken
      refreshToken.value = response.data.refreshToken
      if (authToken.value !== null) {
        localStorage.setItem('authToken', authToken.value)
      }
      if (refreshToken.value !== null) {
        localStorage.setItem('refreshToken', refreshToken.value)
      }
      await fetchUserInfo()
    }
  }

  const logout = async () => {
    authToken.value = null
    refreshToken.value = null
    userRole.value = null
    localStorage.removeItem('authToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('userRole')
    localStorage.removeItem('hasRefreshedTokens')
  }

  const register = async (email: string, password: string) => {
    // Example login logic
    const response = await axios.post('http://localhost:8088/register', { email, password })
    if (response.status === 200) {
      isRegistered.value = true
    } else {
      isRegistered.value = false
    }
  }

  const fetchUserInfo = async () => {
    if (authToken.value) {
      try {
        const response = await axios.get('http://localhost:8088/api/user/userinfo', {
          headers: { Authorization: `Bearer ${authToken.value}` }
        })
        if (response.status === 200) {
          userRole.value = response.data.role
          localStorage.setItem('userRole', response.data.role)
        }
      } catch (error) {
        console.error('Error fetching user info', error)
        await logout()
      }
    }
  }

  const refreshTokenFunc = async () => {
    const hasRefreshed = localStorage.getItem('hasRefreshedTokens')

    if (!hasRefreshed) {
      const localRefreshToken = localStorage.getItem('refreshToken')
      if (localRefreshToken) {
        try {
          const response = await axios.post('http://localhost:8088/refresh', {
            refreshToken: localRefreshToken
          })
          if (response.status === 200) {
            authToken.value = response.data.accessToken
            refreshToken.value = response.data.refreshToken

            localStorage.setItem('authToken', authToken.value ?? '')
            localStorage.setItem('refreshToken', refreshToken.value ?? '')

            localStorage.setItem('hasRefreshedTokens', 'true')

            await fetchUserInfo()
          }
        } catch (error) {
          console.error('Error refreshing token', error)
          await logout()
        }
      }
    }
  }

  return {
    login,
    logout,
    register,
    fetchUserInfo,
    isLoggedIn,
    isRegistered,
    isAdmin,
    refreshTokenFunc
  }
}
