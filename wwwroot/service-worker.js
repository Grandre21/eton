// In sviluppo il service worker non deve mettere niente in cache: altrimenti si continua a
// vedere la versione precedente dell'app dopo ogni modifica. Quello vero è
// service-worker.published.js, che il publish mette al suo posto.
self.addEventListener('fetch', () => { });
