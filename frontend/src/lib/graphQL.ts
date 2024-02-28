// Import necessary functions and types from Apollo Client
import { gql, type ApolloQueryResult } from '@apollo/client/core'
import { apolloClient } from '../main' // Assumed path to your Apollo Client instance

// Define TypeScript interfaces for the query response to ensure type safety
interface Property {
  name: string
  propertyType: string
}

interface ClassTable {
  name: string
  properties: Property[]
}

interface AvailableClassTablesResponse {
  availableClassTables: ClassTable[]
}

const GET_AVAILABLE_CLASS_TABLES = gql`
  query GetAvailableClassTables {
    availableClassTables {
      name
      properties {
        name
        propertyType
      }
    }
  }
`

const GET_AVAILABLE_QUERIES = gql`
  query GetAvailableQueries {
    availableQueries
  }
`

export async function fetchAvailableClassTables(): Promise<AvailableClassTablesResponse> {
  try {
    const response: ApolloQueryResult<AvailableClassTablesResponse> = await apolloClient.query({
      query: GET_AVAILABLE_CLASS_TABLES
    })

    console.log(response.data)

    return response.data
  } catch (error) {
    console.error('Error fetching available class tables:', error)
    throw new Error('Failed to fetch available class tables')
  }
}

export async function fetchAvailableQueries(): Promise<string[]> {
  try {
    const response: ApolloQueryResult<string[]> = await apolloClient.query({
      query: GET_AVAILABLE_QUERIES
    })

    console.log(response.data)

    return response.data
  } catch (error) {
    console.error('Error fetching available queries:', error)
    throw new Error('Failed to fetch available queries')
  }
}
