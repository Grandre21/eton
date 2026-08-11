// Attenzione ai caveat dell'offline nelle PWA: https://aka.ms/blazor-offline-considerations
self.importScripts('./service-worker-assets.js');

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

// Aggiornamento su richiesta: la pagina invia { type: 'SKIP_WAITING' } quando l'utente accetta di
// aggiornare, e solo allora il service worker in attesa si attiva. NESSUNO skipWaiting automatico
// in onInstall: attivare una versione nuova sotto i piedi di una sessione aperta significa
// mescolare codice vecchio e nuovo nella stessa pagina. Il banner che invia il messaggio arriva
// in una fetta successiva; il gestore sta qui da subito perché cambiare la strategia di
// aggiornamento a service worker già installati sui telefoni è molto più fastidioso che
// prevederla adesso.
self.addEventListener('message', event => {
    if (event.data?.type === 'SKIP_WAITING') self.skipWaiting();
});

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.webmanifest$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

// Base path derivato DINAMICAMENTE dallo scope: "/" in locale, "/eton/" su GitHub Pages.
// 'self.location' è l'URL di questo script, quindi './' risolve la cartella da cui è servito:
// nessun percorso scritto a mano, e nessun sed da ricordare nel workflow di deploy.
const base = new URL('./', self.location).pathname;
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));

    // Prende il controllo dei client già aperti subito dopo l'attivazione: così al PRIMO
    // caricamento la pagina è già controllata dal service worker e l'app funziona offline da
    // subito, senza un reload manuale. Non intacca l'aggiornamento su richiesta: negli update
    // onActivate gira solo dopo lo SKIP_WAITING.
    await self.clients.claim();
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // Ogni richiesta di navigazione si risolve con l'index.html in cache, tranne quando punta
        // a una risorsa vera dell'app: è ciò che fa funzionare le rotte di Blazor offline.
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }
    return cachedResponse || fetch(event.request);
}
