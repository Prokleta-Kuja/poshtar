/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CalendarCM } from '../models/CalendarCM';
import type { CalendarLMListResponse } from '../models/CalendarLMListResponse';
import type { CalendarUM } from '../models/CalendarUM';
import type { CalendarUserSMListResponse } from '../models/CalendarUserSMListResponse';
import type { CalendarVM } from '../models/CalendarVM';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class CalendarService {
    /**
     * @returns CalendarLMListResponse OK
     * @throws ApiError
     */
    public static getCalendars({
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<CalendarLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/calendars',
            query: {
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }
    /**
     * @returns CalendarVM OK
     * @throws ApiError
     */
    public static createCalendar({
        requestBody,
    }: {
        requestBody?: CalendarCM,
    }): CancelablePromise<CalendarVM> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/calendars',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }
    /**
     * @returns CalendarVM OK
     * @throws ApiError
     */
    public static getCalendar({
        calendarId,
    }: {
        calendarId: number,
    }): CancelablePromise<CalendarVM> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/calendars/{calendarId}',
            path: {
                'calendarId': calendarId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns CalendarVM OK
     * @throws ApiError
     */
    public static updateCalendar({
        calendarId,
        requestBody,
    }: {
        calendarId: number,
        requestBody?: CalendarUM,
    }): CancelablePromise<CalendarVM> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/calendars/{calendarId}',
            path: {
                'calendarId': calendarId,
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
    public static deleteCalendar({
        calendarId,
    }: {
        calendarId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/calendars/{calendarId}',
            path: {
                'calendarId': calendarId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns CalendarUserSMListResponse OK
     * @throws ApiError
     */
    public static getCalendarAddableUsers({
        calendarId,
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        calendarId: number,
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<CalendarUserSMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/calendars/{calendarId}/addable-users',
            path: {
                'calendarId': calendarId,
            },
            query: {
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }
    /**
     * @returns void
     * @throws ApiError
     */
    public static addCalendarUser({
        calendarId,
        userId,
        canWrite,
    }: {
        calendarId: number,
        userId: number,
        canWrite?: boolean,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/calendars/{calendarId}/users/{userId}',
            path: {
                'calendarId': calendarId,
                'userId': userId,
            },
            query: {
                'canWrite': canWrite,
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
    public static removeCalendarUser({
        calendarId,
        userId,
    }: {
        calendarId: number,
        userId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/calendars/{calendarId}/users/{userId}',
            path: {
                'calendarId': calendarId,
                'userId': userId,
            },
            errors: {
                404: `Not Found`,
                409: `Conflict`,
            },
        });
    }
}
