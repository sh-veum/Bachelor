import { gql, type ApolloQueryResult } from '@apollo/client/core'
import { apolloClient } from '../main'
import type { ClassTable, Query } from '@/components/interfaces/GraphQLSchema'

interface AvailableClassTablesResponse {
  availableClassTables: ClassTable[]
}

interface AvailableQueriesResponse {
  availableQueries: Query[]
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

    return response.data
  } catch (error) {
    console.error('Error fetching available class tables:', error)
    throw new Error('Failed to fetch available class tables')
  }
}

export async function fetchAvailableQueries(): Promise<AvailableQueriesResponse> {
  try {
    const response: ApolloQueryResult<{ availableQueries: [string, string][] }> =
      await apolloClient.query({
        query: GET_AVAILABLE_QUERIES
      })

    const formattedResponse: AvailableQueriesResponse = {
      availableQueries: response.data.availableQueries.map(([queryName, queryResponseTable]) => ({
        queryName,
        queryResponseTable
      }))
    }

    return formattedResponse
  } catch (error) {
    console.error('Error fetching available queries:', error)
    throw new Error('Failed to fetch available queries')
  }
}
