<script setup lang="ts">
import { ref } from 'vue'
import { fetchSchema } from '@/lib/fetchSchema'
import { Button } from '@/components/ui/button'
import { TextArea } from '@/components/ui/textarea'
import type { SchemaType } from './interfaces/GraphQLSchema'

const responseData = ref<string>('')
const schemaData = ref<SchemaType | null>(null)
const objectTypesToFetch = ref<string[]>(['Organization', 'Species'])

// Fetch the schema once and store it for later processing
const getSchemaData = async () => {
  if (!schemaData.value) {
    try {
      const schema = await fetchSchema()
      schemaData.value = schema // Assuming fetchSchema now correctly returns the expected structure
    } catch (error) {
      console.error(error)
      responseData.value = 'Failed to fetch schema data. Check the console for details.'
    }
  }
}

// Extract queries like "species" and "organizations" from the schema
const getAvailableQueries = async () => {
  await getSchemaData()
  if (schemaData.value && schemaData.value.__schema.types) {
    const queryType = schemaData.value.__schema.types.find((type) => type.name === 'Query')
    if (queryType && queryType.fields) {
      const queries = queryType.fields
        .map((field) => {
          const typeName = resolveTypeName(field.type)
          return `${field.name}(...): [${typeName}!]`
        })
        .join('\n')
      responseData.value = queries
    } else {
      responseData.value = 'No query types found.'
    }
  } else {
    responseData.value = 'Schema data is not properly initialized.'
  }
}

// Extract object types like "Organization" and "Species" from the schema
const getObjects = async () => {
  await getSchemaData()
  if (schemaData.value && schemaData.value.__schema.types) {
    const objects = schemaData.value.__schema.types.filter((type) =>
      objectTypesToFetch.value.includes(type.name)
    )
    if (objects.length > 0) {
      const objectDetails = objects
        .map((obj) => {
          const fields = obj.fields.map(
            (field) =>
              `${field.name}: ${field.type.ofType?.name ?? field.type.name}${field.type.kind === 'NON_NULL' ? '!' : ''}`
          )
          return `${obj.name} {\n  ${fields.join('\n  ')}\n}`
        })
        .join('\n\n')
      responseData.value = objectDetails
    } else {
      responseData.value = 'No object types found.'
    }
  } else {
    responseData.value = 'Schema data is not properly initialized.'
  }
}

function resolveTypeName(type: any): string {
  // If the type has a name, return it directly
  if (type.name) {
    return type.name
  }
  // If the type does not have a name but has an ofType, recurse into ofType
  if (type.ofType) {
    return resolveTypeName(type.ofType)
  }
  // Fallback case, should not typically be reached
  return 'UnknownType'
}
</script>

<template>
  <div>
    <TextArea
      v-model="responseData"
      class="mt-2 h-[250px] w-full"
      placeholder="GraphQL response will appear here..."
    />
    <div class="flex gap-2">
      <Button @click="getAvailableQueries" class="bg-purple-500 hover:bg-purple-700 mt-2"
        >Fetch Available Queries</Button
      >
      <Button @click="getObjects" class="bg-purple-500 hover:bg-purple-700 mt-2 px-2"
        >Fetch Objects</Button
      >
    </div>
  </div>
</template>

