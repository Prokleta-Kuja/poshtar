/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ServiceRequestModel } from '../models/ServiceRequestModel';
import type { ServiceResultModel } from '../models/ServiceResultModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ServicesService {

    /**
     * @returns ServiceResultModel Success
     * @throws ApiError
     */
    public static request({
        requestBody,
    }: {
        requestBody?: ServiceRequestModel,
    }): CancelablePromise<ServiceResultModel> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/api/services',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
