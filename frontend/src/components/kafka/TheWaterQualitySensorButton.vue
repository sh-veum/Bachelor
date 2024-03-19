<script setup lang="ts">
import { onMounted, ref, defineEmits, defineProps } from 'vue'
import axios from 'axios'
import { Button } from '@/components/ui/button'
import { userId } from '@/lib/useAuth'
import Switch from '@/components/ui/switch/Switch.vue'
import Label from '@/components/ui/label/Label.vue'

interface Sensor {
  sensorId: string
  active: boolean
}

interface WaterQualityLog {
  id: number
  timeStamp: string
  ph: number
  turbidity: number
  temperature: number
}

const sensorRunning = ref(false)
const isLoading = ref(true)
const fetchAllData = ref(false)

const props = defineProps({
  isLive: Boolean
})

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
  let data = {}

  // Determine the appropriate URL and payload based on the sensor's current state
  const url = sensorRunning.value
    ? 'http://localhost:8088/api/sensor/waterQuality/stopSensor'
    : 'http://localhost:8088/api/sensor/waterQuality/startSensor'

  // When starting the sensor, include the SendHistoricalData flag in the request body
  if (!sensorRunning.value) {
    data = {
      SendHistoricalData: fetchAllData.value
    }

    if (fetchAllData.value) {
      emit('clear-waterquality-logs')
    }
  }

  try {
    const response = await axios.post(url, data)
    sensorRunning.value = !sensorRunning.value

    if (!sensorRunning.value) {
      let logs: WaterQualityLog[] = response.data.map((log: WaterQualityLog) => {
        const date = new Date(log.timeStamp)
        const formattedTimeStamp = date.toISOString().replace(/T/, ' ').replace(/\..+/, '')
        return {
          ...log,
          timeStamp: formattedTimeStamp
        }
      })

      emit('waterquality-logs-updated', logs)
    }
  } catch (error) {
    console.error('Error toggling the sensor state:', error)
  } finally {
    isLoading.value = false
  }
}
const emit = defineEmits(['waterquality-logs-updated', 'clear-waterquality-logs'])

const handleSwitchChange = (value: boolean) => {
  fetchAllData.value = value
  console.log('Fetch all data:', fetchAllData.value)
}


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
    <div class="flex justify-center items-center gap-2">
      <Switch
        :checked="fetchAllData"
        @update:checked="handleSwitchChange"
        :disabled="!props.isLive"
        id="fetchAllData"
      />
      <Label for="fetchAllData" class="text-sm w-max">Fetch all data</Label>
    </div>
  </div>
</template>
