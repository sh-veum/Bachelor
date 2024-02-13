<script setup lang="ts">
import ThemeCollapsible from '@/components/ThemeCollapsible.vue'
import { Button } from '@/components/ui/button'

import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table'

const keys = [
  { name: 'Key 1', themes: ['Theme 1', 'Theme 2', 'Theme 3'] },
  { name: 'Key 2', themes: ['Theme 1', 'Theme 2', 'Theme 3'] },
  { name: 'Key 3', themes: ['Theme 1', 'Theme 2', 'Theme 3'] }
]
</script>

<template>
  <Button class="mb-4">Create new key</Button>
  <Table>
    <TableCaption>A list of your created keys</TableCaption>
    <TableHeader>
      <TableRow>
        <TableHead>Name</TableHead>
        <TableHead>Themes</TableHead>
        <TableHead>Actions</TableHead>
      </TableRow>
    </TableHeader>
    <TableBody>
      <TableRow v-for="key in keys">
        <TableCell>{{ key.name }}</TableCell>
        <TableCell>
          <ThemeCollapsible :msg="'wtf'" />
        </TableCell>
        <TableCell><Button variant="destructive" @click=""> Delete </Button></TableCell>
      </TableRow>
    </TableBody>
  </Table>
</template>
