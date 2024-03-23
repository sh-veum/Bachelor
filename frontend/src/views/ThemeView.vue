<script setup lang="ts">
import ThemeDialog from '@/components/rest/ThemeDialog.vue'
import ThemeComponent from '@/components/rest/ThemeComponent.vue'
import { Button } from '@/components/ui/button'

import { ref, onMounted } from 'vue'
import axios from 'axios'
import type { Theme } from '@/components/interfaces/RestSchema'

const themes = ref<Theme[]>([])
const editingTheme = ref<Theme | undefined>(undefined)
const isOpen = ref(false)

const fetchThemes = async () => {
  try {
    //TODO: should the url be more dynamic?
    const themesResponse = await axios.get('http://localhost:8088/api/rest/get-themes-by-user')
    themes.value = themesResponse.data
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const handleEdit = (theme: Theme) => {
  editingTheme.value = theme
  isOpen.value = true
}

const handleSubmit = () => {
  isOpen.value = false
  fetchThemes()
}

onMounted(fetchThemes)
</script>

<template>
  <ThemeDialog :theme="editingTheme" @submit="handleSubmit" v-model:open="isOpen" :isOpen="isOpen">
    <div class="flex">
      <Button @click="editingTheme = undefined" class="ml-auto mr-4">Create Theme</Button>
    </div>
  </ThemeDialog>

  <div class="flex justify-center">
    <div class="flex-row w-1/2">
      <ThemeComponent
        v-for="theme in themes"
        :theme="theme"
        :actions="true"
        @delete="fetchThemes"
        @edit="handleEdit"
        class="py-4"
        :key="theme.id"
      />
      <div v-if="themes.length === 0" class="text-center mt-4">No themes to show</div>
    </div>
  </div>
</template>
