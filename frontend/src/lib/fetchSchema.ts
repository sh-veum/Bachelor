import { gql } from '@apollo/client/core'
import { apolloClient } from '../main'
import type { SchemaType } from '@/components/interfaces/GraphQLSchema'

const introspectionQuery = gql`
  query IntrospectionQuery {
    __schema {
      queryType {
        name
      }
      mutationType {
        name
      }
      subscriptionType {
        name
      }
      types {
        ...FullType
      }
      directives {
        name
        description
        args(includeDeprecated: true) {
          ...InputValue
        }
        isRepeatable
        locations
      }
    }
  }

  fragment FullType on __Type {
    kind
    name
    description
    specifiedByURL
    fields(includeDeprecated: true) {
      name
      description
      args(includeDeprecated: true) {
        ...InputValue
      }
      type {
        ...TypeRef
      }
      isDeprecated
      deprecationReason
    }
    inputFields(includeDeprecated: true) {
      ...InputValue
    }
    interfaces {
      ...TypeRef
    }
    enumValues(includeDeprecated: true) {
      name
      description
      isDeprecated
      deprecationReason
    }
    possibleTypes {
      ...TypeRef
    }
  }

  fragment InputValue on __InputValue {
    name
    description
    type {
      ...TypeRef
    }
    defaultValue
    isDeprecated
    deprecationReason
  }

  fragment TypeRef on __Type {
    kind
    name
    ofType {
      kind
      name
      ofType {
        kind
        name
        ofType {
          kind
          name
          ofType {
            kind
            name
            ofType {
              kind
              name
            }
          }
        }
      }
    }
  }
`

export const fetchSchema = async (): Promise<SchemaType> => {
  try {
    const { data } = await apolloClient.query({ query: introspectionQuery })
    console.log(data)
    // Assuming `data` contains the `__schema` field directly as shown in your output
    return data // Return the data directly
  } catch (error) {
    console.error('Error fetching schema:', error)
    throw new Error('Failed to fetch schema data')
  }
}
