# Flygio.se - Projektplan

## Fas 1: Projektgrund

```json
[
  {
    "id": "1.1",
    "task": "Skapa solution-struktur: flygio.sln, src/Flygio/Flygio.csproj (.NET 8 web), tests/Flygio.Tests/Flygio.Tests.csproj. Lägg till .gitignore för .NET. Verifiera att dotnet build fungerar.",
    "passes": false
  },
  {
    "id": "1.2",
    "task": "Konfigurera Dockerfile för Railway-deploy: multi-stage build (sdk -> aspnet runtime), exponera PORT från miljövariabel. Skapa railway.toml med build/deploy-config.",
    "passes": false
  },
  {
    "id": "1.3",
    "task": "Sätt upp Entity Framework Core med PostgreSQL-provider (Npgsql). Skapa FlygioDbContext. Konfigurera anslutningssträng via DATABASE_URL miljövariabel (Railway-format). Skapa alla modeller: FlightRoute, PricePoint, PriceAlert, Subscriber, Article. Skapa initial migration.",
    "passes": false
  },
  {
    "id": "1.4",
    "task": "Konfigurera grundläggande Razor Pages med _Layout.cshtml. Inkludera Tailwind CSS via CDN, Chart.js via CDN, Travelpayouts tracking-script (https://emrld.cc/NTAzOTk0.js?t=503994). Skapa responsiv layout med header (logo + nav), main content area, footer (om oss, kontakt, affiliate-disclaimer). Mobil-först design med hamburgermeny.",
    "passes": false
  }
]
```

## Fas 2: Flyg-API & Prissökning

```json
[
  {
    "id": "2.1",
    "task": "Skapa IFlightSearchService-interfacet med metoder: SearchFlightsAsync, GetPriceHistoryAsync, GetPopularRoutesAsync, GetCalendarPricesAsync. Implementera TravelpayoutsService som använder Travelpayouts Flight Data API (base URL: https://api.travelpayouts.com/v2/, auth via X-Access-Token header). Implementera endpoints: /prices/latest, /prices/cheap, /prices/calendar, /city-directions. Cacha svar i 1 timme. Registrera i DI. Skriv enhetstester med mockad HttpClient.",
    "passes": false
  },
  {
    "id": "2.2",
    "task": "Skapa AmadeusService som implementerar IFlightSearchService (förberett för framtida bruk). OAuth2 client credentials flow med token caching. Registrera villkorligt i DI baserat på FLIGHT_API_PROVIDER miljövariabel (default: travelpayouts). Klassen ska finnas och kompilera men används inte i v1.",
    "passes": false
  },
  {
    "id": "2.3",
    "task": "Bygg Search-sidan (Pages/Search.cshtml): formulär med från/till (svenska städer med IATA-koder), datum. Visa resultat som kort med pris, flygbolag, restid, antal stopp. Varje resultat ska ha en Travelpayouts affiliate-länk (format: https://www.aviasales.com/search/{origin}{destination}{date}?marker=503994). Responsiv grid-layout.",
    "passes": false
  },
  {
    "id": "2.4",
    "task": "Bygg startsidan (Pages/Index.cshtml): hero-sektion med sökruta, sektion med populära rutter från ARN (hämtade via IFlightSearchService.GetPopularRoutesAsync med fallback till hårdkodade: ARN-BCN, ARN-BKK, ARN-LHR, ARN-AGP, ARN-ATH), sektion med senaste artiklar (placeholder). SEO: title, meta description, JSON-LD WebSite schema.",
    "passes": false
  },
  {
    "id": "2.5",
    "task": "Bygg Route-sidan (Pages/Route.cshtml): visa prishistorik för en specifik rutt som linjegraf (Chart.js via GetCalendarPricesAsync), lägsta/högsta/genomsnittspris, bästa månad att flyga, affiliate-knapp 'Boka flyg' med Travelpayouts-länk. SEO: JSON-LD BreadcrumbList.",
    "passes": false
  }
]
```

## Fas 3: Prisbevakning & Notiser

```json
[
  {
    "id": "3.1",
    "task": "Implementera PriceTrackingService som IHostedService: kör var 6:e timme, hämtar priser för alla bevakade rutter via IFlightSearchService, sparar PricePoint i db. Logga progress. Hantera API-fel och rate limits (max 200 req/h för Travelpayouts) utan att krascha.",
    "passes": false
  },
  {
    "id": "3.2",
    "task": "Implementera EmailService med Resend API: skicka HTML-mail med flygpris-info. Skapa enkel men snygg mailmall med pris, rutt, datum, affiliate-CTA-knapp (Travelpayouts-länk). Hantera fel och retry.",
    "passes": false
  },
  {
    "id": "3.3",
    "task": "Implementera AlertService som IHostedService: kör efter PriceTrackingService, jämför nya priser mot aktiva bevakningar, skickar mail via EmailService vid prisfall >= 10%. Markera alert som notifierad för att undvika dubbletter.",
    "passes": false
  },
  {
    "id": "3.4",
    "task": "Bygg Alert/Create-sidan: formulär för att skapa prisbevakning (rutt, maxpris, email). Validering server-side. Bekräftelsesida med info om bevakningen. Ingen inloggning krävs - email-baserat. Double opt-in via bekräftelsemail.",
    "passes": false
  }
]
```

