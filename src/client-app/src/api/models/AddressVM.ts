/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AddressType } from './AddressType'

export type AddressVM = {
  id: number
  domainId: number
  pattern: string
  description?: string | null
  type: AddressType
  disabled?: string | null
}
