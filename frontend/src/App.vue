<script setup lang="ts">
import { RouterView } from 'vue-router'
import TheNavigation from './components/TheNavigation.vue'
</script>

<template>
  <TheNavigation class="mb-6 sticky top-0 left-0 right-0 z-20" />
  <!-- <header>
    <img alt="Vue logo" class="logo" src="@/assets/logo.svg" width="125" height="125" />

    <HelloWorld msg="You did it!" />
  </header> -->

  <RouterView />
</template>
