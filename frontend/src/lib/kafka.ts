import axios from 'axios'

export interface WaterQualityLog {
  id: number
  offset: number
  timeStamp: string
  ph: number
  turbidity: number
  temperature: number
}

export interface BoatLocationLog {
  offset: number
  timeStamp: string
  latitude: number
  longitude: number
}
export interface KafkaKey {
  keyName: string
  createdBy: string
  expiresIn: number
  isEnabled: boolean
  topics: string[]
}

export interface KafkaTopicAndId {
  sensorId: string
  topics: string[]
}

export const createKafkaKey = async (keyName: string, topics: string[]) => {
  try {
    const response = await axios.post('http://localhost:8088/api/kafka/create-accesskey', {
      keyName,
      topics
    })
    return response.data
  } catch (error) {
    console.error('Failed to create kafka key:', error)
  }
}

export const fetchKafkaKeys = async () => {
  try {
    const keysResponse = await axios.get('http://localhost:8088/api/kafka/get-keys-by-user')
    return keysResponse.data
  } catch (error) {
    console.error('Failed to fetch keys:', error)
  }
}

export const deleteKafkaKey = async (id: string) => {
  try {
    const keysResponse = await axios.delete(
      `http://localhost:8088/api/kafka/delete-accesskey?id=${id}`,
      {}
    )
    return keysResponse.status
  } catch (error) {
    console.error('Failed to fetch keys:', error)
  }
}

export const toggleKafkaKeyEnabledStatus = async (id: string, isEnabled: boolean) => {
  try {
    const response = await axios.patch('http://localhost:8088/api/kafka/toggle-key', {
      id: id,
      isEnabled: isEnabled,
      keyType: 'kafka'
    })

    return response.status
  } catch (error) {
    console.error('Error toggling key enabled status:', error)
  }
}

export const getAvailableKafkatopics = async () => {
  try {
    const response = await axios.get('http://localhost:8088/api/kafka/get-available-topics')
    return response.data
  } catch (error) {
    console.error('Error fetching available topics:', error)
  }
}

export const fetchKafkaKeyTopics = async (
  accessKey: string
): Promise<KafkaTopicAndId | { error: string }> => {
  if (
    accessKey &&
    /^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/g.test(accessKey)
  ) {
    try {
      const response = await axios.post('http://localhost:8088/api/kafka/accesskey-kafka-topics', {
        encryptedKey: accessKey
      })
      return response.data as KafkaTopicAndId
    } catch (error: any) {
      if (axios.isAxiosError(error) && error.response?.status === 400) {
        return { error: error.response.data.message || 'API key is not found or disabled.' }
      }
      console.error('Failed to fetch query information:', error)
      return { error: 'An unexpected error occurred.' }
    }
  } else {
    return { error: 'Invalid access key format.' }
  }
}

export const subscribeToAllTopics = async (accessKey: string): Promise<void> => {
  if (
    accessKey &&
    /^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/g.test(accessKey)
  ) {
    try {
      const response = await axios.patch('http://localhost:8088/api/kafka/subscribe-to-topics', {
        encryptedKey: accessKey
      })
      console.log('Message: ' + response.data.message)
    } catch (error) {
      console.error('Failed to subscribe to all topics:', error)
    }
  } else {
    console.error('Invalid access key format.')
  }
}
