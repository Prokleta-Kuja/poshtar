/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { LogEntryLMListResponse } from '../models/LogEntryLMListResponse';
import type { RecipientLMListResponse } from '../models/RecipientLMListResponse';
import type { TransactionLMListResponse } from '../models/TransactionLMListResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TransactionService {
    /**
     * @returns TransactionLMListResponse OK
     * @throws ApiError
     */
    public static getTransactions({
        connectionId,
        countryName,
        asn,
        client,
        from,
        includeMonitor,
        includePrivate,
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        connectionId?: string,
        countryName?: string,
        asn?: string,
        client?: string,
        from?: string,
        includeMonitor?: boolean,
        includePrivate?: boolean,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<TransactionLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/transactions',
            query: {
                'connectionId': connectionId,
                'countryName': countryName,
                'asn': asn,
                'client': client,
                'from': from,
                'includeMonitor': includeMonitor,
                'includePrivate': includePrivate,
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }
    /**
     * @returns LogEntryLMListResponse OK
     * @throws ApiError
     */
    public static getLogs({
        transactionId,
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        transactionId: number,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<LogEntryLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/transactions/{transactionId}/logs',
            path: {
                'transactionId': transactionId,
            },
            query: {
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }
    /**
     * @returns RecipientLMListResponse OK
     * @throws ApiError
     */
    public static getRecipients({
        transactionId,
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        transactionId: number,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<RecipientLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/transactions/{transactionId}/recipients',
            path: {
                'transactionId': transactionId,
            },
            query: {
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }
    /**
     * @returns void
     * @throws ApiError
     */
    public static deleteTransaction({
        transactionId,
    }: {
        transactionId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/transactions/{transactionId}',
            path: {
                'transactionId': transactionId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }
}
