<script setup lang="ts">
import { ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAuth } from '@/lib/useAuth'

const email = ref('')
const password = ref('')
const attemptedRegister = ref(false)

const { register, isRegistered } = useAuth()

const handleSubmit = async () => {
  attemptedRegister.value = true
  await register(email.value, password.value)
}
</script>

<template>
  <section>
    <div class="flex flex-col items-center justify-center px-6 py-8 mx-auto md:h-screen lg:py-0">
      <div class="w-full max-w-md bg-zinc-50 rounded-lg shadow dark:border dark:border-gray-700 mb-60 pb-2">
        <form @submit.prevent="handleSubmit" class="py-2 px-5">
          <div class="py-2">
            <Input v-model="email" type="email" placeholder="Email" />
          </div>
          <div class="py-2">
            <Input v-model="password" type="password" placeholder="Password" />
          </div>
          <Button type="submit" class="w-full mt-4 bg-cyan-900">Register</Button>
        </form>
        <div v-if="attemptedRegister && isRegistered" class="px-5">
          <p class="text-green-500 font-bold">Registered user: {{ email }}</p>
        </div>
        <div v-if="attemptedRegister && !isRegistered" class="px-5 pb-2">
          <p class="text-red-500 font-bold">Failed registration</p>
        </div>
      </div>
    </div>
  </section>
</template>
