<script setup lang="ts">
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger
} from '@/components/ui/accordion'
import axios from 'axios'
import { watch, ref, defineProps } from 'vue'
import { TextArea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'

interface Endpoint {
  path: string
  method: string
  expectedBody: {
    properties: Array<{
      name: string
      type: string
    }>
  }
}

const props = defineProps({
  accessKey: String
})

const endpoints = ref<Endpoint[]>([])
const APIResponseData = ref('')

const fetchAccesskeyEndpoint = async (accessKey: string) => {
  if (
    accessKey &&
    /^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/g.test(accessKey)
  ) {
    try {
      const response = await axios.post('http://localhost:8088/api/key/accesskey-endpoints', {
        encryptedKey: accessKey
      })
      endpoints.value = response.data
    } catch (error) {
      console.error('Failed to fetch data:', error)
    }
  } else {
    // Clear the endpoints if the accessKey is empty or invalid
    endpoints.value = []
  }
}

const testApiEndpointWithAccessKey = async (endpoint: Endpoint) => {
  try {
    let response
    const url = `http://localhost:8088${endpoint.path}`
    const body = {
      encryptedKey: props.accessKey
    }

    // Check the method and send the request accordingly
    switch (endpoint.method) {
      case 'POST':
        response = await axios.post(url, body)
        break
      case 'GET':
        response = await axios.get(url)
        break
      default:
        console.error(`Method ${endpoint.method} is not supported`)
        // TODO: Implement checks for other methods
        return
    }

    console.log('API Response:', response.data)
    APIResponseData.value = JSON.stringify(response.data, null, 2)
  } catch (error) {
    console.error('API Test error:', error)
    APIResponseData.value = JSON.stringify((error as any).response?.data || 'Error', null, 2)
  }
}

watch(
  () => props.accessKey,
  (newAccessKey) => {
    if (newAccessKey) {
      fetchAccesskeyEndpoint(newAccessKey)
    }
  },
  { immediate: true }
)

const formatProperty = (property: { name: string; type: string }) =>
  `${property.name}: ${property.type}`

// Helper to get border color class
const methodBaseColors: { [key: string]: string } = {
  POST: 'blue-500',
  GET: 'green-500',
  PUT: 'yellow-500',
  PATCH: 'purple-500'
}

const getBorderColorClass = (method: string) =>
  `border-${methodBaseColors[method]} border-2 rounded-sm`

// Helper to get badge background color class
const getBadgeBgColorClass = (method: string) => `bg-${methodBaseColors[method]}`
</script>

<template>
  <Accordion type="single" class="mt-2" collapsible>
    <AccordionItem
      v-for="(endpoint, index) in endpoints"
      :key="index"
      :value="`item-${index}`"
      :class="getBorderColorClass(endpoint.method)"
      class="mb-2 px-2"
    >
      <AccordionTrigger>
        <div>
          <Button
            @click="testApiEndpointWithAccessKey(endpoint)"
            :class="getBadgeBgColorClass(endpoint.method)"
            class="rounded-sm h-8 px-4 font-bold text-base text-center"
            >{{ endpoint.method }}</Button
          >
        </div>
        <div class="text-base text-left">
          {{ endpoint.path }}
        </div>
      </AccordionTrigger>
      <AccordionContent>
        <div v-for="(property, index) in endpoint.expectedBody.properties" :key="index">
          {{ formatProperty(property) }}
        </div>
      </AccordionContent>
    </AccordionItem>
  </Accordion>
  <TextArea
    v-if="endpoints.length > 0"
    v-model="APIResponseData"
    class="mt-2 h-72 w-2/4"
    placeholder="API response will appear here..."
  />
</template>
