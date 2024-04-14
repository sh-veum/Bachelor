<script setup lang="ts">
import { ref, toRef, watch, onMounted } from 'vue'
import axios from 'axios'
import { Button } from '@/components/ui/button'
import WebSocketService from '@/lib/WebSocketService'

interface Sensor {
  sensorId: string
  active: boolean
}

const sensorRunning = ref(false)
const isLoading = ref(true)
const fetchAllData = ref(false)

const props = defineProps({
  selectedTopicType: String,
  accessKey: String
})

const selectedTopicType = toRef(props, 'selectedTopicType')
const accessKey = toRef(props, 'accessKey')

const checkSensorStatus = async () => {
  console.log('Checking sensor status...')
  if (selectedTopicType.value === '') return

  try {
    const response = await axios.post(
      `http://localhost:8088/api/sensor/${selectedTopicType.value}/activeSensors`,
      { encryptedKey: accessKey.value }
    )

    const sensors = response.data
    // Check if any of the sensors' IDs match the userId
    // TODO: Get topic id and match with sensor id?
    sensorRunning.value = sensors.some((sensor: Sensor) => sensor.active)

    if (sensors.length === 0) {
      sensorRunning.value = false
    }
  } catch (error) {
    console.error('Failed to fetch sensor status:', error)
  } finally {
    console.log(
      'Sensor status checked',
      sensorRunning.value ? 'Sensor is running' : 'Sensor is not running'
    )
    isLoading.value = false // Loading is complete
  }
}

const toggleSensor = async () => {
  if (selectedTopicType.value === '') return

  console.log('Toggling sensor state...')

  isLoading.value = true
  let data = {}

  const sessionId = WebSocketService.sessionId

  // Determine the appropriate URL and payload based on the sensor's current state
  let url = `http://localhost:8088/api/sensor/${selectedTopicType.value}/`
  if (sensorRunning.value) {
    url += 'stopSensor'
    data = {
      encryptedKey: accessKey.value
    }
  } else {
    url += 'startSensor'
    data = {
      encryptedKey: accessKey.value
    }
    if (fetchAllData.value) {
      emit('clear-logs')
    }
    url += `?SendHistoricalData=${encodeURIComponent(fetchAllData.value)}&SessionId=${encodeURIComponent(sessionId)}`
    console.log('URL:', url)
  }

  try {
    await axios.post(url, data)
    sensorRunning.value = !sensorRunning.value
  } catch (error) {
    console.error('Error toggling the sensor state:', error)
  } finally {
    isLoading.value = false
  }
}

const emit = defineEmits(['water-logs-updated', 'boat-logs-updated', 'clear-logs'])

watch(selectedTopicType, () => {
  checkSensorStatus()
})

onMounted(() => {
  checkSensorStatus()
})
</script>

<template>
  <div class="flex justify-center items-center gap-4">
    <Button
      :class="isLoading ? 'bg-gray-500' : sensorRunning ? 'bg-red-500' : 'bg-green-500'"
      :disabled="isLoading"
      @click="toggleSensor"
    >
      {{ isLoading ? 'Loading...' : sensorRunning ? 'Stop Sensor' : 'Start Sensor' }}
    </Button>
  </div>
</template>
