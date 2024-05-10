<script setup lang="ts">
import { ref } from 'vue'
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
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogTrigger
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import type { GraphQLKey } from '../interfaces/GraphQLSchema'
import { toggleApiKey, deleteGraphQLApiKey } from '@/lib/graphQL'
import type { UUID } from 'crypto'

const receivedMessage = ref(null)

const graphQLKeys = defineModel('graphQLKeys', {
  type: Array as () => GraphQLKey[],
  default: []
})

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
</script>

<template>
  <div>
    <span v-if="receivedMessage" class="text-red-500 ml-4 font-bold">{{ receivedMessage }}</span>
    <Table>
      <TableCaption>GraphQL Keys Overview</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead class="w-[200px]">Key Name</TableHead>
          <TableHead>Permissions</TableHead>
          <TableHead class="w-[100px]">Expires in (days)</TableHead>
          <TableHead class="w-[100px] text-center">Disable</TableHead>
          <TableHead class="w-[100px] text-center">Delete</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="(graphQLKey, index) in graphQLKeys" :key="index">
          <TableCell class="text-xl">{{ graphQLKey.keyName }}</TableCell>
          <TableCell>
            <Accordion type="single" collapsible>
              <AccordionItem :value="`${index}`">
                <AccordionTrigger>
                  Total: {{ graphQLKey.permissions.length }} tables,
                  {{
                    graphQLKey.permissions.reduce(
                      (total, permissions) => total + permissions.allowedFields.length,
                      0
                    )
                  }}
                  fields
                </AccordionTrigger>
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
          <TableCell class="text-center">
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
          <TableCell class="text-center">
            <Dialog>
              <DialogTrigger as-child>
                <Button variant="destructive" class="p-2 hover:bg-red-800"> Delete </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Delete Key</DialogTitle>
                  <DialogDescription>
                    Are you sure you want to delete this key? This action cannot be undone.
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <Button variant="destructive" @click="deleteKey(graphQLKey.id)">
                    Delete key
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
