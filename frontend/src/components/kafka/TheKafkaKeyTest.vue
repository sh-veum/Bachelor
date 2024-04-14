<script setup lang="ts">
import { Button } from '@/components/ui/button'
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select'
import {
  Pagination,
  PaginationEllipsis,
  PaginationFirst,
  PaginationLast,
  PaginationList,
  PaginationListItem,
  PaginationNext,
  PaginationPrev
} from '@/components/ui/pagination'
import { onMounted, ref, watch, computed, toRef, onBeforeUnmount } from 'vue'
import WebSocketService from '@/lib/WebSocketService'
import TheWaterQualitySensorButton from './TheWaterQualitySensorButton.vue'
import axios from 'axios'
import { fetchKafkaKeyTopics, type BoatLocationLog, type WaterQualityLog } from '@/lib/kafka'
import TheWaterQualityLogTable from './tables/TheWaterQualityLogTable.vue'
import TheBoatLocationLogTable from './tables/TheBoatLocationLogTable.vue'
import TextArea from '@/components/ui/textarea/TextArea.vue'

const props = defineProps({
  accessKey: String
})

const accessKey = toRef(props, 'accessKey')
const accessKeyTopics = ref<string[]>([])
const accessKeySensorId = ref<string>('')
const kafkaErrorMessage = ref<string | null>(null)
const waterQualityLogs = ref<WaterQualityLog[]>([])
const boatLocationLogs = ref<BoatLocationLog[]>([])
const selectedView = ref('live')
const currentPage = ref(1)
const itemsPerPage = 10
const selectedTopic = ref('')
const responseData = ref('')

// Utilize a map to handle topic-specific logic efficiently
const topicTypeMap: { [key: string]: string } = {
  'water-quality-updates': 'waterQuality',
  'boat-location-updates': 'boat'
}

const selectedTopicType = computed(() => topicTypeMap[selectedTopic.value] || '')
const totalItems = computed(
  () =>
    (selectedTopicType.value === 'waterQuality' ? waterQualityLogs : boatLocationLogs).value.length
)
const totalPages = computed(() => Math.ceil(totalItems.value / itemsPerPage))
const isLive = computed(() => selectedView.value === 'live')
const isHistorical = computed(() => selectedView.value === 'historical')

const paginatedWaterQualityLogs = computed(() => {
  if (selectedTopicType.value !== 'waterQuality') return []
  const start = (currentPage.value - 1) * itemsPerPage
  return waterQualityLogs.value.slice(start, start + itemsPerPage)
})

const paginatedBoatLocationLogs = computed(() => {
  if (selectedTopicType.value !== 'boat') return []
  const start = (currentPage.value - 1) * itemsPerPage
  return boatLocationLogs.value.slice(start, start + itemsPerPage)
})

// Reformat and sort logs as needed
const processLogs = (logs: any, sortOrder: string) => {
  return logs
    .map((log: any) => ({
      ...log,
      timeStamp: new Date(log.timeStamp).toISOString().replace(/T/, ' ').replace(/\..+/, '')
    }))
    .sort((a: any, b: any) => {
      if (sortOrder === 'newest-rest') {
        return new Date(b.timeStamp).getTime() - new Date(a.timeStamp).getTime()
      } else if (sortOrder === 'oldest-rest') {
        return new Date(a.timeStamp).getTime() - new Date(b.timeStamp).getTime()
      } else {
        return 0
      }
    })
}

