/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DomainByIdResponse } from '../models/DomainByIdResponse';
import type { DomainsResponse } from '../models/DomainsResponse';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DomainService {

    /**
     * @param id
     * @returns DomainByIdResponse Success
     * @throws ApiError
     */
    public static getDomainById(
        id: number,
    ): CancelablePromise<DomainByIdResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/domains/{id}',
            path: {
                'id': id,
            },
            errors: {
                400: `Bad Request`,
                403: `Forbidden`,
                500: `Server Error`,
            },
        });
    }

    /**
     * @param size
     * @param page
     * @returns DomainsResponse Success
     * @throws ApiError
     */
    public static getDomains(
        size?: number,
        page?: number,
    ): CancelablePromise<DomainsResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/domains',
            query: {
                'size': size,
                'page': page,
            },
            errors: {
                400: `Bad Request`,
                403: `Forbidden`,
                500: `Server Error`,
            },
        });
    }

}
