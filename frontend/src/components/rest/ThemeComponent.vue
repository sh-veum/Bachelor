<script setup lang="ts">
import { ref } from 'vue'
import { Collapsible, CollapsibleContent } from '@/components/ui/collapsible'
import { Button } from '@/components/ui/button'
import { Pencil, PowerOff, Trash2 } from 'lucide-vue-next'
import axios from 'axios'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '../ui/tooltip'
import type { UUID } from 'crypto'
import type { Theme } from '../interfaces/RestSchema'

const props = defineProps<{
  theme: Theme
  actions: boolean
}>()

const emit = defineEmits<{
  delete: [themeId: UUID]
  edit: [theme: Theme]
}>()

const isOpen = ref(true)

const deleteTheme = async (themeId: UUID) => {
  try {
    await axios.delete(
      `${import.meta.env.VITE_VUE_APP_API_URL}/api/rest/delete-theme?id=${themeId}`,
      {}
    )
  } catch (error) {
    console.error('Failed to delete theme:', error)
  }
}

const handleDelete = () => {
  deleteTheme(props.theme.id).then(() => emit('delete', props.theme.id))
}

const handleEdit = () => {
  emit('edit', props.theme)
}

const handleDeprecate = async (theme: Theme) => {
  // also updates the frontend meaning we do not need to refetch
  theme.isDeprecated = true
  try {
    //TODO: change backend to be able to use `${import.meta.env.VITE_VUE_APP_API_URL}/api/key/update-theme?id=${id}`?
    await axios.put(`${import.meta.env.VITE_VUE_APP_API_URL}/api/rest/update-theme`, theme)
  } catch (error) {
    console.error('Failed to deprecate theme:', error)
  }
}
</script>

<template>
  <Collapsible v-model:open="isOpen" class="space-y-2">
    <div class="flex items-center justify-between space-x-4 px-4">
      <h4 class="text-l py-3 font-semibold">
        {{ theme.themeName }}
        <span v-if="theme.isDeprecated" class="text-red-500">(deprecated)</span>
      </h4>
      <div v-if="actions">
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger>
              <Button variant="ghost" size="sm" class="w-9 p-0" @click="handleEdit">
                <Pencil class="h-4 w-4" />
              </Button>
              <span class="sr-only">Pencil</span>
            </TooltipTrigger>
            <TooltipContent>Edit</TooltipContent>
          </Tooltip>
        </TooltipProvider>

        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger>
              <Button
                v-if="!theme.isDeprecated"
                variant="ghost"
                size="sm"
                class="w-9 p-0"
                @click="handleDeprecate(theme)"
              >
                <PowerOff class="h-4 w-4 text-destructive" />
              </Button>
              <span v-if="!theme.isDeprecated" class="sr-only">Pencil</span>
            </TooltipTrigger>
            <TooltipContent>Deprecate</TooltipContent>
          </Tooltip>
        </TooltipProvider>

        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger>
              <Button variant="ghost" size="sm" class="w-9 p-0" @click="handleDelete">
                <Trash2 class="h-4 w-4 text-destructive" />
              </Button>
              <span class="sr-only">Trash</span>
            </TooltipTrigger>
            <TooltipContent>Delete</TooltipContent>
          </Tooltip>
        </TooltipProvider>
      </div>
    </div>
    <CollapsibleContent class="space-y-2">
      <div
        v-for="endpoint in theme.accessibleEndpoints"
        :key="endpoint"
        class="rounded-md border px-4 py-3 font-mono text-sm break-all"
      >
        {{ endpoint }}
      </div>
    </CollapsibleContent>
  </Collapsible>
</template>
