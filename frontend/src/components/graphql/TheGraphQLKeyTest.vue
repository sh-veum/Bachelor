<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import axios from 'axios'
import { useLazyQuery } from '@vue/apollo-composable'
import gql from 'graphql-tag'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { TextArea } from '@/components/ui/textarea'
import { provideApolloClient } from '@vue/apollo-composable'
import { apolloClient } from '../../main'

interface QueryField {
  id: string
  label: string
}

interface QueryInfo {
  queryName: string
  allowedFields: QueryField[]
}

const props = defineProps({
  accessKey: String
})

const queryInfos = ref<QueryInfo[]>([])
const selectedFields = ref<Record<string, string[]>>({})

// Fetch query information
const fetchQueryInfo = async (accessKey: string) => {
  if (
    accessKey &&
    /^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/g.test(accessKey)
  ) {
    try {
      const response = await axios.post(
        'http://localhost:8088/api/graphql/accesskey-graphql-permissions',
        {
          encryptedKey: accessKey
        }
      )
      queryInfos.value = response.data.map((query: any) => ({
        queryName: query.queryName,
        allowedFields: query.allowedFields.map((field: string) => ({ id: field, label: field }))
      }))
      queryInfos.value.forEach((info) => {
        selectedFields.value[info.queryName] = []
      })
    } catch (error) {
      console.error('Failed to fetch query information:', error)
    }
  }
}

watch(
  () => props.accessKey,
  (newAccessKey) => {
    if (newAccessKey) {
      fetchQueryInfo(newAccessKey)
    }
  },
  { immediate: true }
)

const toggleField = (queryName: string, fieldId: string) => {
  console.log('toggleField', queryName, fieldId)
  const index = selectedFields.value[queryName].indexOf(fieldId)
  if (index >= 0) {
    selectedFields.value[queryName].splice(index, 1)
  } else {
    selectedFields.value[queryName].push(fieldId)
  }
}

const responseData = ref('')

const combinedQuery = computed(() => {
  if (!props.accessKey) return ''

  const allQueries = queryInfos.value
    .filter((queryInfo) => selectedFields.value[queryInfo.queryName].length > 0)
    .map((queryInfo) => {
      const fields = selectedFields.value[queryInfo.queryName].join('\n        ')
      return `
        ${queryInfo.queryName}(encryptedKey: "${props.accessKey}") {
          ${fields}
        }
      `
    })
    .join('\n  ')

  if (!allQueries) return ''

  return gql`
    query CombinedQuery {
      ${allQueries}
    }
  `
})

const executeAllQueries = () => {
  if (!combinedQuery.value) {
    responseData.value = 'No fields selected for any query'
    return
  }

  // Ensure the Apollo Client is available in the current context
  provideApolloClient(apolloClient)

  const { load, onResult } = useLazyQuery(combinedQuery.value, () => ({
    encryptedKey: props.accessKey
  }))

  onResult((result) => {
    if (result.data) {
      const cleanedData = removeTypename(JSON.parse(JSON.stringify(result.data)))
      responseData.value = JSON.stringify(cleanedData, null, 2)
    } else {
      responseData.value = 'No data in query'
    }
  })

  load()
}

const removeTypename = (value: any) => {
  if (value && typeof value === 'object') {
    delete value.__typename
    Object.values(value).forEach((val) => removeTypename(val))
  }
  return value
}
</script>

<template>
  <div>
    <div v-for="queryInfo in queryInfos" :key="queryInfo.queryName">
      <h1 class="text-xl">{{ queryInfo.queryName }}</h1>
      <div
        v-for="field in queryInfo.allowedFields"
        :key="field.id"
        class="flex items-center space-x-3 space-y-0"
      >
        <Checkbox
          :id="field.id"
          @update:checked="() => toggleField(queryInfo.queryName, field.id)"
        />
        <label for="field.id" class="font-normal">{{ field.label }}</label>
      </div>
    </div>
    <Button @click="executeAllQueries" class="mt-2">Execute All Selected Queries</Button>
    <TextArea
      v-model="responseData"
      placeholder="Response data will appear here..."
      class="mt-2 h-[250px]"
    />
  </div>
</template>
