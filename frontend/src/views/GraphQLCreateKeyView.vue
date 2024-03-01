<script setup lang="ts">
import GraphQLCreateKeyDialog from '@/components/graphql/GraphQLCreateKeyDialog.vue'
import GraphQLKeysOverview from '@/components/graphql/GraphQLKeysOverview.vue'
</script>

<template>
  <div class="mx-6">
    <div class="my-6">
      <GraphQLCreateKeyDialog />
    </div>
    <div>
      <GraphQLKeysOverview />
    </div>
  </div>
</template>
