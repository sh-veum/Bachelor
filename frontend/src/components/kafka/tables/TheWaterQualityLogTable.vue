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
import { toRef } from 'vue'
import { type WaterQualityLog } from '@/lib/kafka'

const props = defineProps<{
  paginatedWaterQualityLogs: WaterQualityLog[]
  showIdColumn: Boolean
}>()

const waterQualityLogs = toRef(props, 'paginatedWaterQualityLogs')
const showIdColumn = toRef(props, 'showIdColumn')
</script>

<template>
  <Table class="w-[800px]">
    <TableCaption>Log list</TableCaption>
    <TableHeader>
      <TableRow>
        <TableHead class="w-[50px]" v-if="showIdColumn">ID</TableHead>
        <TableHead class="w-[50px]">Offset</TableHead>
        <TableHead class="w-[200px]">TimeStamp</TableHead>
        <TableHead class="w-[200px]">pH value</TableHead>
        <TableHead class="w-[200px]">Turbidity (NTU)</TableHead>
        <TableHead class="w-[200px]">Temperature (C)</TableHead>
      </TableRow>
    </TableHeader>
    <TableBody>
      <TableRow v-for="(logs, index) in waterQualityLogs" :key="index">
        <TableCell v-if="showIdColumn">{{ logs.id }}</TableCell>
        <TableCell>{{ logs.offset }}</TableCell>
        <TableCell>{{ logs.timeStamp }}</TableCell>
        <TableCell>{{ logs.ph }}</TableCell>
        <TableCell>{{ logs.turbidity }}</TableCell>
        <TableCell>{{ logs.temperature }}</TableCell>
      </TableRow>
    </TableBody>
  </Table>
</template>
