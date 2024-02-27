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

interface Property {
  name: string
  type: string
}

interface Endpoint {
  path: string
  method: string
  expectedBody: {
    properties: Array<Property>
  }
}

interface Theme {
  name: string
  endpoints: Array<Endpoint>
}

const props = defineProps({
  accessKey: String
})

const themes = ref<Theme[]>([])
const APIResponseData = ref('')

const fetchThemes = async (accessKey: string) => {
  if (
    accessKey &&
    /^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/g.test(accessKey)
  ) {
    try {
      const response = await axios.post('http://localhost:8088/api/key/accesskey-themes', {
        encryptedKey: accessKey
      })
      themes.value = response.data
    } catch (error) {
      console.error('Failed to fetch data:', error)
    }
  } else {
    themes.value = []
  }
}

watch(
  () => props.accessKey,
  (newAccessKey) => {
    if (newAccessKey) {
      fetchThemes(newAccessKey)
    }
  },
  { immediate: true }
)

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

const formatProperty = (property: Property) => `${property.name}: ${property.type}`

// Helper to get border color class
const methodBaseColors: { [key: string]: string } = {
  POST: 'blue-500',
  GET: 'green-500',
  PUT: 'yellow-500',
  PATCH: 'purple-500'
}

const getBorderColorClass = (method: string) =>
  `border-${methodBaseColors[method]} border-2 rounded-sm`
const getBadgeBgColorClass = (method: string) => `bg-${methodBaseColors[method]}`
</script>

<template>
  <div v-for="(theme, themeIndex) in themes" :key="`theme-${themeIndex}`">
    <Accordion type="single" collapsible class="mt-2">
      <AccordionItem :value="`theme-${themeIndex}`">
        <AccordionTrigger>{{ theme.name }}</AccordionTrigger>
        <AccordionContent>
          <Accordion class="mt-2" type="single" collapsible>
            <AccordionItem
              v-for="(endpoint, index) in theme.endpoints"
              :key="`endpoint-${index}`"
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
                <div
                  v-for="(property, propIndex) in endpoint.expectedBody.properties"
                  :key="`prop-${propIndex}`"
                >
                  {{ formatProperty(property) }}
                </div>
              </AccordionContent>
            </AccordionItem>
          </Accordion>
        </AccordionContent>
      </AccordionItem>
    </Accordion>
  </div>
  <TextArea
    v-if="themes.length > 0"
    v-model="APIResponseData"
    class="mt-2 h-72 w-2/4"
    placeholder="API response will appear here..."
  />
</template>
