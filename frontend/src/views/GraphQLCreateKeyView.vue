<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { ref } from 'vue'
import { fetchAvailableClassTables, fetchAvailableQueries } from '@/lib/graphQL'

const keyName = ref('')

const createGraphQLKey = async () => {
  await fetchAvailableClassTables()
  await fetchAvailableQueries()
}
</script>

<template>
  <div class="m-2 w-[500px]">
    <Input v-model="keyName" placeholder="write key name here" />
    <Button @click="createGraphQLKey" class="mt-2">Create Key</Button>
  </div>
</template>
