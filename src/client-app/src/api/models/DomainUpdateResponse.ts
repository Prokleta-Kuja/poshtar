/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type DomainUpdateResponse = {
    domainId: number;
    name: string;
    host: string;
    port: number;
    isSecure: boolean;
    username: string;
    disabled?: string | null;
};

