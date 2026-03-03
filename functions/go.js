const ALLOWED_HOSTS = new Set([
  'www.agoda.com',
  'www.airbnb.se',
  'www.apollo.se',
  'www.autoeurope.se',
  'www.booking.com',
  'www.erv.se',
  'www.europcar.se',
  'www.europeiska.se',
  'www.expedia.se',
  'www.flysas.com',
  'www.fritidsresor.se',
  'www.getyourguide.com',
  'www.google.com',
  'www.gouda.se',
  'www.hedvig.com',
  'www.hertz.se',
  'www.hostelworld.com',
  'www.hotels.com',
  'www.if.se',
  'www.kayak.se',
  'www.kiwi.com',
  'www.klook.com',
  'www.momondo.se',
  'www.norwegian.com',
  'www.rentalcars.com',
  'www.sixt.se',
  'www.skyscanner.se',
  'www.stsalpresor.se',
  'www.ticket.se',
  'www.tiqets.com',
  'www.trivago.se',
  'www.tui.se',
  'www.viator.com',
  'www.ving.se',
  'www.wizzair.com',
]);

export async function onRequest(context) {
  const url = new URL(context.request.url);
  const target = url.searchParams.get('url');

  if (!target) {
    return new Response('Missing url parameter', { status: 400 });
  }

  let parsed;
  try {
    parsed = new URL(target);
    if (!['http:', 'https:'].includes(parsed.protocol)) {
      return new Response('Invalid URL protocol', { status: 400 });
    }
  } catch {
    return new Response('Invalid URL', { status: 400 });
  }

  if (!ALLOWED_HOSTS.has(parsed.hostname)) {
    return new Response('Destination not allowed', { status: 403 });
  }

  console.log(`[go] Redirect: ${target} | Referer: ${context.request.headers.get('referer') || 'direct'}`);

  return Response.redirect(target, 302);
}
