<script setup lang="ts">
import { ref, defineProps } from 'vue'
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible'
import { Button } from '@/components/ui/button'
import { ChevronsUpDown } from 'lucide-vue-next'
import { ChevronsDown } from 'lucide-vue-next'
import { ChevronsUp } from 'lucide-vue-next'
import ThemeComponent from './theme-view/ThemeComponent.vue'

interface Theme {
  id: string
  themeName: string
  accessibleEndpoints: string[]
  isDeprecated: boolean
}

interface Key {
  id: string
  keyName: string
  createdBy: string
  expiresIn: number
  isEnabled: boolean
  themes: Theme[]
}

defineProps<{
  apiKey: Key
}>()

const isOpen = ref(false)
</script>

<template>
  <Collapsible v-model:open="isOpen" class="space-y-2">
    <CollapsibleTrigger as-child>
      <Button variant="ghost" size="sm" class="w-full p-0 hover:bg-zinc-50 hover:underline">
        <div class="flex items-center space-x-4 justify-between w-full">
          <h4 v-if="apiKey.themes.length > 1" class="text-sm py-3">
            {{ apiKey.themes.length }} themes
          </h4>
          <h4 v-else class="text-sm py-3">{{ apiKey.themes.length }} theme</h4>
          <ChevronsDown v-if="isOpen == false" class="h-4 w-4" />
          <ChevronsUp v-if="isOpen == true" class="h-4 w-4" />
          <span class="sr-only">Toggle</span>
        </div>
      </Button>
    </CollapsibleTrigger>

    <CollapsibleContent class="space-y-2">
      <div v-for="theme in apiKey.themes" :key="theme.id" class="py-3 font-mono text-sm">
        <ThemeComponent :actions="false" :theme="theme" />
      </div>
    </CollapsibleContent>
  </Collapsible>
</template>
