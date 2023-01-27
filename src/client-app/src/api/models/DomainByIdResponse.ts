/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type DomainByIdResponse = {
    domainId: number;
    name: string;
    host?: string;
    port?: number;
    isSecure?: boolean;
    username?: string;
    disabled?: string | null;
    addressCount?: number;
    userCount?: number;
};

