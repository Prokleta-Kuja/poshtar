/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DomainCM } from '../models/DomainCM';
import type { DomainLMListResponse } from '../models/DomainLMListResponse';
import type { DomainUM } from '../models/DomainUM';
import type { DomainVM } from '../models/DomainVM';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DomainService {

    /**
     * @param addressId
     * @param notAddressId
     * @param userId
     * @param notUserId
     * @param size
     * @param page
     * @param ascending
     * @param sortBy
     * @param searchTerm
     * @returns DomainLMListResponse Success
     * @throws ApiError
     */
    public static getDomains(
        addressId?: number,
        notAddressId?: number,
        userId?: number,
        notUserId?: number,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    ): CancelablePromise<DomainLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/domains',
            query: {
                'addressId': addressId,
                'notAddressId': notAddressId,
                'userId': userId,
                'notUserId': notUserId,
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }

    /**
     * @param requestBody
     * @returns DomainVM Success
     * @throws ApiError
     */
    public static createDomain(
        requestBody?: DomainCM,
    ): CancelablePromise<DomainVM> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/domains',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param domainId
     * @returns DomainVM Success
     * @throws ApiError
     */
    public static getDomain(
        domainId: number,
    ): CancelablePromise<DomainVM> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/domains/{domainId}',
            path: {
                'domainId': domainId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @param domainId
     * @param requestBody
     * @returns DomainVM Success
     * @throws ApiError
     */
    public static updateDomain(
        domainId: number,
        requestBody?: DomainUM,
    ): CancelablePromise<DomainVM> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/domains/{domainId}',
            path: {
                'domainId': domainId,
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
     * @param domainId
     * @returns void
     * @throws ApiError
     */
    public static deleteDomain(
        domainId: number,
    ): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/domains/{domainId}',
            path: {
                'domainId': domainId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @param domainId
     * @param userId
     * @returns void
     * @throws ApiError
     */
    public static addDomainUser(
        domainId: number,
        userId: number,
    ): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/domains/{domainId}/users/{userId}',
            path: {
                'domainId': domainId,
                'userId': userId,
            },
            errors: {
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }

    /**
     * @param domainId
     * @param userId
     * @returns void
     * @throws ApiError
     */
    public static removeDomainUser(
        domainId: number,
        userId: number,
    ): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/domains/{domainId}/users/{userId}',
            path: {
                'domainId': domainId,
                'userId': userId,
            },
            errors: {
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }

}
