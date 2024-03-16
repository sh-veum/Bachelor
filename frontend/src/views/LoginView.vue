<script setup lang="ts">
import { ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/lib/useAuth'

const email = ref('')
const password = ref('')
const attemptedLogin = ref(false)

const { isLoggedIn, login, logout, isAdmin, loginErrors  } = useAuth()

const handleLogin = async () => {
  attemptedLogin.value = true
  await login(email.value, password.value)
}

const handleLogout = async () => {
  logout()
}
</script>

<template>
  <section>
    <div
      class="flex flex-col items-center justify-center px-6 py-8 mx-auto h-screen mt-[-65px] lg:py-0"
    >
      <div
        class="w-full max-w-md bg-zinc-50 rounded-lg shadow dark:border dark:border-zinc-700 mb-60 pb-2"
      >
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
        <div v-if="isLoggedIn" class="px-5">
          <p class="text-green-500 font-bold">Logged in</p>
        </div>
        <div v-else class="px-5">
          <p class="text-red-500 font-bold">No user logged in</p>
        </div>
        <div v-if="isAdmin" class="px-5">
          <p class="text-green-500 font-bold">Is admin</p>
        </div>
        <div v-else class="px-5">
          <p class="text-red-500 font-bold">Is not admin</p>
        </div>
        <div v-if="attemptedLogin" class="px-5">
          <ul v-if="Object.keys(loginErrors).length > 0">
            <li v-for="(errorMessages, index) in Object.values(loginErrors)" :key="index">
              <p v-for="message in errorMessages" :key="message" class="text-red-500">
                - {{ message }}
              </p>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </section>
</template>
