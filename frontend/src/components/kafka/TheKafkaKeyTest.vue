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
import { onMounted, ref, watch, computed, toRef } from 'vue'
import WebSocketService from '@/lib/WebSocketService'
import { userId } from '@/lib/useAuth'
import TheWaterQualitySensorButton from './TheWaterQualitySensorButton.vue'
import axios from 'axios'
import { fetchKafkaKeyTopics, type BoatLocationLog, type WaterQualityLog } from '@/lib/kafka'
import TheWaterQualityLogTable from './tables/TheWaterQualityLogTable.vue'
import TheBoatLocationLogTable from './tables/TheBoatLocationLogTable.vue'

const props = defineProps({
  accessKey: String
})

const accessKey = toRef(props, 'accessKey')
const accessKeyTopics = ref<string[]>([])
const accesssKeySensorId = ref<string>('')
const kafkaErrorMessage = ref<string | null>(null)

const waterQualityLogs = ref<WaterQualityLog[]>([])
const boatLocationLogs = ref<BoatLocationLog[]>([])
const selectedSortOrder = ref('oldest')
const currentPage = ref(1)
const itemsPerPage = 10

const selectedTopic = ref('')
const selectedTopicType = computed(() => {
  if (selectedTopic.value === 'water-quality-updates') {
    return 'waterQuality'
  } else if (selectedTopic.value === 'boat-location-updates') {
    return 'boat'
  } else {
    return ''
  }
})

const totalItems = computed(() => {
  if (selectedTopic.value === 'water-quality-updates') {
    return waterQualityLogs.value.length
  } else if (selectedTopic.value === 'boat-location-updates') {
    return boatLocationLogs.value.length
  } else {
    return 0
  }
})
const totalPages = computed(() => Math.ceil(totalItems.value / itemsPerPage))

const isLive = computed(() => selectedSortOrder.value === 'live')

const paginatedWaterQualityLogs = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return waterQualityLogs.value.slice(start, end)
})

const paginatedBoatLocationLogs = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return boatLocationLogs.value.slice(start, end)
})

