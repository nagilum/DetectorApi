"use strict";

/**
 * Sign the user out of the system.
 * @param {Event} e Click event.
 */
 const MenuSignOut = async (e) => {
    if (e) {
        e.preventDefault();
    }

    return fetch(
        '/api/auth',
        {
            method: 'DELETE'
        })
        .then(() => {
            window.location = '/';
        });
};

/**
 * Toggle the visibility of an element.
 * @param {Event} e Click event.
 */
const ToggleElementHiddenState = async (e) => {
    if (e) {
        e.preventDefault();
    }

    qs(`#${e.target.getAttribute('data-dom-id')}`).classList.toggle('hidden');
};

/**
 * Setup some of the functions as global functions.
 */
(async () => {
    // Make functions globally accessable.
    window['MenuSignOut'] = MenuSignOut;
    window['ToggleElementHiddenState'] = ToggleElementHiddenState;

    // Map click auto-functions.
    qsa('a').forEach(a => {
        const fn = a.getAttribute('data-on-click');

        if (!fn) {
            return;
        }

        a.addEventListener('click', window[fn]);
    });
})();