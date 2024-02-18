<script setup lang="ts">
import { computed, ref } from 'vue'
import { useLazyQuery } from '@vue/apollo-composable'
import gql from 'graphql-tag'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'

const encryptedKey = ref('')

// Define the GraphQL query
const FETCH_SPECIES_QUERY = gql`
  query getSpecies($encryptedKey: String!) {
    species(encryptedKey: $encryptedKey) {
      id
      name
    }
  }
`

const FETCH_ORGANIZATIONS_QUERY = gql`
  query getOrganizations($encryptedKey: String!) {
    organizations(encryptedKey: $encryptedKey) {
      orgNo
      name
    }
  }
`

// Setup the lazy query with a function to dynamically pass variables
const { result: speciesResult, load: loadSpecies } = useLazyQuery(FETCH_SPECIES_QUERY, () => ({
  encryptedKey: encryptedKey.value
}))

const { result: organizationsResult, load: loadOrganizations } = useLazyQuery(
  FETCH_ORGANIZATIONS_QUERY,
  () => ({
    encryptedKey: encryptedKey.value
  })
)

const speciesList = computed(() => speciesResult.value?.species ?? [])
const organizationsList = computed(() => organizationsResult.value?.organizations ?? [])

// Function to trigger the query
const fetchSpecies = () => {
  if (encryptedKey.value) {
    loadSpecies()
  }
}
const fetchOrganizations = () => {
  if (encryptedKey.value) {
    loadOrganizations()
  }
}
</script>

<template>
  <div class="ml-6 w-[800px] border-black border-2 p-4">
    <Input v-model="encryptedKey" placeholder="Place encryptedKey here" class="mb-4" />
    <Button @click="fetchSpecies" class="bg-blue-500 hover:bg-blue-700">Get Species</Button>
    <Button @click="fetchOrganizations" class="bg-green-500 hover:bg-green-700 ml-4"
      >Get Organizations</Button
    >
    <!-- <div v-if="speciesLoading || organizationsLoading">Loading...</div>
    <div v-if="speciesError || organizationsError">
      {{ speciesError?.message || organizationsError?.message }}
    </div> -->
    <ul class="my-4">
      <li v-for="(species, index) of speciesList" :key="index" class="list-disc ml-6">
        {{ species.id }} - {{ species.name }}
      </li>
    </ul>
    <ul class="my-4">
      <li v-for="(organizations, index) of organizationsList" :key="index" class="list-disc ml-6">
        {{ organizations.orgNo }} - {{ organizations.name }}
      </li>
    </ul>
  </div>
</template>
