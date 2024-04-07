<script setup lang="ts">
import TheKafkaKeyTest from '@/components/kafka/TheKafkaKeyTest.vue'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { ref } from 'vue'
import { Lock, RotateCcw } from 'lucide-vue-next'
import { subscribeToAllTopics } from '@/lib/kafka'
import WebSocketService from '@/lib/WebSocketService'

const accessKey = ref(
  'PvYcOZfRhm/I5ApAq4j7n8HHM6nwlKi4WaYR7cX41DUZoNuOOcxpZGSO5eKMQ38MGGW2euikt7CjbLtxbkp8nQ=='
)
const isLocked = ref(false)

const toggleLock = () => {
  isLocked.value = !isLocked.value
  if (isLocked.value) {
    subscribeToAllTopics(accessKey.value)
  } else {
    WebSocketService.disconnect()
  }
}
</script>

<template>
  <div class="mx-6">
    <div class="flex flex-row items-center gap-2">
      <Input
        v-model="accessKey"
        :disabled="isLocked"
        placeholder="Place access key here"
        class="max-w-[800px]"
      />
      <Button @click="toggleLock">
        <component :is="isLocked ? RotateCcw : Lock" color="#ffffff" />
      </Button>
      <p class="italic text-sm text-zinc-500">(lock in access key)</p>
    </div>
    <TheKafkaKeyTest v-if="isLocked" class="mt-2" :accessKey="accessKey" />
  </div>
</template>
