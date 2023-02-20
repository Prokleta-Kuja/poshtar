/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AddressCM } from '../models/AddressCM';
import type { AddressLMListResponse } from '../models/AddressLMListResponse';
import type { AddressUM } from '../models/AddressUM';
import type { AddressVM } from '../models/AddressVM';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class AddressService {

    /**
     * @returns AddressLMListResponse Success
     * @throws ApiError
     */
    public static getAddresses({
        domainId,
        notDomainId,
        userId,
        notUserId,
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        domainId?: number,
        notDomainId?: number,
        userId?: number,
        notUserId?: number,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<AddressLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/addresses',
            query: {
                'domainId': domainId,
                'notDomainId': notDomainId,
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
     * @returns AddressVM Success
     * @throws ApiError
     */
    public static createAddress({
        requestBody,
    }: {
        requestBody?: AddressCM,
    }): CancelablePromise<AddressVM> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/addresses',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns AddressVM Success
     * @throws ApiError
     */
    public static getAddress({
        addressId,
    }: {
        addressId: number,
    }): CancelablePromise<AddressVM> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/addresses/{addressId}',
            path: {
                'addressId': addressId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns AddressVM Success
     * @throws ApiError
     */
    public static updateAddress({
        addressId,
        requestBody,
    }: {
        addressId: number,
        requestBody?: AddressUM,
    }): CancelablePromise<AddressVM> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/addresses/{addressId}',
            path: {
                'addressId': addressId,
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
    public static deleteAddress({
        addressId,
    }: {
        addressId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/addresses/{addressId}',
            path: {
                'addressId': addressId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static addAddressUser({
        addressId,
        userId,
    }: {
        addressId: number,
        userId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/addresses/{addressId}/users/{userId}',
            path: {
                'addressId': addressId,
                'userId': userId,
            },
            errors: {
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static removeAddressUser({
        addressId,
        userId,
    }: {
        addressId: number,
        userId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/addresses/{addressId}/users/{userId}',
            path: {
                'addressId': addressId,
                'userId': userId,
            },
            errors: {
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }

}
