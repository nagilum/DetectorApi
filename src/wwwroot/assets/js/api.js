"use strict";

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