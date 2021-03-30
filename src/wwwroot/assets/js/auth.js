"use strict";

/**
 * Add the given user to the interface.
 * @param {Object} user User object.
 */
const AddUserToInterface = async (user) => {
    console.log('user', user);
};

/**
 * Go through the Google API to verify the credentials from the one-tag login.
 * @param {Object} res Google callback credentials.
 */
const VerifyGoogleCredentials = async (res) => {
    if (!res || !res.credential) {
        console.log('res', res);

        // TODO: Show error notification!

        return;
    }

    if (await VerifyUser(res.credential) === false) {
        // TODO: Show error notification!

        return;
    }

    const user = await GetUser();

    // Is the user valid, if so, add to interface.
    if (user && user.email) {
        AddUserToInterface(user);
    }
};

/**
 * Check if user is logged in and continue to app, otherwise, show login info.
 */
(async () => {
    const user = await GetUser();

    // Is the user valid, if so, add to interface.
    if (user && user.email) {
        AddUserToInterface(user);
    }

    // Add the Google login form.
    else {
        const settings = await GetSettings(),
            div = ce('div'),
            script = ce('script');

        window['VerifyGoogleCredentials'] = VerifyGoogleCredentials;

        div.setAttribute('id', 'g_id_onload');
        div.setAttribute('data-client_id', settings['auth']['google']['clientId']);
        div.setAttribute('data-callback', 'VerifyGoogleCredentials');

        script.setAttribute('src', 'https://accounts.google.com/gsi/client');

        const body = qs('body');

        body.appendChild(div);
        body.appendChild(script);
    }
})();