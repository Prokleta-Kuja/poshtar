/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AddressType } from './AddressType'

export type AddressCM = {
  domainId: number
  pattern: string
  description?: string | null
  type: AddressType
}
