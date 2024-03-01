<script setup lang="ts">
import { ref } from 'vue'
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible'
import { Button } from '@/components/ui/button'
import { ChevronsUpDown, Pencil, SquarePen, Trash, Trash2 } from 'lucide-vue-next'
import { ChevronsDown } from 'lucide-vue-next'
import { ChevronsUp } from 'lucide-vue-next'
import { MoreHorizontal } from 'lucide-vue-next'
import axios from 'axios'

//TODO: use a shared interface for theme
const props = defineProps<{
  theme: {
    id: string
    themeName: string
    accessibleEndpoints: string[]
  }
}>()

const isOpen = ref(true)

const deleteTheme = async (theme: {
  id: string
  themeName: string
  accessibleEndpoints: string[]
}) => {
  try {
    await axios.delete(`http://localhost:8088/api/key/delete-theme?id=${theme.id}`, {})
  } catch (error) {
    console.error('Failed to delete theme:', error)
  }
}

const updateTheme = async (theme: {
  id: string
  themeName: string
  accessibleEndpoints: string[]
}) => {
  try {
    await axios.put('http://localhost:8088/api/key/update-theme', {
      data: theme
    })
  } catch (error) {
    console.error('Failed to update theme:', error)
  }
}

const handleDelete = () => {
  deleteTheme(props.theme)
}

const handleEdit = () => {
  console.log('editing theme')
}
</script>

<template>
  <Collapsible v-model:open="isOpen" class="space-y-2">
    <div class="flex items-center justify-between space-x-4 px-4">
      <h4 class="text-sm py-3 font-semibold">{{ theme.themeName }}</h4>
      <div>
        <CollapsibleTrigger as-child>
          <Button variant="ghost" size="sm" class="w-9 p-0">
            <ChevronsDown v-if="isOpen == false" class="h-4 w-4" />
            <ChevronsUp v-if="isOpen == true" class="h-4 w-4" />
            <span class="sr-only">Toggle</span>
          </Button>
        </CollapsibleTrigger>
        <Button variant="ghost" size="sm" class="w-9 p-0" @click="handleEdit">
          <Pencil class="h-4 w-4" />
        </Button>
        <span class="sr-only">Pencil</span>
        <Button variant="ghost" size="sm" class="w-9 p-0" @click="handleDelete">
          <Trash2 class="h-4 w-4 text-destructive" />
        </Button>
        <span class="sr-only">Trash</span>
      </div>
    </div>
    <CollapsibleContent class="space-y-2">
      <div
        v-for="endpoint in theme.accessibleEndpoints"
        class="rounded-md border px-4 py-3 font-mono text-sm break-all"
      >
        {{ endpoint }}
      </div>
    </CollapsibleContent>
  </Collapsible>
</template>
