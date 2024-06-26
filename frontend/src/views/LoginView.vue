<script setup lang="ts">
import { ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/lib/useAuth'
import { useToast } from '@/components/ui/toast/use-toast'

const email = ref('')
const password = ref('')
const attemptedLogin = ref(false)

const { isLoggedIn, login, logout, loginErrors } = useAuth()
const { toast } = useToast()

const handleLogin = async () => {
  attemptedLogin.value = true
  await login(email.value, password.value).then(() => {
    // If login is successful, show the toast notification
    if (isLoggedIn.value) {
      toast({
        description: 'Logged in successfully!',
        variant: 'success'
      })
    }
  })
}

const handleLogout = async () => {
  logout()
}
</script>

<template>
  <!-- mt-[-24px] to counteract the mb-6 in app.vue -->
  <div class="flex items-center justify-center router-view mt-[-24px]">
    <div class="w-full max-w-md bg-zinc-50 rounded-lg shadow dark:border dark:border-zinc-700 py-2">
      <form @submit.prevent="handleLogin" class="py-2 px-5">
        <div class="py-2">
          <Input v-model="email" type="email" placeholder="Email" />
        </div>
        <div class="py-2">
          <Input v-model="password" type="password" placeholder="Password" />
        </div>
        <Button type="submit" class="w-full mt-4">Login</Button>
      </form>
      <form @submit.prevent="handleLogout" class="py-2 px-5">
        <Button type="submit" class="w-full">Logout</Button>
      </form>
      <div v-if="attemptedLogin" class="px-5">
        <ul v-if="Object.keys(loginErrors).length > 0">
          <li v-for="(errorMessages, index) in Object.values(loginErrors)" :key="index">
            <p v-for="message in errorMessages" :key="message" class="text-red-500">
              - {{ message }}
            </p>
          </li>
        </ul>
      </div>
      <div class="text-center mt-4">
        <span>Don't have an account? </span>
        <router-link to="/register" class="text-blue-500 hover:underline"
          >Register here</router-link
        >
      </div>
    </div>
  </div>
</template>

<style>
.router-view {
  /* Screen height - height of navigation bar */
  /* TODO: use a global variable for the height of the navigation bar? */
  height: calc(100vh - (3rem + 1px));
}
</style>
