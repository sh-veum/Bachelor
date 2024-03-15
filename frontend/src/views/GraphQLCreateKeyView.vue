<script setup lang="ts">
import GraphQLCreateKeyDialog from '@/components/graphql/GraphQLCreateKeyDialog.vue'
import GraphQLKeysOverview from '@/components/graphql/GraphQLKeysOverview.vue'
import type { GraphQLKey } from '@/components/interfaces/GraphQLSchema'
import { fetchGraphQLKeys } from '@/lib/graphQL'
import { onMounted, ref } from 'vue'

const graphQLKeys = ref<GraphQLKey[]>([])

const fetchData = async () => {
  try {
    graphQLKeys.value = await fetchGraphQLKeys()
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const handleSubmit = () => {
  // TODO: emit the created key and update the graphQLKeys ref instead?
  // would need to get the created key as a response from the request (cause we need the id, etc)
  fetchData()
}

onMounted(fetchData)
</script>

<template>
  <div class="mx-6">
    <div class="my-6">
      <GraphQLCreateKeyDialog @submit="handleSubmit" />
    </div>
    <div>
      <GraphQLKeysOverview :graph-q-l-keys="graphQLKeys" />
    </div>
  </div>
</template>
