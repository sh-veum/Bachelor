<script setup lang="ts">
import { ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/lib/useAuth'

const email = ref('')
const password = ref('')

const { isLoggedIn, login, logout, isAdmin } = useAuth()

const handleLogin = async () => {
  await login(email.value, password.value)
}

const handleLogout = async () => {
  logout()
}
</script>

<template>
  <div class="pl-5">
    <form @submit.prevent="handleLogin">
      <Input v-model="email" type="email" placeholder="Email" />
      <Input v-model="password" type="password" placeholder="Password" />
      <Button type="submit">Login</Button>
    </form>
    <form @submit.prevent="handleLogout">
      <Button type="submit">Logout</Button>
    </form>
    <div v-if="isLoggedIn">
      <p class="text-green-500 font-bold">Logged in</p>
    </div>
    <div v-else>
      <p class="text-red-500 font-bold">No user logged in</p>
    </div>
    <div v-if="isAdmin">
      <p class="text-green-500 font-bold">Is admin</p>
    </div>
    <div v-else>
      <p class="text-red-500 font-bold">Is not admin</p>
    </div>
  </div>
</template>
