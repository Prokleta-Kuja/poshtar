/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
export type AntiSpamSettings = {
    banHours?: number;
    tarpitSeconds?: number;
    enableFCrDNSWorkarounds?: boolean;
    consecutiveCmdFail?: number;
    banConsecutiveCmdFail?: boolean;
    tarpitConsecutiveCmdFail?: boolean;
    consecutiveRcptFail?: number;
    banConsecutiveRcptFail?: boolean;
    tarpitConsecutiveRcptFail?: boolean;
    asnBlocklist?: Array<string>;
    enforceAsnBlocklist?: boolean;
    banAsnBlocklist?: boolean;
    tarpitAsnBlocklist?: boolean;
    clientBlocklist?: Array<string>;
    enforceClientBlocklist?: boolean;
    banClientBlocklist?: boolean;
    tarpitClientBlocklist?: boolean;
    enforceForwardDns?: boolean;
    banForwardDns?: boolean;
    tarpitForwardDns?: boolean;
    enforceReverseDns?: boolean;
    banReverseDns?: boolean;
    tarpitReverseDns?: boolean;
    dnsBlocklist?: Array<string>;
    enforceDnsBlocklist?: boolean;
    banDnsBlocklist?: boolean;
    tarpitDnsBlocklist?: boolean;
    enforceSpf?: boolean;
    banSpf?: boolean;
    tarpitSpf?: boolean;
};

