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
import { fetchGraphQLKeys } from '@/lib/graphQL'

const graphQLKeys = ref<GraphQLKey[]>([])

const fetchData = async () => {
  try {
    graphQLKeys.value = await fetchGraphQLKeys()
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const disableKey = () => {
  alert('Not yet implemented')
}

const deleteKey = () => {
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
            <Button class="bg-zinc-600" @click="disableKey">Disable</Button>
          </TableCell>
          <TableCell>
            <Button variant="destructive" @click="deleteKey">Delete key</Button>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
