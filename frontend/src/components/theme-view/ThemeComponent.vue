<script setup lang="ts">
import { ref, defineProps } from 'vue'
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible'
import { Button } from '@/components/ui/button'
import { ChevronsUpDown } from 'lucide-vue-next'
import { ChevronsDown } from 'lucide-vue-next'
import { ChevronsUp } from 'lucide-vue-next'
import { MoreHorizontal } from 'lucide-vue-next'

defineProps<{
  theme: {
    name: string
    endpoints: string[]
  }
}>()

const isOpen = ref(true)
</script>

<template>
  <Collapsible v-model:open="isOpen" class="space-y-2">
    <div class="flex items-center justify-between space-x-4 px-4">
      <h4 class="text-sm py-3 font-semibold">{{ theme.name }}</h4>
      <div>
        <CollapsibleTrigger as-child>
          <Button variant="ghost" size="sm" class="w-9 p-0">
            <ChevronsDown v-if="isOpen == false" class="h-4 w-4" />
            <ChevronsUp v-if="isOpen == true" class="h-4 w-4" />
            <span class="sr-only">Toggle</span>
          </Button>
        </CollapsibleTrigger>
        <Button variant="ghost" size="sm" class="w-9 p-0">
          <MoreHorizontal class="h-4 w-4" />
        </Button>
        <span class="sr-only">More options</span>
      </div>
    </div>
    <CollapsibleContent class="space-y-2">
      <div
        v-for="endpoint in theme.endpoints"
        class="rounded-md border px-4 py-3 font-mono text-sm break-all"
      >
        {{ endpoint }}
      </div>
    </CollapsibleContent>
  </Collapsible>
</template>
