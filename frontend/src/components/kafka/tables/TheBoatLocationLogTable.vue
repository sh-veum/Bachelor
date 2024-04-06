<script setup lang="ts">
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table'
import { type BoatLocationLog } from '@/lib/kafka'
import { toRef } from 'vue'

const props = defineProps<{
  paginatedBoatLocationLogs: BoatLocationLog[]
}>()

const boatLocationLogs = toRef(props, 'paginatedBoatLocationLogs')
</script>

<template>
  <Table class="w-[800px]">
    <TableCaption>Log list</TableCaption>
    <TableHeader>
      <TableRow>
        <TableHead class="w-[50px]">Offset</TableHead>
        <TableHead class="w-[250px]">TimeStamp</TableHead>
        <TableHead class="w-[250px]">Latitude</TableHead>
        <TableHead class="w-[250px]">Longitude</TableHead>
      </TableRow>
    </TableHeader>
    <TableBody>
      <TableRow v-for="logs in boatLocationLogs" :key="logs.offset">
        <TableCell>{{ logs.offset }}</TableCell>
        <TableCell>{{ logs.timeStamp }}</TableCell>
        <TableCell>{{ logs.latitude }}</TableCell>
        <TableCell>{{ logs.longitude }}</TableCell>
      </TableRow>
    </TableBody>
  </Table>
</template>
