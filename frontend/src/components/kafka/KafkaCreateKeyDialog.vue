<script setup lang="ts">
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Checkbox } from '@/components/ui/checkbox'
import { computed, ref } from 'vue'
import { createKafkaKey, getAvailableKafkatopics } from '@/lib/kafka'
import { Copy, Check } from 'lucide-vue-next'
import { Label } from '@/components/ui/label'

const keyName = ref('')
const availableTopics = ref<string[]>([])
const selectedTopics = ref<Set<string>>(new Set())
const keyNameErrorMessage = ref('')
const fieldsErrorMessage = ref('')

const isKeyCreatedDialogOpen = ref(false)
const createdKey = ref('')
const copySuccess = ref(false)

const hasSelectedTopics = computed(() => selectedTopics.value.size > 0)

const emit = defineEmits(['submit'])

const fetchData = async () => {
  try {
    const topics = await getAvailableKafkatopics()
    availableTopics.value = topics
  } catch (error) {
    console.error('Error fetching Kafka topics:', error)
  }
}

const toggleSelectedTopic = (topic: string) => {
  if (selectedTopics.value.has(topic)) {
    selectedTopics.value.delete(topic)
  } else {
    selectedTopics.value.add(topic)
  }
}

const createKey = async () => {
  keyNameErrorMessage.value = ''
  fieldsErrorMessage.value = ''

  if (!keyName.value.trim()) {
    keyNameErrorMessage.value = 'Key name is required.'
    return
  }
  if (!hasSelectedTopics.value) {
    fieldsErrorMessage.value = 'At least one topic must be selected.'
    return
  }

  try {
    const createdKeyData = await createKafkaKey(keyName.value, Array.from(selectedTopics.value))
    createdKey.value = createdKeyData.encryptedKey
    isKeyCreatedDialogOpen.value = true
    emit('submit')
  } catch (error) {
    console.error('Error creating Kafka key:', error)
  }
}

const copyLink = () => {
  navigator.clipboard.writeText(createdKey.value)
  copySuccess.value = true
  setTimeout(() => {
    copySuccess.value = false
  }, 1000)
}

const closeKeyCreatedDialog = () => {
  isKeyCreatedDialogOpen.value = false
}
</script>

<template>
  <Dialog>
    <DialogTrigger as-child>
      <Button @click="fetchData">Create Key</Button>
    </DialogTrigger>
    <DialogContent class="grid-rows-[auto_minmax(0,1fr)_auto] p-0 max-h-[90dvh]">
      <DialogHeader class="mx-6 mt-6">
        <DialogTitle>Create Key</DialogTitle>
        <DialogDescription> Create a new key to access the Kafka Topics.</DialogDescription>
      </DialogHeader>
      <div class="grid gap-2 my-2 overflow-y-auto px-6">
        <div class="flex flex-col justify-between">
          <Input v-model="keyName" placeholder="Write key name here" class="my-2" />
          <p v-if="keyNameErrorMessage" class="text-red-500">{{ keyNameErrorMessage }}</p>
          <p class="text-2xl font-bold mt-2">Available Topics</p>
          <hr class="my-2" />
          <ul>
            <li v-for="(topic, index) in availableTopics" :key="index">
              <div class="flex items-center space-x-3 h-6">
                <Checkbox @update:checked="() => toggleSelectedTopic(topic)" />
                <p class="font-normal">{{ topic }}</p>
                <hr class="my-2" />
              </div>
            </li>
          </ul>
          <p v-if="fieldsErrorMessage" class="text-red-500">{{ fieldsErrorMessage }}</p>
        </div>
      </div>
      <DialogFooter class="p-6 pt-0 sm:justify-start">
        <DialogClose as-child>
          <Button
            class="mt-2"
            @click="createKey"
            type="submit"
            v-bind:disabled="!keyName || !hasSelectedTopics"
            >Create Key</Button
          >
        </DialogClose>
      </DialogFooter>
    </DialogContent>
  </Dialog>
  <Dialog v-model:open="isKeyCreatedDialogOpen">
    <DialogContent class="p-4">
      <DialogHeader>
        <DialogTitle class="text-center">Key created successfully!</DialogTitle>
        <DialogDescription class="text-red-500 font-bold italic text-center">
          Store the key, you won't be able to see it again when you close the dialog
        </DialogDescription>
      </DialogHeader>
      <div class="flex items-center space-x-2">
        <div class="grid flex-1 gap-2">
          <Label for="link" class="sr-only"> Link </Label>
          <Input id="link" :model-value="createdKey" readonly />
        </div>
        <Button type="submit" size="sm" class="px-3">
          <span class="sr-only">Copy</span>
          <div v-if="copySuccess">
            <Check class="w-4 h-4 text-green-500" />
          </div>
          <div v-else>
            <Copy @click="copyLink" class="w-4 h-4" />
          </div>
        </Button>
      </div>
      <DialogFooter class="mt-4">
        <Button @click="closeKeyCreatedDialog">Close</Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
