/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DomainByIdResult } from '../models/DomainByIdResult';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DomainService {

    /**
     * @param id
     * @returns DomainByIdResult Success
     * @throws ApiError
     */
    public static domainById(
        id: number,
    ): CancelablePromise<DomainByIdResult> {
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

}
