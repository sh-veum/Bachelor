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
  <div class="pl-5">
    <form @submit.prevent="handleSubmit">
      <Input v-model="email" type="email" placeholder="Email" />
      <Input v-model="password" type="password" placeholder="Password" />
      <Button type="submit">Register</Button>
    </form>
    <div v-if="attemptedRegister && isRegistered">
      <p class="text-green-500 font-bold">Registerd user: {{ email }}</p>
    </div>
    <div v-if="attemptedRegister && !isRegistered">
      <p class="text-red-500 font-bold">Failed registration</p>
    </div>
  </div>
</template>
