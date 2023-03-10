/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type UserLM = {
    id: number;
    name: string;
    description?: string | null;
    isMaster: boolean;
    quotaMegaBytes?: number | null;
    disabled?: string | null;
    addressCount: number;
};

