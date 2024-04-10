<script setup lang="ts">
import ThemeComponent from '@/components/rest/ThemeComponent.vue'
import { Collapsible, CollapsibleContent } from '@/components/ui/collapsible'
import { ChevronDown, ChevronUp } from 'lucide-vue-next'
import { defineProps } from 'vue'
import type { Key } from '../interfaces/RestSchema'

defineProps<{
  apiKey: Key
}>()

const isOpen = defineModel('isOpen', {
  type: Boolean,
  default: false
})
</script>

<template>
  <Collapsible v-model:open="isOpen" class="space-y-2">
    <div class="w-full p-0">
      <div
        class="flex items-center font-medium space-x-4 justify-between w-full group-hover:underline"
      >
        <h4 v-if="apiKey.themes.length > 1" class="text-sm py-3">
          {{ apiKey.themes.length }} themes
        </h4>
        <h4 v-else class="text-sm py-3">{{ apiKey.themes.length }} theme</h4>
        <ChevronDown v-if="isOpen == false" class="h-4 w-4" />
        <ChevronUp v-if="isOpen == true" class="h-4 w-4" />
        <span class="sr-only">Toggle</span>
      </div>
    </div>

    <CollapsibleContent class="space-y-2">
      <div v-for="theme in apiKey.themes" :key="theme.id" class="py-3 font-mono text-sm">
        <ThemeComponent :actions="false" :theme="theme" />
      </div>
    </CollapsibleContent>
  </Collapsible>
</template>
