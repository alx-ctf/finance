const STORAGE_KEY = 'ft-theme';

function resolveDark(mode) {
    if (mode === 'dark') return true;
    if (mode === 'light') return false;
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

function applyToDocument(dark) {
    document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
    return dark;
}

let dotnetRef = null;
let mediaListener = null;

export function getMode() {
    return localStorage.getItem(STORAGE_KEY) || 'system';
}

export function isDark() {
    return resolveDark(getMode());
}

export function apply(mode) {
    return applyToDocument(resolveDark(mode));
}

export function setMode(mode) {
    localStorage.setItem(STORAGE_KEY, mode);
    return applyToDocument(resolveDark(mode));
}

export function init(dotNetHelper) {
    dotnetRef = dotNetHelper;

    if (mediaListener) {
        window.matchMedia('(prefers-color-scheme: dark)').removeEventListener('change', mediaListener);
    }

    mediaListener = () => {
        if (getMode() === 'system' && dotnetRef) {
            apply('system');
            dotnetRef.invokeMethodAsync('OnSystemThemeChanged');
        }
    };

    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', mediaListener);
}
