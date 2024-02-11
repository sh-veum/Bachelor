<script setup lang="ts">
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger
} from '@/components/ui/accordion'
import { Badge } from '@/components/ui/badge'
import axios from 'axios'
import { onMounted, ref } from 'vue'

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

const endpoints = ref<Endpoint[]>([])

const fetchDefaultEndpoints = async () => {
  try {
    // Fetch default endpoints
    const endpointsResponse = await axios.get(
      'http://localhost:8088/api/database/get-default-endpoints'
    )
    endpoints.value = endpointsResponse.data
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const formatProperty = (property: { name: string; type: string }) =>
  `${property.name}: ${property.type}`

onMounted(fetchDefaultEndpoints)

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
  <Accordion type="single" class="w-[800px]" collapsible>
    <AccordionItem
      v-for="(endpoint, index) in endpoints"
      :key="index"
      :value="`item-${index}`"
      :class="getBorderColorClass(endpoint.method)"
      class="mb-2 px-2"
    >
      <AccordionTrigger>
        <div>
          <Badge
            :class="getBadgeBgColorClass(endpoint.method)"
            class="rounded-sm h-8 px-4 font-bold text-base text-center"
            >{{ endpoint.method }}</Badge
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
</template>
