<script setup lang="ts">
import CreateThemeDialog from '@/components/theme-view/CreateThemeDialog.vue'
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

const fetchData = async () => {
  try {
    //TODO: find a way to make the url more dynamic?
    const themesResponse = await axios.get('http://localhost:8088/api/key/get-themes-by-user')
    themes.value = themesResponse.data
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const updateThemes = () => {
  fetchData()
}

onMounted(fetchData)

//TODO: should maybe update the themes directly in the frontend instead of refetching them?
watch(themes, updateThemes)
</script>

<template>
  <CreateThemeDialog>
    <div class="flex">
      <Button class="ml-auto mr-4">Create Theme</Button>
    </div>
  </CreateThemeDialog>

  <div class="flex justify-center">
    <div class="flex-row w-1/2">
      <ThemeComponent v-for="theme in themes" :theme="theme" class="py-4" />
    </div>
  </div>
</template>
