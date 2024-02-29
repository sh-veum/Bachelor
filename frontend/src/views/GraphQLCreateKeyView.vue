<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useMutation } from '@vue/apollo-composable'
import gql from 'graphql-tag'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { fetchAvailableClassTables, fetchAvailableQueries } from '@/lib/graphQL'
import { Checkbox } from '@/components/ui/checkbox'
import GraphQLTablesSkeleton from '@/components/GraphQLTablesSkeleton.vue'
import type { ClassTable, Query } from '@/components/interfaces/GraphQLSchema'
import type { fieldNameFromStoreName } from '@apollo/client/cache'

const keyName = ref('')
const availableClassTables = ref<ClassTable[]>([])
const availableQueries = ref<Query[]>([])
const selectedFields = ref<Record<string, string[]>>({})
const isOptionsLoading = ref(false)
const keyNameErrorMessage = ref('')
const fieldsErrorMessage = ref('')

const hasSelectedFields = computed(() => {
  return Object.values(selectedFields.value).some((fields) => fields.length > 0)
})

const fetchOptions = async () => {
  isOptionsLoading.value = true
  try {
    const [classTablesResponse, queriesResponse] = await Promise.all([
      fetchAvailableClassTables(),
      fetchAvailableQueries()
    ])

    availableClassTables.value = classTablesResponse.availableClassTables
    availableQueries.value = queriesResponse.availableQueries
  } catch (error) {
    console.error('Error fetching data:', error)
  } finally {
    isOptionsLoading.value = false
  }
}

const toggleSelectedField = (tableName: string, propertyName: string) => {
  const tableFields = selectedFields.value[tableName] || []
  if (tableFields.includes(propertyName)) {
    selectedFields.value[tableName] = tableFields.filter((field) => field !== propertyName)
  } else {
    selectedFields.value[tableName] = [...tableFields, propertyName]
  }
}

onMounted(() => {
  fetchOptions()
})

const CREATE_GRAPHQL_KEY_MUTATION = gql`
  mutation CreateGraphQLAccessKey($keyName: String!, $permissions: [AccessKeyPermissionInput!]!) {
    createGraphQLAccessKey(keyName: $keyName, permissions: $permissions) {
      encryptedKey
    }
  }
`

// Use the mutation with Vue Apollo
const { mutate: createGraphQLKey } = useMutation(CREATE_GRAPHQL_KEY_MUTATION)

const createKey = async () => {
  keyNameErrorMessage.value = ''
  fieldsErrorMessage.value = ''

  if (!keyName.value.trim()) {
    keyNameErrorMessage.value = 'Key name is required.'
  }
  if (!hasSelectedFields.value) {
    fieldsErrorMessage.value = 'At least one field must be selected.'
  }

  if (!keyName.value.trim() || !hasSelectedFields.value) {
    return
  }

  const permissions = Object.entries(selectedFields.value).map(([queryName, allowedFields]) => ({
    queryName,
    allowedFields: allowedFields.map((field) => field.charAt(0).toLowerCase() + field.slice(1))
  }))

  try {
    const response = await createGraphQLKey({
      keyName: keyName.value,
      permissions
    })
    if (response && response.data && response.data.createGraphQLAccessKey) {
      console.log('Encrypted Key:', response.data.createGraphQLAccessKey.encryptedKey)
    }
  } catch (e) {
    console.error('Error creating GraphQL access key:', e)
  }
}
</script>

<template>
  <div class="mx-6 w-[500px]">
    <Input v-model="keyName" placeholder="Write key name here" />
    <p v-if="keyNameErrorMessage" class="text-red-500">{{ keyNameErrorMessage }}</p>
    <h1 class="text-2xl font-bold mt-2">Available Query Class Tables</h1>
    <hr class="my-2" />
    <div v-if="isOptionsLoading">
      <GraphQLTablesSkeleton />
    </div>
    <ul>
      <li v-for="(query, index) in availableQueries" :key="index">
        <p class="text-lg font-bold">{{ query.queryResponseTable }}</p>
        <div
          v-for="property in availableClassTables[index].properties"
          :key="`property-${property.name}`"
          class="flex items-center space-x-3"
        >
          <Checkbox
            :checked="selectedFields[query.queryName]?.includes(property.name)"
            @update:checked="() => toggleSelectedField(query.queryName, property.name)"
          />
          <p class="font-normal">{{ property.name }}</p>
        </div>
        <hr class="my-2" />
      </li>
    </ul>
    <p v-if="fieldsErrorMessage" class="text-red-500">{{ fieldsErrorMessage }}</p>
    <Button class="mt-2" @click="createKey">Create Key</Button>
  </div>
</template>
