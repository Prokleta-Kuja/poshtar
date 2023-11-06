/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { RecipientLM } from './RecipientLM'

export type RecipientLMListResponse = {
  items: Array<RecipientLM>
  size: number
  page: number
  total: number
  ascending: boolean
  sortBy?: string | null
}
