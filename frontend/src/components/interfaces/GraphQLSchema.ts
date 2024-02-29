export interface FieldType {
  name: string
  type: {
    kind: string
    name?: string
    ofType?: {
      name: string
    }
  }
}

export interface ObjectType {
  name: string
  fields: FieldType[]
}

export interface SchemaType {
  __schema: {
    types: ObjectType[]
  }
}

export interface PropertyInfo {
  __typename: string
  name: string
  propertyType: string
}

export interface ClassTable {
  __typename: string
  name: string
  properties: PropertyInfo[]
}

export interface Query {
  queryName: string
  queryResponseTable: string
}
