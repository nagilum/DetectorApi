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
            let el = e.target;

            if (!el) {
                console.error(`Implementation Error! In function 'BrowserHistoryAdd', parameter 'e' is of type 'object' but does not have a 'target'!`);
                return;
            }

            while (true) {
                if (!el || !el.tagName) {
                    return;
                }

                if (el.tagName.toLowerCase() === 'a') {
                    break;
                }

                el = el.parentElement;
            }

            href = el.getAttribute('href');

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
 * Get stats and populate stuff.
 */
const GetStatsAndPopulate = async () => {
    const stats = await GetStats();

    // Update version.
    const version = qs('header > h1 > span');

    if (version) {
        version.innerText = `Version ${stats.app.version}`;
    }

    // Update resource count.
    const src = qs('a#HeaderLinkResources > span.count');

    if (src) {
        src.innerText = stats.stats.resourceCount;
    }

    // Update open issues count.
    const soic = qs('a#HeaderLinkOpenIssues > span.count');

    if (soic) {
        soic.innerText = stats.stats.openIssueCount;

        if (stats.stats.openIssueCount > 0) {
            soic.classList.add('alert');
        }
        else {
            soic.classList.remove('alert');
        }
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

    // New resource?
    else if (parts.length === 2 && parts[0] === 'resource' && parts[1] === 'new') {
        await TogglePanel(null, 'PanelResourceNew');
    }

    // Bulk new resources?
    else if (parts.length === 2 && parts[0] === 'resource' && parts[1] === 'new-bulk') {
        await TogglePanel(null, 'PanelResourceNewBulk');
    }

    // Load single resource?
    else if (parts.length === 2 && parts[0] === 'resource') {
        await TogglePanel(null, 'PanelResource', parts[1]);
    }

    // Load issues?
    else if (parts.length === 1 && parts[0] === 'issues') {
        await TogglePanel(null, 'PanelIssues');
    }

    // Something else?
    else {
        console.warn(`Possible Implementation Error! No clause for pathname '${window.location.pathname}' on-load.`, parts);
    }
};

/**
 * Load and display the issues.
 */
const PanelIssuesLoad = async () => {
    const panel = qs('panel#PanelIssues'),
        issues = await GetIssues(),
        resources = await GetResources(),
        alerts = await GetAlerts(),
        openIssues = issues.filter(n => !n.resolved);
    
    const table = qs('table#openIssuesList'),
        tbody = table.qs('tbody');

    panel.classList.remove('loading');

    tbody.innerHTML = '';
    
    openIssues.forEach(issue => {
        const resource = resources.filter(n => n.id === issue.resourceId)[0];

        if (!resource) {
            return;
        }

        const tr = ce('tr'),
            tdResource = ce('td'),
            tdCreated = ce('td'),
            tdUpdated = ce('td'),
            tdAlerts = ce('td'),
            tdMessage = ce('td');

        // Resource.
        const ra = ce('a');

        ra.innerText = `[${resource.identifier}] ${resource.name}`;
        ra.setAttribute('href', `/#resource/${resource.identifier}`);
        ra.setAttribute('data-dom-id', 'PanelResource');
        ra.setAttribute('data-entity-id', resource.identifier);
        ra.setAttribute('data-type', 'resource');
        ra.addEventListener('click', BrowserHistoryAdd);
        ra.addEventListener('click', TogglePanel);

        tdResource.appendChild(ra);
        
        // Created.
        tdCreated.innerText = issue.created;

        // Updated.
        tdUpdated.innerText = issue.updated;

        // Alerts.
        tdAlerts.innerText = alerts.filter(n => n.issueId === issue.id).length;

        // Message.
        tdMessage.innerText = issue.message;

        // Add to table.
        tr.appendChild(tdResource);
        tr.appendChild(tdCreated);
        tr.appendChild(tdUpdated);
        tr.appendChild(tdAlerts);
        tr.appendChild(tdMessage);
        tbody.appendChild(tr);
    });
};

/**
 * Load and display the resources.
 */
const PanelResourcesLoad = async () => {
    const panel = qs('panel#PanelResources'),
        table = panel.qs('table'),
        tbody = table.qs('tbody'),
        resources = await GetResources();

    panel.classList.remove('loading');

    tbody.innerHTML = '';

    resources.forEach(resource => {
        const tr = ce('tr'),
            tdCheck = ce('td'),
            tdStatus = ce('td'),
            tdId = ce('td'),
            tdName = ce('td'),
            tdUrl = ce('td'),
            tdLastScan = ce('td'),
            tdNextScan = ce('td');

        // Checkbox for bulk actions.
        const cb = ce('input');

        cb.setAttribute('type', 'checkbox');
        cb.setAttribute('data-entity-id', resource.identifier);
        cb.classList.add('resources-bulk-action');

        tdCheck.appendChild(cb);

        // Status
        if (resource.active === null || resource.active === true) {
            if (!resource.status) {
                tdStatus.innerText = 'Not Scanned';
                tdStatus.classList.add('status');
            }
            else {
                tdStatus.innerText = resource.status;
                tdStatus.classList.add('status');
                tdStatus.classList.add(resource.status.toLowerCase());
            }
        }
        else {
            tdStatus.innerText = 'Paused';
            tdStatus.classList.add('status');
        }

        // Id
        const aid = ce('a');

        aid.innerText = resource.identifier;
        aid.setAttribute('href', `/#resource/${resource.identifier}`);
        aid.setAttribute('data-dom-id', 'PanelResource');
        aid.setAttribute('data-entity-id', resource.identifier);
        aid.setAttribute('data-type', 'resource');
        aid.addEventListener('click', BrowserHistoryAdd);
        aid.addEventListener('click', TogglePanel);

        tdId.appendChild(aid);

        // Name
        const aname = ce('a');

        aname.innerText = resource.name;
        aname.setAttribute('href', `/#resource/${resource.identifier}`);
        aname.setAttribute('data-dom-id', 'PanelResource');
        aname.setAttribute('data-entity-id', resource.identifier);
        aname.setAttribute('data-type', 'resource');
        aname.addEventListener('click', BrowserHistoryAdd);
        aname.addEventListener('click', TogglePanel);

        tdName.appendChild(aname);

        // Url
        const aurl = ce('a');

        aurl.innerText = resource.url;
        aurl.setAttribute('href', resource.url);
        aurl.setAttribute('target', '_blank');

        tdUrl.appendChild(aurl);

        // Last Scan
        tdLastScan.innerText = resource.lastScan;

        // Next Scan
        tdNextScan.innerText = resource.nextScan;

        // Done
        tr.appendChild(tdCheck);
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
 * Create a new resource.
 */
const PanelResourceCreateNew = async () => {
    const panel = qs('panel#PanelResourceNew'),
        name = panel.qs('input#TextBoxNewResourceName').value,
        url = panel.qs('input#TextBoxNewResourceUrl').value;
    
    if (!name) {
        alert('Name is required!');
        return;
    }

    if (!url) {
        alert('URL is required!');
        return;
    }

    const resource = await CreateResource(name, url);

    if (!resource) {
        alert('Unhandled error. Check browser console for details.');
        return;
    }

    await TogglePanel(null, 'PanelResource', resource.identifier);
    await GetStatsAndPopulate();
};

/**
 * Delete a resource.
 */
const PanelResourceDelete = async () => {
    const res = confirm('Are you sure?');

    if (!res) {
        return;
    }

    const panel = qs('panel#PanelResource'),
        id = panel.getAttribute('data-entity-id');

    await DeleteResource(id);
    await TogglePanel(null, 'PanelResources');
};

/**
 * Update an existing resource.
 */
const PanelResourceSave = async () => {
    const panel = qs('panel#PanelResource'),
        id = panel.getAttribute('data-entity-id'),
        name = panel.qs('input#TextBoxEditResourceName').value,
        url = panel.qs('input#TextBoxEditResourceUrl').value;
    
    if (!name) {
        alert('Name is required!');
        return;
    }

    if (!url) {
        alert('URL is required!');
        return;
    }

    await UpdateResource(id, name, url);
};

/**
 * Pause/unpause a resource.
 */
const PanelResourceToggleActive = async () => {
    const panel = qs('panel#PanelResource'),
        id = panel.getAttribute('data-entity-id'),
        buttonToggleActive = qs('a#PanelResourceToggleActiveButton'),
        tbStatus = panel.qs('input#TextBoxEditResourceStatus');
    
    let resource = await GetResource(id);

    const postValue = resource.active === null ||
                      resource.active === true
        ? false
        : true;
    
    await UpdateResource(id, null, null, postValue);

    resource = await GetResource(id);

    if (!resource) {
        return;
    }

    if (resource.active === null || resource.active === true) {
        buttonToggleActive.innerText = 'Pause';
        buttonToggleActive.classList.add('danger');
        buttonToggleActive.classList.remove('success');

        if (!resource.status) {
            tbStatus.value = 'Not Scanned';
            tbStatus.classList = 'readonly';
        }
        else {
            tbStatus.value = resource.status;
            tbStatus.classList = 'readonly';
            tbStatus.classList.add(resource.status.toLowerCase());
        }
    }
    else {
        buttonToggleActive.innerText = 'Activate';
        buttonToggleActive.classList.remove('danger');
        buttonToggleActive.classList.add('success');

        tbStatus.value = 'Paused';
        tbStatus.classList = 'readonly';
    }
};

/**
 * Load and display a single resource.
 */
const PanelResourceLoad = async () => {
    const panel = qs('panel#PanelResource'),
        eid = panel.getAttribute('data-entity-id'),
        resource = await GetResource(eid),
        issues = await GetIssues(eid),
        alerts = await GetAlerts(eid),
        logs = await GetLogs(eid),
        graphs = await GetGraph(eid);

    panel.classList.remove('loading');

    const buttonToggleActive = qs('a#PanelResourceToggleActiveButton'),
        tbId = qs('input#TextBoxEditResourceId'),
        tbStatus = qs('input#TextBoxEditResourceStatus'),
        tbName = qs('input#TextBoxEditResourceName'),
        tbUrl = qs('input#TextBoxEditResourceUrl'),
        tbCreated = qs('input#TextBoxEditResourceCreated'),
        tbLastScan = qs('input#TextBoxEditResourceLastScan'),
        tbNextScan = qs('input#TextBoxEditResourceNextScan'),
        tableOpenIssues = qs('table#openIssues'),
        tableResolvedIssues = qs('table#resolvedIssues'),
        tableLogs = qs('table#logs'),
        tbodyOpenIssues = tableOpenIssues.qs('tbody'),
        tbodyResolvedIssues = tableResolvedIssues.qs('tbody'),
        tbodyLogs = tableLogs.qs('tbody'),
        noOpenIssues = qs('div#noOpenIssues'),
        noResolvedIssues = qs('div#noResolvedIssues'),
        noLogs = qs('div#noLogs');

    // Id
    tbId.value = resource.identifier;

    // Status
    if (resource.active === null || resource.active === true) {
        buttonToggleActive.innerText = 'Pause';
        buttonToggleActive.classList.add('danger');

        if (!resource.status) {
            tbStatus.value = 'Not Scanned';
            tbStatus.classList = 'readonly';
        }
        else {
            tbStatus.value = resource.status;
            tbStatus.classList = 'readonly';
            tbStatus.classList.add(resource.status.toLowerCase());
        }
    }
    else {
        buttonToggleActive.innerText = 'Activate';
        buttonToggleActive.classList.remove('danger');

        tbStatus.value = 'Paused';
        tbStatus.classList = 'readonly';
    }

    // Name
    tbName.value = resource.name;
    document.title = `Detector - ${resource.name}`;

    // Url
    tbUrl.value = resource.url;

    // Created
    tbCreated.value = resource.created;

    // Last scan
    tbLastScan.value = resource.lastScan;

    // Next scan
    tbNextScan.value = resource.nextScan;

    // Graph
    try {
        const wrapper = qs('div#GraphWrapper'),
            canvas = ce('canvas');

        wrapper.innerHTML = '';

        canvas.setAttribute('height', '100');
        canvas.setAttribute('width', '1000');

        wrapper.appendChild(canvas);

        const ctx = canvas.getContext('2d'),
            labels = [],
            backgroundColor = [],
            borderColor = [],
            data = [];

        graphs.forEach(gp => {
            if (gp.st === 'Ok') {
                backgroundColor.push('#009900');
                borderColor.push('#00ff00');
                labels.push('');
            }
            else {
                backgroundColor.push('#990000');
                borderColor.push('#ff0000');
                labels.push(gp.dt);
            }

            data.push(gp.rt ?? 0);
        });

        new Chart(
            ctx,
            {
                type: 'line',
                data: {
                    labels,
                    datasets: [
                        {
                            label: 'Response Times (ms)',
                            data,
                            backgroundColor,
                            borderColor,
                            borderWidth: 1
                        }
                    ]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
    }
    catch {
        //
    }

    // Clear tables.
    tbodyOpenIssues.innerHTML = '';
    tbodyResolvedIssues.innerHTML = '';
    tbodyLogs.innerHTML = '';

    // Add open issues.
    const openIssues = issues.filter(n => !n.resolved);

    openIssues.filter(n => !n.resolved).forEach(issue => {
        const tr = ce('tr'),
            tdCreated = ce('td'),
            tdUpdated = ce('td'),
            tdAlerts = ce('td'),
            tdMessage = ce('td');
        
        // Created.
        tdCreated.innerText = issue.created;

        // Updated.
        tdUpdated.innerText = issue.updated;

        // Alerts.
        tdAlerts.innerText = alerts.filter(n => n.issueId === issue.id).length;

        // Message.
        tdMessage.innerText = issue.message;

        // Add to table.
        tr.appendChild(tdCreated);
        tr.appendChild(tdUpdated);
        tr.appendChild(tdAlerts);
        tr.appendChild(tdMessage);
        tbodyOpenIssues.appendChild(tr);
    });

    // No issues?
    if (openIssues.length > 0) {
        tableOpenIssues.classList.remove('hidden');
        noOpenIssues.classList.add('hidden');
    }
    else {
        tableOpenIssues.classList.add('hidden');
        noOpenIssues.classList.remove('hidden');
    }

    // Add resolved issues.
    const resolvedIssues = issues.filter(n => n.resolved);

    resolvedIssues.filter(n => n.resolved).forEach(issue => {
        const tr = ce('tr'),
            tdCreated = ce('td'),
            tdResolved = ce('td'),
            tdAlerts = ce('td'),
            tdMessage = ce('td');
        
        // Created.
        tdCreated.innerText = issue.created;

        // Updated.
        tdResolved.innerText = issue.updated;

        // Alerts.
        tdAlerts.innerText = alerts.filter(n => n.issueId === issue.id).length;

        // Message.
        tdMessage.innerText = issue.message;

        // Add to table.
        tr.appendChild(tdCreated);
        tr.appendChild(tdResolved);
        tr.appendChild(tdAlerts);
        tr.appendChild(tdMessage);
        tbodyResolvedIssues.appendChild(tr);
    });

    // No issues?
    if (resolvedIssues.length > 0) {
        tableResolvedIssues.classList.remove('hidden');
        noResolvedIssues.classList.add('hidden');
    }
    else {
        tableResolvedIssues.classList.add('hidden');
        noResolvedIssues.classList.remove('hidden');
    }

    // Add logs.
    logs.forEach(log => {
        const tr = ce('tr'),
            tdCreated = ce('td'),
            tdUser = ce('td'),
            tdType = ce('td'),
            tdMessage = ce('td');

        // Created.
        tdCreated.innerText = log.created;

        // User.

        // Type.
        tdType.innerText = log.type;
        tdType.classList.add('status-text');
        tdType.classList.add(log.type);

        // Message.
        tdMessage.innerText = log.message;

        // Add to table.
        tr.appendChild(tdCreated);
        tr.appendChild(tdUser);
        tr.appendChild(tdType);
        tr.appendChild(tdMessage);
        tbodyLogs.appendChild(tr);
    });

    // No logs?
    if (logs.length > 0) {
        tableLogs.classList.remove('hidden');
        noLogs.classList.add('hidden');
    }
    else {
        tableLogs.classList.add('hidden');
        noLogs.classList.remove('hidden');
    }
};

/**
 * Add multiple resources as bulk.
 */
const PanelResourcesBulkAdd = async () => {
    const panel = qs('panel#PanelResourceNewBulk'),
        tbUrls = panel.qs('textarea'),
        value = tbUrls.value,
        hasR = value.indexOf("\r") > -1,
        hasN = value.indexOf("\n") > -1;

    let sep = '',
        urls = [];

    if (hasR) {
        sep = "\r";
    }
    else if (hasN) {
        sep = "\n";
    }

    if (sep === '') {
        urls.push(value);
    }
    else {
        urls = value.split(sep);
    }

    await CreateResourcesBulk(urls);

    await TogglePanel(null, 'PanelResources');
    await GetStatsAndPopulate();
};

/**
 * Delete multiple resources in bulk.
 */
const PanelResourcesBulkDelete = async () => {
    const res = confirm('Are you sure?');

    if (!res) {
        return;
    }

    const panel = qs('panel#PanelResources'),
        cbl = panel.qsa('input.resources-bulk-action'),
        ids = [];

    cbl.forEach(cb => {
        if (cb.checked) {
            ids.push(cb.getAttribute('data-entity-id'));
        }
    });

    if (ids.length === 0) {
        return;
    }

    await DeleteResourcesBulk(ids);
    await TogglePanel(null, 'PanelResources');
    await GetStatsAndPopulate();
};

/**
 * Pause/upause multiple resources in bulk.
 */
const PanelResourcesBulkToggleActive = async () => {
    const res = confirm('Are you sure?');

    if (!res) {
        return;
    }

    const panel = qs('panel#PanelResources'),
        cbl = panel.qsa('input.resources-bulk-action'),
        ids = [];

    cbl.forEach(cb => {
        if (cb.checked) {
            ids.push(cb.getAttribute('data-entity-id'));
        }
    });

    if (ids.length === 0) {
        return;
    }

    await ToggleResourcesActiveBulk(ids);
    await TogglePanel(null, 'PanelResources');
    await GetStatsAndPopulate();
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
 * Toggle checked on all the checkboxes.
 */
const ToggleResourcesCheckboxes = async () => {
    const panel = qs('panel#PanelResources'),
        mcb = panel.qs('input#ToggleResourcesCheckboxesTrigger'),
        cbl = panel.qsa('input.resources-bulk-action');
    
    cbl.forEach(cb => {
        cb.checked = mcb.checked;
    });
};

/**
 * Toggle the visibility of a panel (hide others) and init the load function.
 * @param {Event} e Click event.
 * @param {String} passedId Passed 'id' variable.
 * @param {String} passedEid Passed 'eid' variable.
 */
const TogglePanel = async (e, passedId, passedEid) => {
    let el;

    if (e) {
        e.preventDefault();

        el = e.target;

        while(true) {
            if (el.tagName.toLowerCase() === 'a') {
                break;
            }
    
            el = el.parentElement;
        }
    }

    let id = el?.getAttribute('data-dom-id'),
        eid = el?.getAttribute('data-entity-id');

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

        if (!fn) {
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
    window['PanelIssuesLoad'] = PanelIssuesLoad;
    window['PanelResourcesLoad'] = PanelResourcesLoad;
    window['PanelResourceLoad'] = PanelResourceLoad;
    window['PanelResourceCreateNew'] = PanelResourceCreateNew;
    window['PanelResourceSave'] = PanelResourceSave;
    window['PanelResourceToggleActive'] = PanelResourceToggleActive;
    window['PanelResourceDelete'] = PanelResourceDelete;
    window['TogglePanel'] = TogglePanel;
    window['ToggleElementHiddenState'] = ToggleElementHiddenState;
    window['PanelResourcesBulkAdd'] = PanelResourcesBulkAdd;
    window['PanelResourcesBulkDelete'] = PanelResourcesBulkDelete;
    window['PanelResourcesBulkToggleActive'] = PanelResourcesBulkToggleActive;
    window['ToggleResourcesCheckboxes'] = ToggleResourcesCheckboxes;

    // Map click auto-functions.
    [
        'a',
        'input'
    ].forEach(tag => {
        qsa(tag).forEach(el => {
            const fn = el.getAttribute('data-on-click');

            if (!fn) {
                return;
            }
    
            if (!window[fn]) {
                console.error(`Implementation Error! Function not found: ${fn}`);
                return;
            }
    
            el.addEventListener('click', BrowserHistoryAdd);
            el.addEventListener('click', window[fn]);
        });
    });

    // Setup pop-state.
    window.onpopstate = BrowserHistoryOnPopState;

    // Get stats and populate stuff.
    await GetStatsAndPopulate();

    // Open the correct panel based on the URL when the document was loaded.
    await OpenPanelBasedOnUrl();
})();