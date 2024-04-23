<script setup lang="ts">
import { toggleKafkaKeyEnabledStatus, deleteKafkaKey, type KafkaKey } from '@/lib/kafka'
import { toRef } from 'vue'
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

const props = defineProps<{
  kafkaKeys: KafkaKey[]
}>()

const kafkaKeys = toRef(props, 'kafkaKeys')

const toggleKeyEnabledStatus = async (id: string, isEnabled: boolean) => {
  try {
    const responseStatus = await toggleKafkaKeyEnabledStatus(id, isEnabled)
    if (responseStatus === 200) {
      const keyIndex = kafkaKeys.value.findIndex((key) => key.id === id)
      if (keyIndex !== -1) {
        kafkaKeys.value[keyIndex].isEnabled = isEnabled
      }
    } else {
      console.error('Failed to toggle key enabled status:', responseStatus)
    }
  } catch (error) {
    console.error('Error toggling key enabled status:', error)
  }
}

const deleteKey = async (id: string) => {
  try {
    const responseStatus = await deleteKafkaKey(id)
    if (responseStatus === 200) {
      const keyIndex = kafkaKeys.value.findIndex((key) => key.id === id)
      if (keyIndex !== -1) {
        kafkaKeys.value.splice(keyIndex, 1)
      }
    } else {
      console.error('Failed to delete key:', responseStatus)
    }
  } catch (error) {
    console.error('Error deleting key:', error)
  }
}
</script>

<template>
  <div>
    <Table>
      <TableCaption>Kafka Keys Overview</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead class="w-[200px]">Key Name</TableHead>
          <TableHead>Topics</TableHead>
          <TableHead class="w-[100px]">Expires in (days)</TableHead>
          <TableHead class="w-[100px] text-center">Disable</TableHead>
          <TableHead class="w-[100px] text-center">Delete</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="(kafkaKey, index) in kafkaKeys" :key="index">
          <TableCell class="text-xl">{{ kafkaKey.keyName }}</TableCell>
          <TableCell>
            <Accordion type="single" collapsible>
              <AccordionItem :value="`${index}`">
                <AccordionTrigger>{{ kafkaKey.topics.length }} topics</AccordionTrigger>
                <AccordionContent>
                  <div v-for="(topic, index) in kafkaKey.topics" :key="index">
                    <p class="text-base">{{ topic }}</p>
                  </div>
                </AccordionContent>
              </AccordionItem>
            </Accordion>
          </TableCell>
          <TableCell>
            <p>{{ kafkaKey.expiresIn }} days</p>
          </TableCell>
          <TableCell class="text-center">
            <Button
              class="bg-zinc-600"
              v-if="kafkaKey.isEnabled"
              @click="toggleKeyEnabledStatus(kafkaKey.id, false)"
            >
              Disable
            </Button>
            <Button class="bg-green-600" v-else @click="toggleKeyEnabledStatus(kafkaKey.id, true)">
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
                  <DialogTitle>Delete User</DialogTitle>
                  <DialogDescription>
                    Are you sure you want to delete this user? This action cannot be undone.
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <Button variant="destructive" @click="deleteKey(kafkaKey.id)">
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
