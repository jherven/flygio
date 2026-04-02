window.flygio = window.flygio || {};

window.flygio.trackEvent = function (eventName, data) {
    if (typeof umami !== 'undefined') {
        umami.track(eventName, data);
    }
};

window.flygio.trackSearch = function (origin, destination, departureDate, returnDate, passengers, resultCount) {
    window.flygio.trackEvent('flight_search', {
        origin: origin,
        destination: destination,
        departure_date: departureDate,
        return_date: returnDate || '',
        passengers: passengers,
        result_count: resultCount,
        route: origin + '_' + destination
    });
};

window.flygio.trackAffiliateClick = function (provider, origin, destination, price, sourcePage) {
    window.flygio.trackEvent('affiliate_click', {
        provider: provider,
        origin: origin,
        destination: destination,
        price: price,
        route: origin + '_' + destination,
        source_page: sourcePage || 'unknown'
    });
};

window.flygio.initAviasalesWidget = function (markerId) {
    var container = document.getElementById('aviasales-widget');
    if (!container) return;

    // Aviasales search widget via Travelpayouts white-label
    var script = document.createElement('script');
    script.src = 'https://tp.media/content?currency=sek&trs=373498&shmarker=' + markerId +
        '&locale=sv&powered_by=true&search_host=www.aviasales.com' +
        '&origin=STO&destination=&origin_name=Stockholm&destination_name=' +
        '&one_way=false&only_direct=false&trip_class=0' +
        '&promo_id=4880&campaign_id=101';
    script.async = true;
    script.charset = 'utf-8';
    container.appendChild(script);
};
