<script setup lang="ts">
import { ref, defineEmits } from 'vue'
import TheRestApiOverview from '@/components/TheRestApiOverview.vue'
import { Input } from '@/components/ui/input'

const accessKey = ref('')

const emits = defineEmits(['check-accesskey'])

emits('check-accesskey', accessKey.value)
</script>

<template>
  <div class="ml-6 w-[800px]">
    <Input v-model="accessKey" placeholder="place accesskey here" />
    <TheRestApiOverview :accessKey="accessKey" />
  </div>
</template>
