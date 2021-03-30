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
 * Load and display the resources.
 */
const PanelResourcesLoad = async () => {
    const panel = qs('panel#PanelResources'),
        table = panel.querySelector('table'),
        tbody = table.querySelector('tbody'),
        resources = await GetResources();

    panel.classList.remove('loading');

    tbody.innerHTML = '';

    resources.forEach(resource => {
        const tr = ce('tr'),
            tdStatus = ce('td'),
            tdId = ce('td'),
            tdName = ce('td'),
            tdUrl = ce('td'),
            tdLastScan = ce('td'),
            tdNextScan = ce('td');

        // Status
        tdStatus.innerText = resource.status;
        tdStatus.classList.add('status');
        tdStatus.classList.add(resource.status.toLowerCase());

        // Id
        const aid = ce('a');

        aid.innerText = resource.id;
        aid.setAttribute('data-dom-id', 'PanelResource');
        aid.setAttribute('data-entity-id', resource.id);
        aid.addEventListener('click', TogglePanel);

        tdId.appendChild(aid);

        // Name
        tdName.innerText = resource.name;

        // Url
        tdUrl.innerText = resource.url;

        // Last Scan
        tdLastScan.innerText = resource.lastScan;

        // Next Scan
        tdNextScan.innerText = resource.nextScan;

        // Done
        tr.appendChild(tdStatus);
        tr.appendChild(tdId);
        tr.appendChild(tdName);
        tr.appendChild(tdUrl);
        tr.appendChild(tdLastScan);
        tr.appendChild(tdNextScan);
        tbody.appendChild(tr);
    });
};

/**
 * Load and display a single resource.
 */
const PanelResourceLoad = async () => {
    const panel = qs('panel#PanelResource'),
        eid = panel.getAttribute('data-entity-id'),
        resource = await GetResource(eid);

    panel.classList.remove('loading');

    const tbId = qs('input#TextBoxEditResourceId'),
        tbStatus = qs('input#TextBoxEditResourceStatus'),
        tbName = qs('input#TextBoxEditResourceName'),
        tbUrl = qs('input#TextBoxEditResourceUrl'),
        tbLastScan = qs('input#TextBoxEditResourceLastScan'),
        tbNextScan = qs('input#TextBoxEditResourceNextScan');

    // Id
    tbId.value = resource.id;

    // Status
    tbStatus.value = resource.status;
    tbStatus.classList.add(resource.status.toLowerCase());

    // Name
    tbName.value = resource.name;

    // Url
    tbUrl.value = resource.url;

    // Last scan
    tbLastScan.value = resource.lastScan;

    // Next scan
    tbNextScan.value = resource.nextScan;
};

/**
 * Toggle the visibility of an element.
 * @param {Event} e Click event.
 */
const ToggleElementHiddenState = async (e) => {
    if (e) {
        e.preventDefault();
    }

    const id = e.target.getAttribute('data-dom-id');

    qs(`#${id}`).classList.toggle('hidden');
};

/**
 * Toggle the visibility of a panel (hide others) and init the load function.
 * @param {Event} e Click event.
 */
const TogglePanel = async (e) => {
    if (e) {
        e.preventDefault();
    }

    const id = e.target.getAttribute('data-dom-id'),
        eid = e.target.getAttribute('data-entity-id');

    qsa('panel').forEach(panel => {
        const pid = panel.getAttribute('id');

        if (pid !== id) {
            panel.classList.add('hidden');
            return;
        }

        panel.classList.remove('hidden');
        panel.setAttribute('data-entity-id', eid);

        const fn = panel.getAttribute('data-fn-on-show');

        if (!fn || !window[fn]) {
            return;
        }

        panel.classList.add('loading');

        window[fn]();
    });
};

/**
 * Setup some of the functions as global functions.
 */
(async () => {
    // Make functions globally accessable.
    window['MenuSignOut'] = MenuSignOut;
    window['PanelResourcesLoad'] = PanelResourcesLoad;
    window['PanelResourceLoad'] = PanelResourceLoad;
    window['TogglePanel'] = TogglePanel;
    window['ToggleElementHiddenState'] = ToggleElementHiddenState;

    // Map click auto-functions.
    qsa('a').forEach(a => {
        const fn = a.getAttribute('data-on-click');

        if (!fn) {
            return;
        }

        if (!window[fn]) {
            console.error(`Implementation Error! Function not found: ${fn}`);
            return;
        }

        a.addEventListener('click', window[fn]);
    });
})();