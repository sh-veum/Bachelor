<script setup lang="ts">
import { ref } from 'vue'
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible'
import { Button } from '@/components/ui/button'
import { ChevronsUpDown, Pencil, PowerOff, SquarePen, Trash, Trash2 } from 'lucide-vue-next'
import { ChevronsDown } from 'lucide-vue-next'
import { ChevronsUp } from 'lucide-vue-next'
import { MoreHorizontal } from 'lucide-vue-next'
import axios from 'axios'
import Separator from '../ui/separator/Separator.vue'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '../ui/tooltip'

interface Theme {
  id: string
  themeName: string
  accessibleEndpoints: string[]
  isDeprecated: boolean
}

//TODO: use a shared interface for theme
const props = defineProps<{
  theme: Theme
}>()

const emit = defineEmits(['edit', 'delete'])

const isOpen = ref(true)

const deleteTheme = async (themeId: string) => {
  try {
    await axios.delete(`http://localhost:8088/api/rest/delete-theme?id=${themeId}`, {})
  } catch (error) {
    console.error('Failed to delete theme:', error)
  }
}

const handleDelete = () => {
  deleteTheme(props.theme.id).then(() => emit('delete'))
}

const handleEdit = () => {
  emit('edit', props.theme)
}

const handleDeprecate = async (theme: Theme) => {
  // also updates the frontend meaning we do not need to refetch
  theme.isDeprecated = true
  try {
    //TODO: change backend to be able to use `http://localhost:8088/api/key/update-theme?id=${id}`?
    await axios.put('http://localhost:8088/api/rest/update-theme', theme)
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
      <div>
        <CollapsibleTrigger as-child>
          <Button variant="ghost" size="sm" class="w-9 p-0">
            <ChevronsDown v-if="isOpen == false" class="h-4 w-4" />
            <ChevronsUp v-if="isOpen == true" class="h-4 w-4" />
            <span class="sr-only">Toggle</span>
          </Button>
        </CollapsibleTrigger>

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
    <!-- <Separator /> -->
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
