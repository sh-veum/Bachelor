<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import HelloWorld from './components/HelloWorld.vue'
import TheNavigation from './components/TheNavigation.vue'
</script>

<template>
  <TheNavigation />
  <header>
    <img alt="Vue logo" class="logo" src="@/assets/logo.svg" width="125" height="125" />

    <HelloWorld msg="You did it!" />
  </header>

  <RouterView />
</template>
