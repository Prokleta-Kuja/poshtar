/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DomainByIdResponse } from '../models/DomainByIdResponse';
import type { DomainCreateResponse } from '../models/DomainCreateResponse';
import type { DomainsResponse } from '../models/DomainsResponse';
import type { DomainUpdateResponse } from '../models/DomainUpdateResponse';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DomainService {

    /**
     * @param searchTerm
     * @param size
     * @param page
     * @param ascending
     * @param sortBy
     * @returns DomainsResponse Success
     * @throws ApiError
     */
    public static getDomains(
        searchTerm?: string,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
    ): CancelablePromise<DomainsResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/domains',
            query: {
                'searchTerm': searchTerm,
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
            },
            errors: {
                400: `Bad Request`,
                403: `Forbidden`,
                500: `Server Error`,
            },
        });
    }

    /**
     * @param name
     * @param host
     * @param port
     * @param isSecure
     * @param username
     * @param password
     * @returns DomainCreateResponse Success
     * @throws ApiError
     */
    public static createDomain(
        name: string,
        host: string,
        port: number,
        isSecure: boolean,
        username: string,
        password: string,
    ): CancelablePromise<DomainCreateResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/domains',
            query: {
                'name': name,
                'host': host,
                'port': port,
                'isSecure': isSecure,
                'username': username,
                'password': password,
            },
            errors: {
                400: `Bad Request`,
                403: `Forbidden`,
                500: `Server Error`,
            },
        });
    }

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
     * @param domainId
     * @param name
     * @param host
     * @param port
     * @param isSecure
     * @param username
     * @param newPassword
     * @returns DomainUpdateResponse Success
     * @throws ApiError
     */
    public static updateDomain(
        domainId: number,
        name: string,
        host: string,
        port: number,
        isSecure: boolean,
        username: string,
        newPassword?: string,
    ): CancelablePromise<DomainUpdateResponse> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/domains/{id}',
            query: {
                'domainId': domainId,
                'name': name,
                'host': host,
                'port': port,
                'isSecure': isSecure,
                'username': username,
                'newPassword': newPassword,
            },
            errors: {
                400: `Bad Request`,
                403: `Forbidden`,
                500: `Server Error`,
            },
        });
    }

}