const fetchData = async (sortOrder: string) => {
  if (!selectedTopicType.value) return
  try {
    const logResponse = await axios.post(
      `http://localhost:8088/api/sensor/${selectedTopicType.value}/logs`,
      { encryptedKey: accessKey.value }
    )
    const logs = processLogs(logResponse.data, sortOrder)
    if (selectedTopicType.value === 'waterQuality') waterQualityLogs.value = logs
    else if (selectedTopicType.value === 'boat') boatLocationLogs.value = logs
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const startStopLiveFeed = (start = true, historical = false) => {
  const topic = `${selectedTopic.value}-${accessKeySensorId.value}`
  console.log(`Starting live feed for topic: ${topic}, historical: ${historical}`)
  if (start) {
    if (!WebSocketService.isConnected()) {
      console.log('Connecting to topic:', topic, historical)
      WebSocketService.connect(topic, historical)
    } else {
      console.log('Updating topic:', topic, historical)
      WebSocketService.updateTopic(topic, historical)
    }
    WebSocketService.setHandler(updateResponseData)
  } else {
    WebSocketService.disconnect()
  }
}

const updateResponseData = (message: any) => {
  const { topic, message: msg, offset: o } = message

  if (topic.startsWith(`${selectedTopic.value}-`)) {
    if (selectedTopic.value === 'water-quality-updates') {
      const logDetails = msg.match(
        /TimeStamp: ([^,]+), pH: ([-\d.]+), Turbidity: ([-\d.]+) NTU, Temperature: ([-\d.]+)C/
      )
      if (logDetails) {
        const [, timeStamp, ph, turbidity, temperature] = logDetails
        const date = new Date(timeStamp)
        const formattedTimeStamp = date.toISOString().replace(/T/, ' ').replace(/\..+/, '')
        const newLog: WaterQualityLog = {
          id: 0,
          offset: o,
          timeStamp: formattedTimeStamp,
          ph: parseFloat(ph),
          turbidity: parseFloat(turbidity),
          temperature: parseFloat(temperature)
        }
        waterQualityLogs.value.unshift(newLog)
      }
    } else if (selectedTopic.value === 'boat-location-updates') {
      const logDetails = msg.match(/TimeStamp: ([^,]+), Latitude: ([-\d.]+), Longitude: ([-\d.]+)/)
      if (logDetails) {
        const [, timeStamp, latitude, longitude] = logDetails
        const date = new Date(timeStamp)
        const formattedTimeStamp = date.toISOString().replace(/T/, ' ').replace(/\..+/, '')
        const newLog: BoatLocationLog = {
          id: 0,
          offset: o,
          timeStamp: formattedTimeStamp,
          latitude: parseFloat(latitude),
          longitude: parseFloat(longitude)
        }
        console.log(`New boat location log: ${JSON.stringify(newLog)}`)
        boatLocationLogs.value.unshift(newLog)
      }
    } else {
      responseData.value += `${msg}\n`
    }
  }

  currentPage.value = 1
}

const fetchTopics = async () => {
  if (accessKey.value) {
    try {
      const response = await fetchKafkaKeyTopics(accessKey.value)
      if ('error' in response) {
        kafkaErrorMessage.value = response.error
        accessKeyTopics.value = []
      } else {
        accessKeySensorId.value = response.sensorId
        accessKeyTopics.value = response.topics
      }
    } catch (error) {
      kafkaErrorMessage.value = 'Failed to fetch Kafka topics due to an unexpected error.'
      console.error('Failed to fetch Kafka topics:', error)
    }
  }
}

const handleWaterQualityLogsUpdated = (logs: WaterQualityLog[]) => {
  waterQualityLogs.value = logs
}

const handleBoatLocationLogsUpdated = (logs: BoatLocationLog[]) => {
  console.log('Amount of Boat logs:', logs.length)
  boatLocationLogs.value = logs
}

const clearLogs = () => {
  waterQualityLogs.value = []
  boatLocationLogs.value = []
}

const getHistoricalData = async () => {
  console.log('Getting historical data...')

  clearLogs()

  if (selectedTopicType.value === '') return

  try {
    const sessionId = WebSocketService.sessionId

    await axios.post(
      `http://localhost:8088/api/kafka/historical?SessionId=${encodeURIComponent(sessionId)}&SensorType=${encodeURIComponent(selectedTopicType.value ?? '')}`,
      {
        encryptedKey: accessKey.value
      }
    )
  } catch (error) {
    console.error('Failed to get historical data status:', error)
  }
}

watch(
  selectedView,
  (newSortOrder) => {
    waterQualityLogs.value = []
    boatLocationLogs.value = []

    if (newSortOrder === 'live') {
      startStopLiveFeed(true, false)
    } else if (newSortOrder === 'historical') {
      startStopLiveFeed(true, true)
      getHistoricalData()
    } else {
      startStopLiveFeed(false, false)
      fetchData(newSortOrder)
    }
  },
  { immediate: true }
)

watch(
  selectedTopic,
  (newTopic, oldTopic) => {
    if (newTopic && newTopic !== oldTopic) {
      startStopLiveFeed(false)
      selectedView.value === 'live' ? startStopLiveFeed(true) : fetchData(selectedView.value)
    }
  },
  { immediate: true }
)

onMounted(() => {
  fetchData(selectedView.value)
  fetchTopics()
})

onBeforeUnmount(() => {
  WebSocketService.disconnect()
})
</script>

<template>
  <div class="w-[800px]">
    <div v-if="kafkaErrorMessage" class="text-red-500">
      {{ kafkaErrorMessage }}
    </div>
    <div v-else>
      <Select v-model="selectedTopic">
        <SelectTrigger>
          <SelectValue placeholder="Select a topic" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <!-- Use `v-bind:value="topic"` to correctly bind each topic value -->
            <SelectItem v-for="topic in accessKeyTopics" :key="topic" :value="topic">
              {{ topic }}
            </SelectItem>
          </SelectGroup>
        </SelectContent>
      </Select>
      <div v-if="selectedTopic">
        <div class="flex justify-between items-center my-2">
          <div class="flex justify-center items-center gap-4">
            <TheWaterQualitySensorButton
              v-if="
                selectedTopic === 'water-quality-updates' ||
                selectedTopic === 'boat-location-updates'
              "
              @water-logs-updated="handleWaterQualityLogsUpdated"
              @boat-logs-updated="handleBoatLocationLogsUpdated"
              @clear-logs="clearLogs"
              :isLive="isLive"
              :selectedTopicType="selectedTopicType"
              :accessKey="accessKey"
            />
          </div>
          <div class="w-[250px]">
            <Select v-model="selectedView">
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectItem value="live">Live Feed (Kafka)</SelectItem>
                  <SelectItem value="historical">Historical Feed (Kafka)</SelectItem>
                  <SelectItem
                    v-if="
                      selectedTopic === 'water-quality-updates' ||
                      selectedTopic === 'boat-location-updates'
                    "
                    value="oldest-rest"
                    >Oldest Values (REST)</SelectItem
                  >
                  <SelectItem
                    v-if="
                      selectedTopic === 'water-quality-updates' ||
                      selectedTopic === 'boat-location-updates'
                    "
                    value="newest-rest"
                    >Newest Values (REST)</SelectItem
                  >
                </SelectGroup>
              </SelectContent>
            </Select>
          </div>
        </div>
        <div>
          <TheWaterQualityLogTable
            :paginatedWaterQualityLogs="paginatedWaterQualityLogs"
            :showIdColumn="!(isLive || isHistorical)"
            v-if="selectedTopic === 'water-quality-updates'"
          />
          <TheBoatLocationLogTable
            :paginatedBoatLocationLogs="paginatedBoatLocationLogs"
            :showIdColumn="!(isLive || isHistorical)"
            v-else-if="selectedTopic === 'boat-location-updates'"
          />
          <TextArea v-else placeholder="No implemented topic selected" v-model="responseData" />
        </div>

        <div class="mt-2">
          <Pagination
            v-slot="{ page }"
            :total="totalPages * 10"
            :sibling-count="1"
            show-edges
            v-model:page="currentPage"
          >
            <PaginationList v-slot="{ items }" class="flex items-center gap-1">
              <PaginationFirst />
              <PaginationPrev />

              <template v-for="(item, index) in items">
                <PaginationListItem
                  v-if="item.type === 'page'"
                  :key="index"
                  :value="item.value"
                  as-child
                >
                  <Button
                    class="w-10 h-10 p-0"
                    :variant="item.value === page ? 'default' : 'outline'"
                  >
                    {{ item.value }}
                  </Button>
                </PaginationListItem>
                <PaginationEllipsis v-else :key="item.type" :index="index" />
              </template>

              <PaginationNext />
              <PaginationLast />
            </PaginationList>
          </Pagination>
        </div>
      </div>
    </div>
  </div>
</template>
