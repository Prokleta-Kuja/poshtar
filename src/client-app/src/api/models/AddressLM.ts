/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AddressType } from './AddressType'

export type AddressLM = {
  id: number
  domainId: number
  domainName: string
  pattern: string
  description?: string | null
  type: AddressType
  disabled?: string | null
  userCount: number
}
