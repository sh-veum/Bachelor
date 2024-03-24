<script setup lang="ts">
import { ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/lib/useAuth'

const email = ref('')
const password = ref('')
const attemptedRegister = ref(false)

const { register, isRegistered, registrationErrors } = useAuth()

const handleSubmit = async () => {
  attemptedRegister.value = true
  await register(email.value, password.value)
  // TODO: Redirect the user to a different page or give some type of feedback
}
</script>

<template>
  <!-- mt-[-24px] to counteract the mb-6 in app.vue -->
  <div class="flex items-center justify-center router-view mt-[-24px]">
    <div class="w-full max-w-md bg-zinc-50 rounded-lg shadow dark:border dark:border-zinc-700 py-2">
      <form @submit.prevent="handleSubmit" class="py-2 px-5">
        <div class="py-2">
          <Input v-model="email" type="email" placeholder="Email" />
        </div>
        <div class="py-2">
          <Input v-model="password" type="password" placeholder="Password" />
        </div>
        <Button type="submit" class="w-full mt-4">Register</Button>
      </form>
      <div v-if="attemptedRegister && isRegistered" class="px-5">
        <p class="text-green-500 font-bold">Registered user: {{ email }}</p>
      </div>
      <div v-if="attemptedRegister && !isRegistered" class="px-5 pb-2">
        <p class="text-red-500 font-bold">Failed registration</p>
        <ul>
          <li v-for="(errorMessages, index) in Object.values(registrationErrors)" :key="index">
            <p v-for="message in errorMessages" :key="message" class="text-red-500">
              - {{ message }}
            </p>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<style>
.router-view {
  /* Screen height - height of navigation bar */
  /* TODO: use a global variable for the supposed height of the navigation bar? */
  height: calc(100vh - (3rem + 1px));
}
</style>
