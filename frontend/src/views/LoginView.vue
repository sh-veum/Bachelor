<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/lib/useAuth'
import { TextArea } from '@/components/ui/textarea'

const email = ref('')
const password = ref('')

const testAPIResponseData = ref('')

const { isLoggedIn, login, logout } = useAuth()

const handleSubmit = async () => {
  await login(email.value, password.value)
}

const testApi = async () => {
  try {
    const token = localStorage.getItem('authToken')

    if (!token) {
      console.error('No token found')
      testAPIResponseData.value = 'No token found'
      return
    }

    const response = await axios.post(
      'http://localhost:8088/api/aquaculturelist/fishhealth/species',
      {
        encryptedKey: '' // TODO: Make it so the backend wont return 400 bad request with an empty body
      },
      {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      }
    )

    console.log('API Response:', response.data)
    testAPIResponseData.value = JSON.stringify(response.data, null, 2)
  } catch (error) {
    console.error('API Test error:', error)
    testAPIResponseData.value = ''
  }
}
</script>

<template>
  <div class="pl-5">
    <form @submit.prevent="handleSubmit">
      <Input v-model="email" type="email" placeholder="Email" />
      <Input v-model="password" type="password" placeholder="Password" />
      <Button type="submit">Login</Button>
    </form>
    <Button @click="logout">Logout</Button>
    <div v-if="isLoggedIn">
      <p class="text-green-500 font-bold">Logged in</p>
    </div>
    <div v-else>
      <p class="text-red-500 font-bold">No user logged in</p>
    </div>
    <Button @click="testApi">Test Api</Button>
    <TextArea
      v-model="testAPIResponseData"
      class="mt-2 h-72 w-2/4"
      placeholder="API response will appear here..."
    />
  </div>
</template>
