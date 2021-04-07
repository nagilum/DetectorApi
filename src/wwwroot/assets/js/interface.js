"use strict";

/**
 * If it's a valid link, push the state to browser history.
 * @param {Event} e Click event.
 */
const BrowserHistoryAdd = async (e) => {
    const typeOf = typeof(e);

    let href;

    switch (typeOf) {
        case 'string':
            href = e;
            break;

        case 'object':
            if (!e.target) {
                console.error(`Implementation Error! In function 'BrowserHistoryAdd', parameter 'e' is of type 'object' but does not have a 'target'!`);
                return;
            }

            href = e.target.getAttribute('href');

            break;

        default:
            console.error(`Implementation Error! In function 'BrowserHistoryAdd', parameter 'e' is of type '${typeOf}' which is not handled!`);
            return;
    }

    if (!href) {
        return;
    }

    const type = e?.target?.getAttribute('data-type'),
        id = e?.target?.getAttribute('data-entity-id');

    window.history.pushState(
        {
            href,
            type,
            id
        },
        document.title,
        href);
};

/**
 * Navigate based on history states.
 * @param {Event} e History pop event.
 */
const BrowserHistoryOnPopState = async (e) => {
    const state = e.state;

    // Load frontpage.
    if (!state) {
        await TogglePanel();
        return;
    }

    // Load by type.
    switch (state.type) {
        case 'resources':
            await TogglePanel(null, 'PanelResources');
            break;

        case 'resource':
            await TogglePanel(null, 'PanelResource', state.id);
            break;

        default:
            console.error(`Implementation Error! Browser state '${state.type}' is not handled!`);
            console.log('state', state);
            break;
    }
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
 * Open the correct panel based on the URL when the document was loaded.
 */
const OpenPanelBasedOnUrl = async () => {
    const hash = window.location.hash;

    if (!hash || hash === '#') {
        return;
    }

    const parts = hash.substr(1).split('/');

    // Load resources?
    if (parts.length === 1 && parts[0] === 'resources') {
        await TogglePanel(null, 'PanelResources');
    }

    // Load single resource?
    if (parts.length === 2 && parts[0] === 'resource') {
        await TogglePanel(null, 'PanelResource', parts[1]);
    }

    // Something else?
    else {
        console.warn(`Possible Implementation Error! No clause for pathname '${window.location.pathname}' on-load.`, parts);
    }
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
        aid.setAttribute('href', `/#resource/${resource.id}`);
        aid.setAttribute('data-dom-id', 'PanelResource');
        aid.setAttribute('data-entity-id', resource.id);
        aid.setAttribute('data-type', 'resource');
        aid.addEventListener('click', BrowserHistoryAdd);
        aid.addEventListener('click', TogglePanel);

        tdId.appendChild(aid);

        // Name
        const aname = ce('a');

        aname.innerText = resource.name;
        aname.setAttribute('href', `/#resource/${resource.id}`);
        aname.setAttribute('data-dom-id', 'PanelResource');
        aname.setAttribute('data-entity-id', resource.id);
        aname.setAttribute('data-type', 'resource');
        aname.addEventListener('click', BrowserHistoryAdd);
        aname.addEventListener('click', TogglePanel);

        tdName.appendChild(aname);

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
        resource = await GetResource(eid),
        results = await GetResults(eid);

    panel.classList.remove('loading');

    const tbId = qs('input#TextBoxEditResourceId'),
        tbStatus = qs('input#TextBoxEditResourceStatus'),
        tbName = qs('input#TextBoxEditResourceName'),
        tbUrl = qs('input#TextBoxEditResourceUrl'),
        tbLastScan = qs('input#TextBoxEditResourceLastScan'),
        tbNextScan = qs('input#TextBoxEditResourceNextScan'),
        table = panel.querySelector('table'),
        tbody = table.querySelector('tbody');

    // Id
    tbId.value = resource.id;

    // Status
    tbStatus.value = resource.status;
    tbStatus.classList.add(resource.status.toLowerCase());

    // Name
    tbName.value = resource.name;
    document.title = `Detector - ${resource.name}`;

    // Url
    tbUrl.value = resource.url;

    // Last scan
    tbLastScan.value = resource.lastScan;

    // Next scan
    tbNextScan.value = resource.nextScan;

    // Table
    tbody.innerHTML = '';

    results.forEach(result => {
        const tr = ce('tr'),
            tdCreated = ce('td'),
            tdUpdated = ce('td'),
            tdStatusCode = ce('td'),
            tdSslError = ce('td'),
            tdConnectingIp = ce('td'),
            tdGeneralError = ce('td');

        let cls;

        // Created
        tdCreated.innerText = result.created;

        // Updated
        tdUpdated.innerText = result.updated;

        // StatusCode
        if (result.statusCode >= 200 && result.statusCode < 300) {
            cls = 'ok';
        }
        else if (result.statusCode >= 300 && result.statusCode < 400) {
            cls = 'warning';
        }
        else if (result.statusCode >= 400) {
            cls = 'error';
        }

        tdStatusCode.innerText = result.statusCode;
        tdStatusCode.classList.add('status-text');
        tdStatusCode.classList.add(cls);

        // SSL Error
        tdSslError.innerText = result.sslErrorCode
            ? `${result.sslErrorCode}: ${result.sslErrorMessage}`
            : '';

        if (result.sslErrorCode) {
            tdSslError.classList.add('status-text');
            tdSslError.classList.add('error');
        }

        // Connecting IP
        tdConnectingIp.innerText = result.connectingIp;

        // General Error
        tdGeneralError.innerText = result.exceptionMessage;

        if (result.exceptionMessage) {
            tdGeneralError.classList.add('status-text');
            tdGeneralError.classList.add('error');
        }

        // Done
        tr.appendChild(tdCreated);
        tr.appendChild(tdUpdated);
        tr.appendChild(tdStatusCode);
        tr.appendChild(tdSslError);
        tr.appendChild(tdConnectingIp);
        tr.appendChild(tdGeneralError);
        tbody.appendChild(tr);
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

    const id = e.target.getAttribute('data-dom-id');

    qs(`#${id}`).classList.toggle('hidden');
};

/**
 * Toggle the visibility of a panel (hide others) and init the load function.
 * @param {Event} e Click event.
 * @param {String} passedId Passed 'id' variable.
 * @param {String} passedEid Passed 'eid' variable.
 */
const TogglePanel = async (e, passedId, passedEid) => {
    if (e) {
        e.preventDefault();
    }

    let id = e?.target?.getAttribute('data-dom-id'),
        eid = e?.target?.getAttribute('data-entity-id');

    if (passedId) {
        id = passedId;
    }

    if (passedEid) {
        eid = passedEid;
    }

    qsa('panel').forEach(panel => {
        const pid = panel.getAttribute('id'),
            title = panel.getAttribute('data-title');

        if (pid !== id) {
            panel.classList.add('hidden');
            return;
        }

        document.title = title
            ? `Detector - ${title}`
            : 'Detector';

        panel.classList.remove('hidden');
        panel.setAttribute('data-entity-id', eid);

        const fn = panel.getAttribute('data-fn-on-show');

        if (!fn || !window[fn]) {
            return;
        }

        if (!window[fn]) {
            console.error(`Implementation Error! Function not found: ${fn}`);
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
    window['BrowserHistoryAdd'] = BrowserHistoryAdd;
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

        a.addEventListener('click', BrowserHistoryAdd);
        a.addEventListener('click', window[fn]);
    });

    // Setup pop-state.
    window.onpopstate = BrowserHistoryOnPopState;

    // Open the correct panel based on the URL when the document was loaded.
    await OpenPanelBasedOnUrl();
})();