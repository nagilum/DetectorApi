"use strict";

/**
 * Toggle the visibility of the user menu.
 * @param {Event} e Click event.
 */
const HeaderToggleUserMenu = async (e) => {
    if (e) {
        e.preventDefault();
    }

    qs('menu#UserMenu').classList.toggle('hidden');
};

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
 * Setup some of the functions as global functions.
 */
(async () => {
    window['HeaderToggleUserMenu'] = HeaderToggleUserMenu;
    window['MenuSignOut'] = MenuSignOut;
})();