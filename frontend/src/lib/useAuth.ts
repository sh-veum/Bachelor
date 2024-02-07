import axios from 'axios'
import { ref, computed } from 'vue'

const authToken = ref(localStorage.getItem('authToken'))

export function useAuth() {
  const isLoggedIn = computed(() => authToken.value !== null)

  const login = async (email: string, password: string) => {
    // Example login logic
    const response = await axios.post('http://localhost:8088/login', { email, password })
    if (response.status === 200) {
      authToken.value = response.data.accessToken
      localStorage.setItem('authToken', authToken.value as string) // Use type assertion to ensure value is always a string
    }
  }

  const logout = () => {
    authToken.value = null
    localStorage.removeItem('authToken')
  }

  return { isLoggedIn, login, logout }
}
