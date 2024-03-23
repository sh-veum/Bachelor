import type { UUID } from "crypto"

export interface Theme {
    id: UUID
    themeName: string
    accessibleEndpoints: string[]
    isDeprecated: boolean
  }
  
export interface Key {
    id: UUID
    keyName: string
    createdBy: string
    expiresIn: number
    isEnabled: boolean
    themes: Theme[]
  }