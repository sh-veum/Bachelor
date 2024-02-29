import { ref } from 'vue'
import { fetchSchema } from '@/lib/fetchSchema'
import type { SchemaType } from '@/components/interfaces/GraphQLSchema'

const schemaData = ref<SchemaType | null>(null)
const objectTypesToFetch = ['Organization', 'Species', 'ClassInfo']

// Helper function to resolve type names recursively
const resolveTypeName = (type: any): string => {
  if (type.name) {
    return type.name
  }
  if (type.ofType) {
    return resolveTypeName(type.ofType)
  }
  return 'UnknownType'
}

const getSchemaData = async (): Promise<void> => {
  if (!schemaData.value) {
    try {
      const schema = await fetchSchema()
      schemaData.value = schema
    } catch (error) {
      console.error(error)
    }
  }
}

// DEPRECATED, exposes whole schema to frotnend.
export const getAvailableQueries = async (): Promise<string> => {
  await getSchemaData()
  let queriesData = ''
  if (schemaData.value && schemaData.value.__schema.types) {
    const queryType = schemaData.value.__schema.types.find((type) => type.name === 'Query')
    if (queryType && queryType.fields) {
      const queries = queryType.fields
        .map((field) => {
          const typeName = resolveTypeName(field.type)
          return `${field.name}(...): [${typeName}!]`
        })
        .join('\n')
      queriesData = queries
    } else {
      queriesData = 'No query types found.'
    }
  } else {
    queriesData = 'Schema data is not properly initialized.'
  }
  console.log(queriesData)
  return queriesData
}

// DEPRECATED, exposes whole schema to frotnend.
export const getObjects = async (): Promise<string> => {
  await getSchemaData()
  let objectsData = ''
  if (schemaData.value && schemaData.value.__schema.types) {
    const objects = schemaData.value.__schema.types.filter((type) =>
      objectTypesToFetch.includes(type.name)
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
      objectsData = objectDetails
    } else {
      objectsData = 'No object types found.'
    }
  } else {
    objectsData = 'Schema data is not properly initialized.'
  }
  console.log(objectsData)
  return objectsData
}
