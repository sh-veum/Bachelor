<script setup lang="ts">
import { ref, defineEmits } from 'vue'
import TheApiOverview from '@/components/TheApiOverview.vue'
import { Input } from '@/components/ui/input'

const accessKey = ref('')

const emits = defineEmits(['check-accesskey'])

emits('check-accesskey', accessKey.value)
</script>

<template>
  <div class="ml-6 w-[800px]">
    <Input v-model="accessKey" placeholder="place accesskey here" />
    <TheApiOverview :accessKey="accessKey" />
  </div>
</template>
