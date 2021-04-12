"use strict";

/**
 * Create a new resource.
 * @param {String} name Name of the resource.
 * @param {String} url URL of the resource.
 * @returns {Object} Created resource.
 */
const CreateResource = async (name, url) => {
    const res = await fetch(
        '/api/resource',
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                name,
                url
            })
        });

    return res?.status === 200
        ? await res.json()
        : {};
};

/**
 * Delete a resource from the API.
 * @param {String} id Resource ID.
 */
const DeleteResource = async (id) => {
    await fetch(
        `/api/resource/${id}`,
        {
            method: 'DELETE'
        });
};

/**
 * Get alerts from API.
 * @param {String} id Resource id.
 * @returns {Array} List of alerts.
 */
const GetAlerts = async (id) => {
    const url = id
        ? `/api/alert?resourceId=${id}`
        : '/api/alert';

    const res = await fetch(url);

    return res?.status === 200
        ? await res.json()
        : [];
};

/**
 * Get issues from API.
 * @param {String} id Resource id.
 * @returns {Array} List of issues.
 */
const GetIssues = async (id) => {
    const url = id
        ? `/api/issue?resourceId=${id}`
        : '/api/issue';

    const res = await fetch(url);

    return res?.status === 200
        ? await res.json()
        : [];
};

/**
 * Get logs from API.
 * @param {String} id Resource id.
 * @returns {Array} List of logs.
 */
const GetLogs = async (id) => {
    const res = await fetch(`/api/log?resourceId=${id}`);

    return res?.status === 200
        ? await res.json()
        : [];
};

/**
 * Get resources from API.
 * @returns {Array} List of resources.
 */
const GetResources = async () => {
    const res = await fetch('/api/resource');

    return res?.status === 200
        ? await res.json()
        : [];
};

/**
 * Get resource from API.
 * @param {String} id Resource id.
 * @returns {Object} Resource.
 */
const GetResource = async (id) => {
    const res = await fetch(`/api/resource/${id}`);

    return res?.status === 200
        ? await res.json()
        : {};
};

/**
 * Get settings from API.
 * @returns {Object} Settings values.
 */
const GetSettings = async () => {
    const res = await fetch('/api/settings');

    return res?.status === 200
        ? await res.json()
        : {};
};

/**
 * Get stats from API.
 * @returns {Object} Stats values.
 */
const GetStats = async () => {
    const res = await fetch('/api/stats');

    return res?.status === 200
        ? await res.json()
        : {};
};

/**
 * Get the user object.
 * @returns {Object} User object.
 */
const GetUser = async () => {
    const res = await fetch('/api/auth');

    return res?.status === 200
        ? await res.json()
        : null;
};

/**
 * Update an existing resource.
 * @param {String} id Resource ID.
 * @param {String} name Name for resoruce.
 * @param {String} url URL for resource.
 */
const UpdateResource = async (id, name, url) => {
    await fetch(
        `/api/resource/${id}`,
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                name,
                url
            })
        });
};

/**
 * Verify the user with Google.
 * @param {String} credentials Google temporary credentials.
 * @returns {Boolean} Success.
 */
const VerifyUser = async (credentials) => {
    const res = await fetch(
        '/api/auth',
        {
            'method': 'POST',
            'body': JSON.stringify({
                'credentials': credentials
            }),
            'headers': {
                'Content-Type': 'application/json'
            }
        });

    return res?.status === 200;
};