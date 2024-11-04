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
    const html = await fetch(route).then((data) => data.text());
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

        let pageId = layoutBody.firstElementChild?.id;
        if (pageId == null) {
            currentPage = null;
            return;
        }

        let pageIdCamelCase = pageId.replace(/-./g, x => x[1].toUpperCase());

        let page = new globalThis[pageIdCamelCase] as Page;
        page?.initialize();

        currentPage = page;
    }
});