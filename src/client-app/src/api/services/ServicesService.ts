/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AuthStatusModel } from '../models/AuthStatusModel';
import type { ServiceRequestModel } from '../models/ServiceRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ServicesService {

    /**
     * @returns AuthStatusModel Success
     * @throws ApiError
     */
    public static request({
        requestBody,
    }: {
        requestBody?: ServiceRequestModel,
    }): CancelablePromise<AuthStatusModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/services',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
