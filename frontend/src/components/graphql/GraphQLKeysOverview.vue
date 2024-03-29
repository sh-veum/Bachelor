<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
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
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger
} from '@/components/ui/accordion'
import { Button } from '@/components/ui/button'
import type { GraphQLKey } from '../interfaces/GraphQLSchema'
import { toggleApiKey, deleteGraphQLApiKey } from '@/lib/graphQL'
import type { UUID } from 'crypto'
import { defineProps, toRef } from 'vue'

const props = defineProps<{
  graphQLKeys: GraphQLKey[]
}>()

// Initialize a ref for the WebSocket
const webSocket = ref<WebSocket | null>(null)
const receivedMessage = ref(null)

// Convert the graphQLKeys prop to a reactive ref
// TODO: maybe change to https://vuejs.org/guide/components/v-model
const graphQLKeys = toRef(props, 'graphQLKeys')

const toggleKeyEnabledStatus = async (id: UUID, isEnabled: boolean) => {
  try {
    const response = await toggleApiKey(id, isEnabled, 'graphql')
    if (response.toggleApiKey.isSuccess) {
      const keyIndex = graphQLKeys.value.findIndex((key) => key.id === id)
      if (keyIndex !== -1) {
        graphQLKeys.value[keyIndex].isEnabled = isEnabled
        console.log(response.toggleApiKey.message) // Log success message
      }
    } else {
      console.error('Failed to toggle key enabled status:', response.toggleApiKey.message)
    }
  } catch (error) {
    console.error('Error toggling key enabled status:', error)
  }
}

const deleteKey = async (id: UUID) => {
  try {
    const response = await deleteGraphQLApiKey(id)
    if (response.deleteGraphQLApiKey.isSuccess) {
      const keyIndex = graphQLKeys.value.findIndex((key) => key.id === id)
      if (keyIndex !== -1) {
        graphQLKeys.value.splice(keyIndex, 1)
      }
    } else {
      console.error('Failed to delete key:', response.deleteGraphQLApiKey.message)
    }
  } catch (error) {
    console.error('Error deleting key:', error)
  }
}

// Function to handle incoming WebSocket messages
const handleWebSocketMessage = (event: MessageEvent) => {
  const message = JSON.parse(event.data)
  console.log('Received message:', message)

  if (message.topic === 'graphql-key-updates') {
    receivedMessage.value = message.message
  }
}

const setupWebSocket = () => {
  webSocket.value = new WebSocket('ws://localhost:8088/ws')

  webSocket.value.onmessage = handleWebSocketMessage
  webSocket.value.onopen = () => console.log('WebSocket connected')
  webSocket.value.onclose = () => console.log('WebSocket disconnected')
}

const cleanupWebSocket = () => {
  if (webSocket.value) {
    webSocket.value.close()
  }
}

onMounted(() => {
  setupWebSocket()
})

onUnmounted(() => {
  cleanupWebSocket()
})
</script>

<template>
  <div>
    <span v-if="receivedMessage" class="text-red-500 ml-4 font-bold">{{ receivedMessage }}</span>
    <Table>
      <TableCaption>GraphQL Keys Overview</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead class="w-[200px]">Key Name</TableHead>
          <TableHead class="w-[200px]">Permissions</TableHead>
          <TableHead class="w-[200px]">Expires in (days)</TableHead>
          <TableHead class="w-[100px]">Disable</TableHead>
          <TableHead class="w-[100px]">Delete</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="(graphQLKey, index) in graphQLKeys" :key="index">
          <TableCell class="text-xl">{{ graphQLKey.keyName }}</TableCell>
          <TableCell>
            <Accordion type="single" collapsible>
              <AccordionItem :value="`${index}`">
                <AccordionTrigger>{{ graphQLKey.permissions.length }} permissions</AccordionTrigger>
                <AccordionContent>
                  <div v-for="(permissions, index) in graphQLKey.permissions" :key="index">
                    <p class="text-base font-bold">{{ permissions.queryName }}</p>
                    <div v-for="(allowedField, index) in permissions.allowedFields" :key="index">
                      <li>{{ allowedField }}</li>
                    </div>
                  </div>
                </AccordionContent>
              </AccordionItem>
            </Accordion>
          </TableCell>
          <TableCell>
            <p>{{ graphQLKey.expiresIn }} days</p>
          </TableCell>
          <TableCell>
            <Button
              class="bg-zinc-600"
              v-if="graphQLKey.isEnabled"
              @click="toggleKeyEnabledStatus(graphQLKey.id, false)"
            >
              Disable
            </Button>
            <Button
              class="bg-green-600"
              v-else
              @click="toggleKeyEnabledStatus(graphQLKey.id, true)"
            >
              Enable
            </Button>
          </TableCell>
          <TableCell>
            <Button variant="destructive" @click="deleteKey(graphQLKey.id)"> Delete key </Button>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
