/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type AntiSpamSettings = {
  banMinutes?: number
  tarpitSeconds?: number
  enableFCrDNSWorkarounds?: boolean
  consecutiveCmdFail?: number
  banConsecutiveCmdFail?: boolean
  tarpitConsecutiveCmdFail?: boolean
  consecutiveRcptFail?: number
  banConsecutiveRcptFail?: boolean
  tarpitConsecutiveRcptFail?: boolean
  asnBlacklist?: Array<string>
  banAsnBlacklist?: boolean
  tarpitAsnBlacklist?: boolean
  clientBlacklist?: Array<string>
  banClientBlacklist?: boolean
  tarpitClientBlacklist?: boolean
  enforceForwardDns?: boolean
  banForwardDns?: boolean
  tarpitForwardDns?: boolean
  enforceReverseDns?: boolean
  banReverseDns?: boolean
  tarpitReverseDns?: boolean
  enforceDnsBlockList?: boolean
  banDnsBlockList?: boolean
  tarpitDnsBlockList?: boolean
  enforceSpf?: boolean
  banSpf?: boolean
  tarpitSpf?: boolean
}
