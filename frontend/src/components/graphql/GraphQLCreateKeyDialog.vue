<script setup lang="ts">
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Checkbox } from '@/components/ui/checkbox'
import GraphQLTablesSkeleton from '@/components/graphql/GraphQLTablesSkeleton.vue'
import { ref, computed } from 'vue'
import { useMutation } from '@vue/apollo-composable'
import gql from 'graphql-tag'
import { fetchAvailableClassTables, fetchAvailableQueries } from '@/lib/graphQL'
import type { ClassTable, Query } from '@/components/interfaces/GraphQLSchema'
import type { GraphQLKey } from '@/components/interfaces/GraphQLSchema'

const keyName = ref('')
const availableClassTables = ref<ClassTable[]>([])
const availableQueries = ref<Query[]>([])
const selectedFields = ref<Record<string, string[]>>({})
const isOptionsLoading = ref(true)
const keyNameErrorMessage = ref('')
const fieldsErrorMessage = ref('')

const hasSelectedFields = computed(() => {
  return Object.values(selectedFields.value).some((fields) => fields.length > 0)
})

const emit = defineEmits(['submit'])

const isKeyCreatedDialogOpen = ref(false)
const createdKey = ref('')

const fetchOptions = async () => {
  // TODO: REMEMBER TO REMOVE THIS DELAY
  // should maybe fetch the options in onMounted instead of right when the dialog is opened?
  await new Promise((resolve) => setTimeout(resolve, 1000))

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
    await createGraphQLKey({
      keyName: keyName.value,
      permissions
    }).then((response) => {
      if (response && response.data && response.data.createGraphQLAccessKey) {
        createdKey.value = response.data.createGraphQLAccessKey.encryptedKey
        isKeyCreatedDialogOpen.value = true
        emit('submit')
      }
    })
  } catch (e) {
    console.error('Error creating GraphQL access key:', e)
  }
}

const closeKeyCreatedDialog = () => {
  isKeyCreatedDialogOpen.value = false
}
</script>

<template>
  <Dialog>
    <DialogTrigger as-child>
      <Button @click="fetchOptions">Create Key</Button>
    </DialogTrigger>
    <DialogContent class="grid-rows-[auto_minmax(0,1fr)_auto] p-0 max-h-[90dvh]">
      <DialogHeader class="mx-6 mt-6">
        <DialogTitle>Create Key</DialogTitle>
        <DialogDescription> Create a new key to access the GraphQL API. </DialogDescription>
      </DialogHeader>
      <div class="grid gap-2 my-2 overflow-y-auto px-6">
        <div class="flex flex-col justify-between">
          <Input v-model="keyName" placeholder="Write key name here" class="my-2" />
          <p v-if="keyNameErrorMessage" class="text-red-500">{{ keyNameErrorMessage }}</p>
          <p class="text-2xl font-bold mt-2">Available Query Class Tables</p>
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
                class="flex items-center space-x-3 h-6"
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
        </div>
      </div>
      <DialogFooter class="p-6 pt-0 sm:justify-start">
        <DialogClose as-child>
          <Button
            class="mt-2"
            @click="createKey"
            type="submit"
            v-bind:disabled="!keyName || !hasSelectedFields"
            >Create Key</Button
          >
        </DialogClose>
      </DialogFooter>
    </DialogContent>
  </Dialog>
  <Dialog v-model:open="isKeyCreatedDialogOpen">
    <DialogContent class="p-4">
      <DialogHeader>
        <DialogTitle>Key Created Successfully</DialogTitle>
      </DialogHeader>
      <div class="overflow-auto">
        <p class="mt-4">Your encrypted key is:</p>
        <br />
        <p class="font-bold break-words">{{ createdKey }}</p>
        <br />
        <p class="text-red-500 font-bold italic break-words">
          Store the key, you won't be able to see it again when you close the dialog
        </p>
      </div>
      <DialogFooter class="mt-4">
        <Button @click="closeKeyCreatedDialog">Close</Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
