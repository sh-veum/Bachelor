<script setup lang="ts">
import { onMounted, ref } from 'vue'
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
import { fetchGraphQLKeys, toggleApiKey } from '@/lib/graphQL'

const graphQLKeys = ref<GraphQLKey[]>([])

const fetchData = async () => {
  try {
    graphQLKeys.value = await fetchGraphQLKeys()
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const toggleKeyEnabledStatus = async (id: number, isEnabled: boolean) => {
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

const deleteKey = async () => {
  alert('Not yet implemented')
}

onMounted(fetchData)
</script>

<template>
  <div>
    <Button @click="fetchData">Re-Load Table</Button>
    <Table>
      <TableCaption>GraphQL Keys Overview</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead class="w-[200px]">Key Name</TableHead>
          <TableHead class="w-[400px]">Permissions</TableHead>
          <TableHead class="w-[200px]">Expires in (days)</TableHead>
          <TableHead class="w-[100px]">Disable</TableHead>
          <TableHead class="w-[100px]">Delete</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="(graphQLKey, index) in graphQLKeys" :key="index">
          <TableCell>{{ graphQLKey.keyName }}</TableCell>
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
            <Button variant="destructive" @click="deleteKey">Delete key</Button>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
