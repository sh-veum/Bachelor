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
const selectedSortOrder = ref('live')
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
const isLive = computed(() => selectedSortOrder.value === 'live')

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
    .sort((a: any, b: any) =>
      sortOrder === 'newest' ? new Date(b.timeStamp).getTime() - new Date(a.timeStamp).getTime() : 0
    )
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

const startStopLiveFeed = (start = true) => {
  const topic = `${selectedTopic.value}-${accessKeySensorId.value}`
  if (start) {
    if (!WebSocketService.isConnected()) WebSocketService.connect()
    WebSocketService.subscribe(topic, updateResponseData)
  } else {
    WebSocketService.unsubscribe(topic, updateResponseData)
  }
}

const updateResponseData = (message: any) => {
  const { topic, message: msg, offset: o } = message

  //   console.log(`Received message: ${msg}` + ' from topic: ' + topic)

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

watch(
  selectedSortOrder,
  (newSortOrder) => {
    waterQualityLogs.value = []
    boatLocationLogs.value = []
    if (newSortOrder === 'live') {
      startStopLiveFeed(true)
    } else {
      startStopLiveFeed(false)
      fetchData(newSortOrder)
    }
  },
  { immediate: true }
)

watch(
  selectedTopic,
  (newTopic) => {
    if (newTopic) {
      startStopLiveFeed(false)
      selectedSortOrder.value === 'live'
        ? startStopLiveFeed(true)
        : fetchData(selectedSortOrder.value)
    }
  },
  { immediate: true }
)

onMounted(() => {
  fetchData(selectedSortOrder.value)
  fetchTopics()
})

onBeforeUnmount(() => {
  startStopLiveFeed(false)
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
      <div class="flex justify-between items-center my-2">
        <div class="flex justify-center items-center gap-4">
          <TheWaterQualitySensorButton
            v-if="
              selectedTopic === 'water-quality-updates' || selectedTopic === 'boat-location-updates'
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
          <Select v-model="selectedSortOrder">
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectItem
                  v-if="
                    selectedTopic === 'water-quality-updates' ||
                    selectedTopic === 'boat-location-updates'
                  "
                  value="oldest"
                  >Oldest Values (REST)</SelectItem
                >
                <SelectItem
                  v-if="
                    selectedTopic === 'water-quality-updates' ||
                    selectedTopic === 'boat-location-updates'
                  "
                  value="newest"
                  >Newest Values (REST)</SelectItem
                >
                <SelectItem value="live">Live Feed</SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
      </div>
      <div>
        <TheWaterQualityLogTable
          :paginatedWaterQualityLogs="paginatedWaterQualityLogs"
          :showIdColumn="!isLive"
          v-if="selectedTopic === 'water-quality-updates'"
        />
        <TheBoatLocationLogTable
          :paginatedBoatLocationLogs="paginatedBoatLocationLogs"
          :showIdColumn="!isLive"
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
</template>
