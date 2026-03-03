# Flygio.se - Projektplan

## Fas 0: Uppdatera projektfiler

```json
[
  {
    "id": "0.1",
    "task": "Uppdatera PROMPT.md och plan.md till att reflektera alla beslut (Blazor SSR, .NET 10, ingen Anthropic API, Travelpayouts-first, ADMIN_API_KEY).",
    "passes": true
  }
]
```

## Fas 1: Foundation

```json
[
  {
    "id": "1.1",
    "task": "Skapa flygio.sln i reporoten. Skapa tests/Flygio.Tests/Flygio.Tests.csproj (xUnit, net10.0, referens till Flygio.csproj, Moq). En smoke-test som passerar. Verifiera: dotnet build flygio.sln + dotnet test.",
    "passes": true
  },
  {
    "id": "1.2",
    "task": "EF Core + PostgreSQL. NuGet: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design. Modeller: FlightRoute, PricePoint, PriceAlert, Article, AffiliateClick, Subscriber. FlygioDbContext med IDbContextFactory. DATABASE_URL-parsning. Auto-migration. Initial migration.",
    "passes": true
  },
  {
    "id": "1.3",
    "task": "Full layout i MainLayout.razor: sticky header med logo + nav, mobil hamburger-meny (CSS-only details/summary), footer med affiliate-disclaimer. Chart.js CDN i App.razor.",
    "passes": true
  },
  {
    "id": "1.4",
    "task": "Konfiguration: TravelpayoutsOptions, ResendOptions, IataData.cs med svenska flygplatser + destinationer, output caching middleware, health check /health, uppdatera railway.toml.",
    "passes": true
  }
]
```

## Fas 2: Flyg-API & Sidor

```json
[
  {
    "id": "2.1",
    "task": "IFlightSearchService interface + TravelpayoutsService: GetLatestPricesAsync, GetCheapestAsync, GetMonthMatrixAsync, GetCalendarPricesAsync, GetPopularRoutesAsync. HttpClient, X-Access-Token, rate limiting, IMemoryCache, DTOs, enhetstester.",
    "passes": true
  },
  {
    "id": "2.2",
    "task": "AffiliateService: generera Travelpayouts-länkar, generic link utan datum. Enhetstester.",
    "passes": true
  },
  {
    "id": "2.3",
    "task": "AmadeusService stub: implementerar IFlightSearchService, alla metoder kastar NotImplementedException. Villkorlig DI via FLIGHT_API_PROVIDER.",
    "passes": true
  },
  {
    "id": "2.4",
    "task": "Delade Blazor-komponenter: PriceCard.razor, AffiliateButton.razor, SearchForm.razor (@rendermode InteractiveServer), PriceGraph.razor (Chart.js). Uppdatera _Imports.razor.",
    "passes": true
  },
  {
    "id": "2.5",
    "task": "Startsidan Home.razor: hero med SearchForm, populära rutter, så funkar det-sektion, senaste artiklar, SEO.",
    "passes": true
  },
  {
    "id": "2.6",
    "task": "Söksidan /sok: query params from/to/date, GetLatestPricesAsync + GetCheapestAsync, PriceCard-grid, tom-state, noindex.",
    "passes": true
  },
  {
    "id": "2.7",
    "task": "Ruttsidan /flyg/{origin}-{destination}: prishistorik Chart.js, månadsmatris, prissammanfattning, affiliate CTA, SEO.",
    "passes": true
  },
  {
    "id": "2.8",
    "task": "Ruttlista /rutter: grid med populära rutter grupperade per avgångsflygplats.",
    "passes": true
  }
]
```

## Fas 3: Prisbevakning & Email

```json
[
  {
    "id": "3.1",
    "task": "PriceTrackingService (BackgroundService): var 6h, hämtar priser för bevakade rutter, sparar PricePoints. 30s startup-delay, sekventiell med rate limit.",
    "passes": true
  },
  {
    "id": "3.2",
    "task": "IEmailService + EmailService (Resend): HTTP POST, SendPriceAlertAsync, SendConfirmationEmailAsync. HTML-mallar på svenska med affiliate-CTA. Enhetstester.",
    "passes": true
  },
  {
    "id": "3.3",
    "task": "AlertService (BackgroundService): var 6h, kollar aktiva+bekräftade alerts, skickar mail vid prisfall >=10% eller under MaxPrice. Max 50 mail per körning.",
    "passes": true
  },
  {
    "id": "3.4",
    "task": "Bevakningssidor: /bevakning (formulär), /bevakning/bekrafta/{token}, /bevakning/avsluta/{token}. Max 5 alerts per email.",
    "passes": true
  },
  {
    "id": "3.5",
    "task": "DataSeeder.cs: populerar FlightRoute med populära rutter (ARN->15 dest, GOT->7, MMX->3). Idempotent. Körs efter migration.",
    "passes": true
  }
]
```

## Fas 4: SEO-innehåll

```json
[
  {
    "id": "4.1",
    "task": "Admin-endpoints: POST /admin/articles/import, GET /admin/articles, DELETE /admin/articles/{id}. Skyddat med ADMIN_API_KEY.",
    "passes": true
  },
  {
    "id": "4.2",
    "task": "Artikelsidor: /guider (grid med paginering), /guider/{slug} (full artikel med relaterade rutter). SEO: JSON-LD Article, OG-tags.",
    "passes": true
  },
  {
    "id": "4.3",
    "task": "Seed 12-15 artiklar i ArticleSeedData.cs med riktigt svenskt innehåll (800-1200 ord) för populära destinationer.",
    "passes": true
  },
  {
    "id": "4.4",
    "task": "SEO-endpoints: GET /sitemap.xml (dynamisk), GET /robots.txt. Disallow /admin/ och /api/.",
    "passes": true
  }
]
```

## Fas 5: Affiliate-tracking

```json
[
  {
    "id": "5.1",
    "task": "Klick-tracking: GET /go?url={}&route={}&dest={}. Validerar URL-domän. Loggar AffiliateClick. 302 redirect. Uppdatera AffiliateButton.",
    "passes": true
  },
  {
    "id": "5.2",
    "task": "Admin-stats /admin/stats?key={}: klick idag/vecka/månad, top 10 destinationer, klick per dag senaste 30 dagar.",
    "passes": true
  }
]
```

## Fas 6: Polish & Production

```json
[
  {
    "id": "6.1",
    "task": "Felsidor på svenska: 404 med sökförslag, 500 med vänligt meddelande.",
    "passes": true
  },
  {
    "id": "6.2",
    "task": "Output caching: startsida 10 min, ruttsidor 1h, artiklar 1h, ruttlista 1h.",
    "passes": true
  },
  {
    "id": "6.3",
    "task": "SEO-audit: alla sidor har unik title, meta desc, canonical, OG-tags, H1, JSON-LD, rel=nofollow sponsored.",
    "passes": true
  },
  {
    "id": "6.4",
    "task": "Graceful empty states: alla sidor hanterar tom data med vänliga meddelanden. Ogiltiga IATA -> 404.",
    "passes": true
  },
  {
    "id": "6.5",
    "task": "Dockerfile-verifiering: Docker-build fungerar med alla dependencies. Favicon.",
    "passes": true
  },
  {
    "id": "6.6",
    "task": "Slutverifiering: dotnet build utan varningar, dotnet test grönt, Dockerfile bygger.",
    "passes": true
  }
]
```
