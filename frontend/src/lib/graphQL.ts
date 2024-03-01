import { gql, type ApolloQueryResult } from '@apollo/client/core'
import { apolloClient } from '../main'
import type { ClassTable, Query } from '@/components/interfaces/GraphQLSchema'

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

const GET_GRAPHQL_KEYS = gql`
  query GetGraphQLKeys {
    graphQLApiKeysByUser {
      keyName
      expiresIn
      graphQLAccessKeyPermissionDto {
        queryName
        allowedFields
      }
    }
  }
`

interface AvailableClassTablesResponse {
  availableClassTables: ClassTable[]
}

interface AvailableQueriesResponse {
  availableQueries: Query[]
}

interface GraphQLKeysResponse {
  graphQLApiKeysByUser: {
    keyName: string
    expiresIn: number
    graphQLAccessKeyPermissionDto: {
      queryName: string
      allowedFields: string[]
    }[]
  }[]
}

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

export async function fetchGraphQLKeys(): Promise<GraphQLKey[]> {
  try {
    const response: ApolloQueryResult<GraphQLKeysResponse> = await apolloClient.query({
      query: GET_GRAPHQL_KEYS,
      fetchPolicy: 'network-only' // Ignore cache, force network request
    })

    const formattedResponse: GraphQLKey[] = response.data.graphQLApiKeysByUser.map((apiKey) => ({
      keyName: apiKey.keyName,
      expiresIn: apiKey.expiresIn,
      permissions: apiKey.graphQLAccessKeyPermissionDto.map(
        (permission) =>
          ({
            queryName: permission.queryName,
            allowedFields: permission.allowedFields
          }) as GraphQLPermission
      )
    }))

    return formattedResponse
  } catch (error) {
    console.error('Error fetching GraphQL keys:', error)
    throw new Error('Failed to fetch GraphQL keys')
  }
}
