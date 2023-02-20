/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { LogEntryVMListResponse } from '../models/LogEntryVMListResponse';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class LogEntryService {

    /**
     * @returns LogEntryVMListResponse Success
     * @throws ApiError
     */
    public static getLogEntries({
        context,
        from,
        till,
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        context?: string,
        from?: string,
        till?: string,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<LogEntryVMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/log-entires',
            query: {
                'context': context,
                'from': from,
                'till': till,
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }

}
