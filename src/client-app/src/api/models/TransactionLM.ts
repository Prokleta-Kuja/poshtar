/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type TransactionLM = {
    id: number;
    connectionId: string;
    start: string;
    end: string;
    client?: string | null;
    username?: string | null;
    from?: string | null;
    complete: boolean;
};

