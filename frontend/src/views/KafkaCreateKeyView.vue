<script setup lang="ts">
import { fetchKafkaKeys, type KafkaKey } from '@/lib/kafka'
import { onMounted, ref } from 'vue'
import KafkaCreateKeyDialog from '@/components/kafka/KafkaCreateKeyDialog.vue'
import KafkaKeysOverview from '@/components/kafka/KafkaKeysOverview.vue'

const kafkaKeys = ref<KafkaKey[]>([])

const fetchData = async () => {
  try {
    kafkaKeys.value = await fetchKafkaKeys()
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const handleSubmit = () => {
  fetchData()
}

onMounted(fetchData)
</script>

<template>
  <div class="mx-6">
    <div class="my-6">
      <KafkaCreateKeyDialog @submit="handleSubmit" />
    </div>
    <div>
      <KafkaKeysOverview :kafkaKeys="kafkaKeys" />
    </div>
  </div>
</template>
