/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Boja } from './Boja';

export type DomainByIdResult = {
    domainId: number;
    name: string;
    disabled?: string | null;
    bojaMoja: Boja;
};

