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
