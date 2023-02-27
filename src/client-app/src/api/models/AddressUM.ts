/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AddressType } from './AddressType';

export type AddressUM = {
    domainId: number;
    pattern: string;
    description?: string | null;
    type: AddressType;
    disabled?: boolean | null;
};

