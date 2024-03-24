import { gql, type ApolloQueryResult, ApolloError } from '@apollo/client/core'
import { apolloClient } from '../main'
import type {
  ClassTable,
  GraphQLKey,
  GraphQLPermission,
  Query
} from '@/components/interfaces/GraphQLSchema'
import type { UUID } from 'crypto'

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
      id
      keyName
      expiresIn
      isEnabled
      graphQLAccessKeyPermissionDto {
        queryName
        allowedFields
      }
    }
  }
`

const TOGGLE_GRAPHQL_KEY = gql`
  mutation ToggleApiKey($id: UUID!, $isEnabled: Boolean!, $keyType: String!) {
    toggleApiKey(toggleApiKeyStatusDto: { id: $id, isEnabled: $isEnabled, keyType: $keyType }) {
      isSuccess
      message
    }
  }
`

const DELETE_GRAPHQL_KEY = gql`
  mutation DeleteGraphQLApiKey($id: UUID!) {
    deleteGraphQLApiKey(id: $id) {
      isSuccess
      message
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
    id: UUID
    keyName: string
    expiresIn: number
    isEnabled: boolean
    graphQLAccessKeyPermissionDto: {
      queryName: string
      allowedFields: string[]
    }[]
  }[]
}

interface ToggleApiKeyResponse {
  toggleApiKey: {
    isSuccess: boolean
    message: string
  }
}

interface DeleteGraphQLApiKeyResponse {
  deleteGraphQLApiKey: {
    isSuccess: boolean
    message: string
  }
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
      id: apiKey.id,
      keyName: apiKey.keyName,
      expiresIn: apiKey.expiresIn,
      isEnabled: apiKey.isEnabled,
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
    if (error instanceof ApolloError) {
      console.error('GraphQL Error:', error.message)
    } else {
      console.error('Error fetching GraphQL keys:', error)
    }
    throw new Error('Failed to fetch GraphQL keys')
  }
}

export async function toggleApiKey(
  id: UUID,
  isEnabled: boolean,
  keyType: string
): Promise<ToggleApiKeyResponse> {
  try {
    const response = await apolloClient.mutate<ToggleApiKeyResponse>({
      mutation: TOGGLE_GRAPHQL_KEY,
      variables: { id, isEnabled, keyType }
    })

    if (response.data) {
      return response.data
    } else {
      throw new Error('No data returned from toggleApiKey mutation')
    }
  } catch (error) {
    if (error instanceof ApolloError) {
      console.error('GraphQL Error:', error.message)
    } else {
      console.error('Error toggling API key:', error)
    }
    throw new Error('Error toggling API key')
  }
}

export async function deleteGraphQLApiKey(id: UUID): Promise<DeleteGraphQLApiKeyResponse> {
  try {
    const response = await apolloClient.mutate<DeleteGraphQLApiKeyResponse>({
      mutation: DELETE_GRAPHQL_KEY,
      variables: { id }
    })

    if (response.data) {
      return response.data
    } else {
      throw new Error('No data returned from deleteGraphQLApiKey mutation')
    }
  } catch (error) {
    if (error instanceof ApolloError) {
      console.error('GraphQL Error:', error.message)
    } else {
      console.error('Error deleting API key:', error)
    }
    throw new Error('Error deleting API key')
  }
}
