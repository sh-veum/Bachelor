<script setup lang="ts">
import { ref } from 'vue'
import TheGraphQLKeyTest from '@/components/TheGraphQLKeyTest.vue'
import { Input } from '@/components/ui/input'

const accessKey = ref('')

const emits = defineEmits(['check-accesskey'])

emits('check-accesskey', accessKey.value)
</script>

<template>
  <div class="ml-6 w-[800px]">
    <Input v-model="accessKey" placeholder="place accesskey here" />
    <TheGraphQLKeyTest :accessKey="accessKey" />
  </div>
</template>
