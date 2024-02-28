<script setup lang="ts">
import { Skeleton } from '@/components/ui/skeleton'
</script>

<template>
  <div>
    <Skeleton class="w-[100px] h-6 rounded-full my-2" />
    <div v-for="n in 2" :key="n" class="flex items-center space-x-3">
      <Skeleton class="w-[17px] h-[17px] my-2 border-[1px] border-gray-500 rounded-sm" />
      <Skeleton class="w-[120px] h-5 rounded-full" />
    </div>
    <hr />
    <Skeleton class="w-[100px] h-6 rounded-full my-2" />
    <div v-for="n in 2" :key="`second-${n}`" class="flex items-center space-x-3">
      <Skeleton class="w-[17px] h-[17px] my-2 border-[1px] border-gray-500 rounded-sm" />
      <Skeleton class="w-[120px] h-5 rounded-full" />
    </div>
  </div>
</template>
