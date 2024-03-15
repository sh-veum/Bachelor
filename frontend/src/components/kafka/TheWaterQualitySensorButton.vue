<script setup lang="ts">
import { onMounted, ref } from 'vue'
import axios from 'axios'
import { Button } from '@/components/ui/button'
import { userId } from '@/lib/useAuth'

interface Sensor {
  sensorId: string
  active: boolean
}

const sensorRunning = ref(false)
const isLoading = ref(true)

const checkSensorStatus = async () => {
  try {
    const response = await axios.get('http://localhost:8088/api/sensor/activeSensors')
    const sensors = response.data
    // Check if any of the sensors' IDs match the userId
    sensorRunning.value = sensors.some(
      (sensor: Sensor) => sensor.sensorId === userId.value && sensor.active
    )
  } catch (error) {
    console.error('Failed to fetch sensor status:', error)
  } finally {
    isLoading.value = false // Loading is complete
  }
}

const toggleSensor = async () => {
  isLoading.value = true
  const url = sensorRunning.value
    ? 'http://localhost:8088/api/sensor/waterQuality/stopSensor'
    : 'http://localhost:8088/api/sensor/waterQuality/startSensor'

  try {
    await axios.post(url)
    sensorRunning.value = !sensorRunning.value
  } catch (error) {
    console.error('Error toggling the sensor state:', error)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  checkSensorStatus()
})
</script>

<template>
  <div>
    <Button
      :class="isLoading ? 'bg-gray-500' : sensorRunning ? 'bg-red-500' : 'bg-green-500'"
      :disabled="isLoading"
      @click="toggleSensor"
    >
      {{ isLoading ? 'Loading...' : sensorRunning ? 'Stop Sensor' : 'Start Sensor' }}
    </Button>
  </div>
</template>
