<script setup lang="ts">
import { RouterView } from 'vue-router'
import TheNavigation from './components/TheNavigation.vue'
import { Toaster } from '@/components/ui/toast'
</script>

<template>
  <TheNavigation class="mb-6 sticky top-0 left-0 right-0 z-20" />

  <RouterView />

  <Toaster />
</template>
