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
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table'
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
import { onMounted, ref, watch, computed } from 'vue'
import WebSocketService from '@/lib/WebSocketService'
import { userId } from '@/lib/useAuth'
import TheWaterQualitySensorButton from './TheWaterQualitySensorButton.vue'
import axios from 'axios'

interface WaterQualityLog {
  id: number
  timeStamp: string
  ph: number
  turbidity: number
  temperature: number
}

const waterQualityLogs = ref<WaterQualityLog[]>([])
const selectedSortOrder = ref('oldest')
const currentPage = ref(1)
const itemsPerPage = 10

const totalItems = computed(() => waterQualityLogs.value.length)
const totalPages = computed(() => Math.ceil(totalItems.value / itemsPerPage))

const paginatedLogs = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return waterQualityLogs.value.slice(start, end)
})

const fetchData = async (sortOrder: string) => {
  try {
    const logResponse = await axios.get('http://localhost:8088/api/sensor/waterQuality/logs')
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
      logs = logs.sort((a, b) => new Date(b.timeStamp).getTime() - new Date(a.timeStamp).getTime())
    }

    waterQualityLogs.value = logs
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const startLiveFeed = () => {
  const topic = `water-quality-updates-${userId.value}`
  if (!WebSocketService.isConnected()) {
    WebSocketService.connect()
  }
  WebSocketService.subscribe(topic, updateResponseData)
}

const stopLiveFeed = () => {
  const topic = `water-quality-updates-${userId.value}`
  WebSocketService.unsubscribe(topic, updateResponseData)
}

const updateResponseData = (message: string) => {
  console.log('MESSAGE :', message)

  const logDetails = message.match(
    /TimeStamp: ([^,]+), pH: ([\d.]+), Turbidity: ([\d.]+) NTU, Temperature: ([\d.]+)C/
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
    waterQualityLogs.value.push(newLog)
  } else {
    console.error('Failed to parse message:', message)
  }
}

const updatePaginationOnNewItem = () => {
  currentPage.value = totalPages.value
}

watch(selectedSortOrder, (newSortOrder: string) => {
  if (newSortOrder === 'live') {
    waterQualityLogs.value = []
    startLiveFeed()
  } else {
    stopLiveFeed()
    fetchData(newSortOrder)
  }
})

onMounted(() => {
  fetchData(selectedSortOrder.value)
})
</script>

<template>
  <div class="w-[800px]">
    <div class="flex justify-between items-center">
      <TheWaterQualitySensorButton />
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
    <Table class="w-[800px]">
      <TableCaption>Log list</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead class="w-[200px]">Id</TableHead>
          <TableHead class="w-[200px]">TimeStamp</TableHead>
          <TableHead class="w-[200px]">pH value</TableHead>
          <TableHead class="w-[200px]">Turbidity (NTU)</TableHead>
          <TableHead class="w-[200px]">Temperature (C)</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="logs in paginatedLogs" :key="logs.id" class="h-20">
          <TableCell>{{ logs.id }}</TableCell>
          <TableCell>{{ logs.timeStamp }}</TableCell>
          <TableCell>{{ logs.ph }}</TableCell>
          <TableCell>{{ logs.turbidity }}</TableCell>
          <TableCell>{{ logs.temperature }}</TableCell>
        </TableRow>
      </TableBody>
    </Table>
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
          <PaginationListItem v-if="item.type === 'page'" :key="index" :value="item.value" as-child>
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
</template>
