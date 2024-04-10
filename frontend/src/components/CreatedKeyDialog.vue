<script setup lang="ts">
import { Button } from '@/components/ui/button'

import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import Label from '@/components/ui/label/Label.vue'
import { Check, Copy } from 'lucide-vue-next'
import { ref } from 'vue'

const props = defineProps<{
  encryptedKey: string
}>()

const isOpen = defineModel('isOpen', {
  type: Boolean,
  default: false
})

const copySuccess = ref(false)

const copyLink = () => {
  navigator.clipboard.writeText(props.encryptedKey)
  copySuccess.value = true
  setTimeout(() => {
    copySuccess.value = false
  }, 1000)
}
</script>

<template>
  <Dialog v-model:open="isOpen">
    <!-- Prevent user from closing dialog by clicking outside of it -->
    <DialogContent
      @interact-outside="
        (event) => {
          return event.preventDefault()
        }
      "
      class="sm:max-w-md"
    >
      <DialogHeader>
        <DialogTitle class="text-center">Key created successfully!</DialogTitle>
        <DialogDescription class="text-red-500 font-bold italic text-center">
          Store the key, you won't be able to see it again when you close the dialog
        </DialogDescription>
      </DialogHeader>
      <div class="flex items-center space-x-2">
        <div class="grid flex-1 gap-2">
          <Label for="link" class="sr-only"> Link </Label>
          <Input id="link" :model-value="encryptedKey" readonly />
        </div>
        <Button @click="copyLink" type="submit" size="sm" class="px-3">
          <span class="sr-only">Copy</span>
          <div v-if="copySuccess">
            <Check class="w-4 h-4 text-green-500" />
          </div>
          <div v-else>
            <Copy class="w-4 h-4" />
          </div>
        </Button>
      </div>
      <DialogFooter class="sm:justify-start">
        <DialogClose as-child>
          <Button type="button" variant="secondary"> Close </Button>
        </DialogClose>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
