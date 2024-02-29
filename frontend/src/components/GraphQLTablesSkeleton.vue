<script setup lang="ts">
import { Skeleton } from '@/components/ui/skeleton'
</script>

<template>
  <div>
    <Skeleton class="w-[100px] h-5 rounded-full my-2 bg-zinc-300" />
    <div v-for="n in 2" :key="n" class="flex items-center space-x-3">
      <Skeleton class="w-4 h-4 my-2 border-[1px] border-zinc-500 bg-zinc-300 rounded-sm" />
      <Skeleton class="w-[120px] h-4 rounded-full bg-zinc-300" />
    </div>
    <hr />
    <Skeleton class="w-[100px] h-5 rounded-full my-2 bg-zinc-300" />
    <div v-for="n in 3" :key="`second-${n}`" class="flex items-center space-x-3">
      <Skeleton class="w-4 h-4 my-2 border-[1px] border-zinc-500 bg-zinc-300 rounded-sm" />
      <Skeleton class="w-[120px] h-4 rounded-full bg-zinc-300" />
    </div>
  </div>
</template>