## Fas 4: SEO-innehåll & Artiklar

```json
[
  {
    "id": "4.1",
    "task": "Implementera ContentService: anropa Claude API (Anthropic SDK) för att generera SEO-artiklar. Prompt-template för destinationsguider ('Flyga till {destination} - Guide {år}') och prisanalyser ('Billigaste flygbiljetterna till {destination}'). Spara Article i db med slug, title, content, meta_description, published_at.",
    "passes": false
  },
  {
    "id": "4.2",
    "task": "Implementera ContentGenerationService som IHostedService: kör en gång per vecka, genererar artiklar för populära destinationer som saknar uppdaterat innehåll (äldre än 30 dagar). Max 5 artiklar per körning (kostnadskontroll).",
    "passes": false
  },
  {
    "id": "4.3",
    "task": "Bygg Guides/Index.cshtml (artikellistning): grid med artikelkort (bild-placeholder, titel, utdrag, datum). Paginering. Bygg Guides/Article.cshtml: full artikel med innehåll, relaterade rutter med priser, affiliate-CTA. SEO: JSON-LD Article schema, canonical URL, Open Graph.",
    "passes": false
  },
  {
    "id": "4.4",
    "task": "Implementera dynamisk sitemap.xml (endpoint som genererar XML): alla rutter, alla artiklar, startsidan. Skapa robots.txt som tillåter crawling och pekar på sitemap. Lägg till rel=canonical på alla sidor.",
    "passes": false
  }
]
```

## Fas 5: Affiliate & Monetarisering

```json
[
  {
    "id": "5.1",
    "task": "Implementera AffiliateService: generera Travelpayouts affiliate-länkar baserat på rutt och datum (format: https://www.aviasales.com/search/{origin}{destination}{date}?marker=503994). Stöd för deep links till Kiwi och WayAway via Travelpayouts. Fallback till Skyscanner/Google Flights med UTM. Alla utgående affiliate-länkar ska ha rel='nofollow sponsored'.",
    "passes": false
  },
  {
    "id": "5.2",
    "task": "Integrera affiliate-länkar i alla prisvisningar: Search-sidan, Route-sidan, artiklar, startsidan. Skapa _AffiliateButton.cshtml partial med tydlig CTA ('Se pris & boka', 'Jämför pris'). Lägg till affiliate-disclaimer i footer.",
    "passes": false
  },
  {
    "id": "5.3",
    "task": "Implementera klick-tracking: logga affiliate-klick (rutt, destination, timestamp) i db för enkel analys. Endpoint /api/track som redirectar till affiliate-URL efter loggning. Enkel admin-vy (/admin/stats) med klickstatistik per rutt och dag.",
    "passes": false
  }
]
```

## Fas 6: Polish & Deploy

```json
[
  {
    "id": "6.1",
    "task": "SEO-granskning: verifiera att alla sidor har unik title (<60 tecken), meta description (<160 tecken), Open Graph-taggar, korrekt heading-hierarki (en H1 per sida), alt-text på bilder, semantisk HTML. Lägg till favicon.",
    "passes": false
  },
  {
    "id": "6.2",
    "task": "Performance: aktivera Response Caching middleware, Output Caching på statiska sidor (1h), cache Travelpayouts-svar i 1 timme (API-data uppdateras sällan). Minifiera CSS/JS eller verifiera CDN-leverans. Verifiera att sidor laddar under 2 sekunder.",
    "passes": false
  },
  {
    "id": "6.3",
    "task": "Felsidor och edge cases: custom 404-sida med sökförslag, custom 500-sida, hantera tom databas gracefully (visa 'inga resultat' istället för krasch), rate limit på alert-skapande (max 5 per email).",
    "passes": false
  },
  {
    "id": "6.4",
    "task": "Slutgiltig verifiering: dotnet build utan varningar, dotnet test alla tester gröna, alla sidor returnerar 200, kör lighthouse-liknande check på SEO och accessibility. Verifiera Dockerfile bygger korrekt. Skriv kort README.md med setup-instruktioner.",
    "passes": false
  }
]
```
