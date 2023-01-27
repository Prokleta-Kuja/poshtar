/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Domains } from './Domains';

export type DomainsResponse = {
    items: Array<Domains>;
    size: number;
    page: number;
    total: number;
    ascending?: boolean;
    sortBy?: string | null;
};

