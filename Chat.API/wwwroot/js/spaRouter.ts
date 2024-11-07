window.addEventListener('popstate', () => handleLocation(window.location.href));

document.addEventListener('click', e => {
    if (e.defaultPrevented)
        return;

    const target = e.target as HTMLElement;
    const anchor = target.closest('a');

    if (!anchor || anchor.origin != window.location.origin)
        return;

    e.preventDefault();

    if (anchor.href == window.location.href)
        return;

    window.history.pushState({}, '', anchor.href);
    handleLocation(anchor.href);
});

async function handleLocation(route: string) {
    let response = await fetch(route, { mode: 'same-origin', redirect: 'manual' });
    if (response.status != 200) {
        window.location.replace(route);
        return;
    }        

    let html = await response.text();
    document.getElementById('layout-body').innerHTML = html;
};

let currentPage: Page;

window.addEventListener('DOMContentLoaded', () => {
    const mutationObserver = new MutationObserver(onPageLoad);
    const layoutBody = document.getElementById('layout-body');

    mutationObserver.observe(layoutBody, { childList: true });

    onPageLoad();

    function onPageLoad() {
        currentPage?.dispose();

        let pageElement = layoutBody.firstElementChild;
        let pageId = pageElement?.id;
        if (pageElement == null || pageId == '') {
            currentPage = null;
            return;
        }

        let pageIdCamelCase = pageId.replace(/-./g, x => x[1].toUpperCase());

        let page = new globalThis[pageIdCamelCase] as Page;
        page?.initialize();

        currentPage = page;
    }
});