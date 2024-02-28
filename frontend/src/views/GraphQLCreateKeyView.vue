<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useMutation } from '@vue/apollo-composable'
import gql from 'graphql-tag'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { fetchAvailableClassTables, fetchAvailableQueries } from '@/lib/graphQL'
import { Checkbox } from '@/components/ui/checkbox'
import type { ClassTable, Query } from '@/components/interfaces/GraphQLSchema'

const keyName = ref('')
const availableClassTables = ref<ClassTable[]>([])
const availableQueries = ref<Query[]>([])
const selectedFields = ref<Record<string, string[]>>({})

const fetchOptions = async () => {
  const classTablesResponse = await fetchAvailableClassTables()
  const queriesResponse = await fetchAvailableQueries()

  availableClassTables.value = classTablesResponse.availableClassTables
  availableQueries.value = queriesResponse.availableQueries
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
  if (!keyName.value.trim()) {
    console.error('Key name is required')
    return
  }

  const permissions = Object.entries(selectedFields.value).map(([queryName, allowedFields]) => ({
    queryName,
    allowedFields: allowedFields.map((field) => field.charAt(0).toLowerCase() + field.slice(1))
  }))

  console.log('Creating key with keyName:', keyName.value)
  console.log('Creating key with permissions:', permissions)

  try {
    const response = await createGraphQLKey({
      keyName: keyName.value ?? 'no name',
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
    <h1 class="text-xl">Available Class Tables</h1>
    <ul>
      <li v-for="(query, index) in availableQueries" :key="index">
        {{ query.queryResponseTable }}
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
      </li>
    </ul>
    <Button class="mt-2" @click="createKey">Create Key</Button>
  </div>
</template>
