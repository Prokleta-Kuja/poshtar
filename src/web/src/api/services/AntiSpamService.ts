/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AntiSpamSettings } from '../models/AntiSpamSettings';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AntiSpamService {
    /**
     * @returns AntiSpamSettings OK
     * @throws ApiError
     */
    public static getAntiSpam(): CancelablePromise<AntiSpamSettings> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/antispam',
        });
    }
    /**
     * @returns AntiSpamSettings OK
     * @throws ApiError
     */
    public static updateAntiSpam({
        requestBody,
    }: {
        requestBody?: AntiSpamSettings,
    }): CancelablePromise<AntiSpamSettings> {
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
