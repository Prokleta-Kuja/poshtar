/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { UserCM } from '../models/UserCM';
import type { UserLMListResponse } from '../models/UserLMListResponse';
import type { UserUM } from '../models/UserUM';
import type { UserVM } from '../models/UserVM';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UserService {

    /**
     * @param addressId
     * @param notAddressId
     * @param domainId
     * @param notDomainId
     * @param size
     * @param page
     * @param ascending
     * @param sortBy
     * @param searchTerm
     * @returns UserLMListResponse Success
     * @throws ApiError
     */
    public static getUsers(
        addressId?: number,
        notAddressId?: number,
        domainId?: number,
        notDomainId?: number,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    ): CancelablePromise<UserLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/users',
            query: {
                'addressId': addressId,
                'notAddressId': notAddressId,
                'domainId': domainId,
                'notDomainId': notDomainId,
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
     * @returns UserVM Success
     * @throws ApiError
     */
    public static createUser(
        requestBody?: UserCM,
    ): CancelablePromise<UserVM> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/users',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param userId
     * @returns UserVM Success
     * @throws ApiError
     */
    public static getUser(
        userId: number,
    ): CancelablePromise<UserVM> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/users/{userId}',
            path: {
                'userId': userId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @param userId
     * @param id
     * @param requestBody
     * @returns UserVM Success
     * @throws ApiError
     */
    public static updateUser(
        userId: string,
        id?: number,
        requestBody?: UserUM,
    ): CancelablePromise<UserVM> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/users/{userId}',
            path: {
                'userId': userId,
            },
            query: {
                'id': id,
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
     * @param userId
     * @returns void
     * @throws ApiError
     */
    public static deleteUser(
        userId: number,
    ): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/users/{userId}',
            path: {
                'userId': userId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

}
