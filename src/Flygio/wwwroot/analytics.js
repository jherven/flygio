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

window.flygio.trackAffiliateClick = function (provider, origin, destination, price) {
    window.flygio.trackEvent('affiliate_click', {
        provider: provider,
        origin: origin,
        destination: destination,
        price: price,
        route: origin + '_' + destination
    });
};
