/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { RelayCM } from '../models/RelayCM';
import type { RelayLMListResponse } from '../models/RelayLMListResponse';
import type { RelayUM } from '../models/RelayUM';
import type { RelayVM } from '../models/RelayVM';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class RelayService {

    /**
     * @returns RelayLMListResponse Success
     * @throws ApiError
     */
    public static getRelays({
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<RelayLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/relays',
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
     * @returns RelayVM Success
     * @throws ApiError
     */
    public static createRelay({
        requestBody,
    }: {
        requestBody?: RelayCM,
    }): CancelablePromise<RelayVM> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/relays',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns RelayVM Success
     * @throws ApiError
     */
    public static getRelay({
        relayId,
    }: {
        relayId: number,
    }): CancelablePromise<RelayVM> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/relays/{relayId}',
            path: {
                'relayId': relayId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns RelayVM Success
     * @throws ApiError
     */
    public static updateRelay({
        relayId,
        requestBody,
    }: {
        relayId: number,
        requestBody?: RelayUM,
    }): CancelablePromise<RelayVM> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/relays/{relayId}',
            path: {
                'relayId': relayId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static deleteRelay({
        relayId,
    }: {
        relayId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/relays/{relayId}',
            path: {
                'relayId': relayId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

}
