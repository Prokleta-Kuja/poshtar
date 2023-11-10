/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AntiSpamSettings } from '../models/AntiSpamSettings';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class AntiSpamService {

    /**
     * @returns AntiSpamSettings Success
     * @throws ApiError
     */
    public static getAntiSpam(): CancelablePromise<AntiSpamSettings> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/antispam',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static updateAntiSpam({
        requestBody,
    }: {
        requestBody?: AntiSpamSettings,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/antispam',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
