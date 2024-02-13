<script setup lang="ts">
import { ref } from 'vue'
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible'
import { ChevronsUpDown } from 'lucide-vue-next'
import { ChevronsDown } from 'lucide-vue-next'
import { ChevronsUp } from 'lucide-vue-next'

defineProps<{
  msg: string
  // key: { name: string; themes: string[] }
}>()

const key = { name: 'Key 1', themes: ['Theme 1', 'Theme 2', 'Theme 3'] }

const isOpen = ref(false)
</script>

<template>
  <Collapsible v-model:open="isOpen" class="w-[350px] space-y-2">
    <div class="flex items-center justify-between space-x-4 px-4">
      <h4 class="text-sm font-semibold px-4 py-3">{{ key.themes.length }} themes</h4>
      <CollapsibleTrigger as-child>
        <Button variant="ghost" size="sm" class="w-9 p-0">
          <ChevronsDown v-if="isOpen == false" class="h-4 w-4" />
          <ChevronsUp v-if="isOpen == true" class="h-4 w-4" />
          <span class="sr-only">Toggle</span>
        </Button>
      </CollapsibleTrigger>
    </div>
    <CollapsibleContent v-if="key.themes.length > 1" class="space-y-2">
      <div v-for="theme in key.themes" class="rounded-md border px-4 py-3 font-mono text-sm">
        {{ theme }}
      </div>
    </CollapsibleContent>
  </Collapsible>
</template>
