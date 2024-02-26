<script setup lang="ts">
import { computed, ref } from 'vue'
import { useLazyQuery } from '@vue/apollo-composable'
import gql from 'graphql-tag'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { TextArea } from '@/components/ui/textarea'
import { Checkbox } from '@/components/ui/checkbox'
import TheGraphQLTester from '@/components/TheGraphQLTester.vue'

const encryptedKey = ref('')
const responseData = ref('')
// Fields selected by the user
const selectedSpeciesFields = ref(['id', 'name'])
const selectedOrganizationFields = ref(['id', 'name'])
// Fields to be used in the query
const querySpeciesFields = ref([...selectedSpeciesFields.value])
const queryOrganizationFields = ref([...selectedOrganizationFields.value])

const speciesOptions = [
  { id: 'id', label: 'ID' },
  { id: 'name', label: 'Name' }
]

const organizationOptions = [
  { id: 'id', label: 'ID' },
  { id: 'orgNo', label: 'OrgNo' },
  { id: 'name', label: 'Name' },
  { id: 'address', label: 'Address' },
  { id: 'postalCode', label: 'PostalCode' },
  { id: 'city', label: 'City' }
]

// Computed property to dynamically construct the species query
const speciesQuery = computed(() => {
  const fields = querySpeciesFields.value.join('\n      ')
  return gql`
    query getSpecies($encryptedKey: String!) {
      species(encryptedKey: $encryptedKey) {
        ${fields}
      }
    }
  `
})

// Computed property to dynamically construct the organizations query
const organizationsQuery = computed(() => {
  const fields = queryOrganizationFields.value.join('\n      ')
  return gql`
    query getOrganizations($encryptedKey: String!) {
      organizations(encryptedKey: $encryptedKey) {
        ${fields}
      }
    }
  `
})

// Setup the lazy queries with dynamic query and variables
const { load: loadSpecies, onResult: onResultSpecies } = useLazyQuery(speciesQuery, () => ({
  encryptedKey: encryptedKey.value
}))

const { load: loadOrganizations, onResult: onResultOrganizations } = useLazyQuery(
  organizationsQuery,
  () => ({
    encryptedKey: encryptedKey.value
  })
)

onResultSpecies((result) => {
  if (result.data && result.data.species) {
    responseData.value = JSON.stringify({ data: { species: result.data.species } }, null, 2)
  } else {
    responseData.value = 'Species data is not available'
  }
})

onResultOrganizations((result) => {
  if (result.data && result.data.organizations) {
    responseData.value = JSON.stringify(
      { data: { organizations: result.data.organizations } },
      null,
      2
    )
  } else {
    responseData.value = 'Organizations data is not available'
  }
})

// Fetch functions with field updates
const fetchSpecies = () => {
  if (selectedSpeciesFields.value.length === 0) {
    // No species fields selected, so set the response data accordingly
    responseData.value = JSON.stringify(
      { data: { message: 'Species data is not available' } },
      null,
      2
    )
    return
  }
  querySpeciesFields.value = [...selectedSpeciesFields.value]
  loadSpecies()
}

const fetchOrganizations = () => {
  if (selectedOrganizationFields.value.length === 0) {
    // No organization fields selected, so set the response data accordingly
    responseData.value = JSON.stringify(
      { data: { message: 'Organizations data is not available' } },
      null,
      2
    )
    return
  }
  queryOrganizationFields.value = [...selectedOrganizationFields.value]
  loadOrganizations()
}

// Function to toggle species field selection
const toggleSpeciesField = (fieldId: string) => {
  const index = selectedSpeciesFields.value.indexOf(fieldId)
  if (index >= 0) {
    selectedSpeciesFields.value.splice(index, 1)
  } else {
    selectedSpeciesFields.value.push(fieldId)
  }
}

// Function to toggle organization field selection
const toggleOrganizationField = (fieldId: string) => {
  const index = selectedOrganizationFields.value.indexOf(fieldId)
  if (index >= 0) {
    selectedOrganizationFields.value.splice(index, 1)
  } else {
    selectedOrganizationFields.value.push(fieldId)
  }
}
</script>

<template>
  <div class="ml-6 w-[800px] bg-zinc-50 rounded-lg shadow dark:border dark:border-gray-700 p-4">
    <h1 class="text-3xl font-bold mb-4">GraphQL Tester</h1>
    <Input v-model="encryptedKey" placeholder="Place encryptedKey here" class="mb-4" />
    <div>
      <p class="text-xl">Select Species fields:</p>
      <div
        v-for="option in speciesOptions"
        :key="option.id"
        class="flex items-center space-x-3 space-y-0"
      >
        <Checkbox
          :checked="selectedSpeciesFields.includes(option.id)"
          @update:checked="() => toggleSpeciesField(option.id)"
        />
        <p class="font-normal">{{ option.label }}</p>
      </div>
      <Button @click="fetchSpecies" class="bg-blue-500 hover:bg-blue-700 mt-2">Get Species</Button>
    </div>
    <div class="mt-4">
      <p class="text-xl">Select Organizations fields:</p>
      <div
        v-for="option in organizationOptions"
        :key="option.id"
        class="flex items-center space-x-3 space-y-0"
      >
        <Checkbox
          :checked="selectedOrganizationFields.includes(option.id)"
          @update:checked="() => toggleOrganizationField(option.id)"
        />
        <p class="font-normal">{{ option.label }}</p>
      </div>
      <Button @click="fetchOrganizations" class="bg-green-500 hover:bg-green-700 mt-2"
        >Get Organizations</Button
      >
    </div>
    <TextArea
      v-model="responseData"
      class="mt-2 h-[250px] w-full"
      placeholder="GraphQL response will appear here..."
    />
    <TheGraphQLTester />
  </div>
</template>
