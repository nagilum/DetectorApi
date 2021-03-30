"use strict";

/**
 * Map auto-functions.
 */
(async () => {
    // Setup click events.
    qsa('a').forEach(a => {
        const fn = a.getAttribute('data-on-click');

        if (!fn) {
            return;
        }

        a.addEventListener('click', window[fn]);
    });
})();