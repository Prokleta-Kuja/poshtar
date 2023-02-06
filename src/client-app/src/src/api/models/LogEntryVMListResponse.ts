/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { LogEntryVM } from './LogEntryVM';

export type LogEntryVMListResponse = {
    items: Array<LogEntryVM>;
    size: number;
    page: number;
    total: number;
    ascending: boolean;
    sortBy?: string | null;
};