const fetchData = async (sortOrder: string) => {
  if (selectedTopicType.value === '') return

  try {
    const logResponse = await axios.get(
      `http://localhost:8088/api/sensor/${selectedTopicType.value}/logs?id=${accesssKeySensorId.value}`
    )

    if (selectedTopic.value === 'water-quality-updates') {
      let logs: WaterQualityLog[] = logResponse.data.map((log: WaterQualityLog) => {
        // Create a new Date object from the timeStamp string
        const date = new Date(log.timeStamp)
        const formattedTimeStamp = date.toISOString().replace(/T/, ' ').replace(/\..+/, '')
        return {
          ...log,
          timeStamp: formattedTimeStamp
        }
      })

      if (sortOrder === 'newest') {
        logs = logs.sort(
          (a, b) => new Date(b.timeStamp).getTime() - new Date(a.timeStamp).getTime()
        )
      }

      waterQualityLogs.value = logs
    } else if (selectedTopic.value === 'boat-location-updates') {
      let logs: BoatLocationLog[] = logResponse.data.map((log: BoatLocationLog) => {
        const date = new Date(log.timeStamp)
        const formattedTimeStamp = date.toISOString().replace(/T/, ' ').replace(/\..+/, '')
        return {
          ...log,
          timeStamp: formattedTimeStamp
        }
      })

      if (sortOrder === 'newest') {
        logs = logs.sort(
          (a, b) => new Date(b.timeStamp).getTime() - new Date(a.timeStamp).getTime()
        )
      }

      boatLocationLogs.value = logs
    } else {
      console.error('Invalid topic:', selectedTopic.value)
    }
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const startLiveFeed = () => {
  const topic = `${selectedTopic.value}-${userId.value}`
  if (!WebSocketService.isConnected()) {
    WebSocketService.connect()
  }
  WebSocketService.subscribe(topic, updateResponseData)
}

const stopLiveFeed = () => {
  const topic = `${selectedTopic.value}-${userId.value}`
  WebSocketService.unsubscribe(topic, updateResponseData)
}

const updateResponseData = (message: string) => {
  if (selectedTopic.value === 'water-quality-updates') {
    const logDetails = message.match(
      /TimeStamp: ([^,]+), pH: ([-\d.]+), Turbidity: ([-\d.]+) NTU, Temperature: ([-\d.]+)C/
    )
    if (logDetails) {
      const [, timeStamp, ph, turbidity, temperature] = logDetails
      const date = new Date(timeStamp)
      const formattedTimeStamp = date.toISOString().replace(/T/, ' ').replace(/\..+/, '')
      const newLog: WaterQualityLog = {
        id: waterQualityLogs.value.length + 1,
        timeStamp: formattedTimeStamp,
        ph: parseFloat(ph),
        turbidity: parseFloat(turbidity),
        temperature: parseFloat(temperature)
      }
      waterQualityLogs.value.unshift(newLog)
    }
  } else if (selectedTopic.value === 'boat-location-updates') {
    const logDetails = message.match(
      /TimeStamp: ([^,]+), Latitude: ([-\d.]+), Longitude: ([-\d.]+)/
    )
    if (logDetails) {
      const [, timeStamp, latitude, longitude] = logDetails
      const date = new Date(timeStamp)
      const formattedTimeStamp = date.toISOString().replace(/T/, ' ').replace(/\..+/, '')
      const newLog: BoatLocationLog = {
        id: boatLocationLogs.value.length + 1,
        timeStamp: formattedTimeStamp,
        latitude: parseFloat(latitude),
        longitude: parseFloat(longitude)
      }
      console.log(`New boat location log: ${JSON.stringify(newLog)}`) // Debugging line to check the log
      boatLocationLogs.value.unshift(newLog)
    }
  } else {
    console.error('Invalid topic:', selectedTopic.value)
  }

  currentPage.value = 1
}

const fetchTopics = async () => {
  if (props.accessKey) {
    try {
      const response = await fetchKafkaKeyTopics(props.accessKey)
      if ('error' in response) {
        kafkaErrorMessage.value = response.error
        accessKeyTopics.value = []
      } else {
        accesssKeySensorId.value = response.sensorId
        accessKeyTopics.value = response.topics
      }
    } catch (error) {
      console.error('Failed to fetch Kafka topics:', error)
      kafkaErrorMessage.value = 'Failed to fetch Kafka topics due to an unexpected error.'
      accessKeyTopics.value = []
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

watch(selectedSortOrder, (newSortOrder: string) => {
  if (newSortOrder === 'live') {
    waterQualityLogs.value = []
    boatLocationLogs.value = []
    startLiveFeed()
  } else {
    stopLiveFeed()
    fetchData(newSortOrder)
  }
})

watch(selectedTopic, () => {
  if (selectedTopic.value === 'water-quality-updates') {
    boatLocationLogs.value = []
  } else if (selectedTopic.value === 'boat-location-updates') {
    waterQualityLogs.value = []
  } else {
    waterQualityLogs.value = []
    boatLocationLogs.value = []
  }

  if (selectedSortOrder.value === 'live') {
    waterQualityLogs.value = []
    boatLocationLogs.value = []
    startLiveFeed()
  } else {
    stopLiveFeed()
    fetchData(selectedSortOrder.value)
  }
})

onMounted(() => {
  fetchData(selectedSortOrder.value)
  fetchTopics()
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
                <SelectItem value="oldest">Oldest Values (REST)</SelectItem>
                <SelectItem value="newest">Newest Values (REST)</SelectItem>
                <SelectItem value="live">Live Feed</SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
      </div>
      <div>
        <TheWaterQualityLogTable
          :paginatedWaterQualityLogs="paginatedWaterQualityLogs"
          v-if="selectedTopic === 'water-quality-updates'"
        />
        <TheBoatLocationLogTable
          :paginatedBoatLocationLogs="paginatedBoatLocationLogs"
          v-else-if="selectedTopic === 'boat-location-updates'"
        />
        <p v-else>No implemented topic selected</p>
      </div>

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
              <Button class="w-10 h-10 p-0" :variant="item.value === page ? 'default' : 'outline'">
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
</template>
