<script setup lang="ts">
import ThemeDialog from '@/components/theme-view/ThemeDialog.vue'
import ThemeComponent from '@/components/theme-view/ThemeComponent.vue'
import { Button } from '@/components/ui/button'

import { ref, onMounted, watch } from 'vue'
import axios from 'axios'

//TODO: move interface and fetching to a separate file and resuse them in the other components
interface Theme {
  id: string
  themeName: string
  accessibleEndpoints: string[]
}

const themes = ref<Theme[]>([])
const editingTheme = ref<Theme | undefined>(undefined)
const isOpen = ref(false)

const fetchData = async () => {
  try {
    //TODO: should the url be more dynamic?
    const themesResponse = await axios.get('http://localhost:8088/api/key/get-themes-by-user')
    themes.value = themesResponse.data
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

onMounted(fetchData)

//TODO: should maybe update the themes directly in the frontend instead of refetching them?
watch(themes, fetchData)

const handleEdit = (theme: Theme) => {
  editingTheme.value = theme
  isOpen.value = true
}
</script>

<template>
  <!-- TODO: put v-model:open in the ThemeDialog Component so that we can close on submit -->
  <ThemeDialog :theme="editingTheme" v-model:open="isOpen">
    <div class="flex">
      <Button class="ml-auto mr-4">Create Theme</Button>
    </div>
  </ThemeDialog>

  <div class="flex justify-center">
    <div class="flex-row w-1/2">
      <ThemeComponent v-for="theme in themes" :theme="theme" @edit="handleEdit" class="py-4" />
    </div>
  </div>
</template>
