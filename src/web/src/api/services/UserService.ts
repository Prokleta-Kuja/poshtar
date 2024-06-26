/* generated using openapi-typescript-codegen -- do not edit */
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
     * @returns UserLMListResponse OK
     * @throws ApiError
     */
    public static getUsers({
        addressId,
        notAddressId,
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        addressId?: number,
        notAddressId?: number,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<UserLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/users',
            query: {
                'addressId': addressId,
                'notAddressId': notAddressId,
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }
    /**
     * @returns UserVM OK
     * @throws ApiError
     */
    public static createUser({
        requestBody,
    }: {
        requestBody?: UserCM,
    }): CancelablePromise<UserVM> {
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
     * @returns UserVM OK
     * @throws ApiError
     */
    public static getUser({
        userId,
    }: {
        userId: number,
    }): CancelablePromise<UserVM> {
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
     * @returns UserVM OK
     * @throws ApiError
     */
    public static updateUser({
        userId,
        requestBody,
    }: {
        userId: number,
        requestBody?: UserUM,
    }): CancelablePromise<UserVM> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/users/{userId}',
            path: {
                'userId': userId,
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
    public static deleteUser({
        userId,
    }: {
        userId: number,
    }): CancelablePromise<void> {
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
