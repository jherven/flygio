# Flygio.se - Flygpris-tracker & Reseguide

## Projektöversikt

Flygio.se är en svensk flygpris-tracker kombinerad med reseguider. Användare kan bevaka flygpriser och få notiser vid prisfall. Sajten driver trafik via SEO-optimerade artiklar och monetariseras genom affiliate-länkar till bokningssajter.

## Teknisk stack

- **Backend:** C# / .NET 10, ASP.NET Core
- **Frontend:** Blazor Static SSR (Server-Side Rendering, kritiskt för SEO)
- **Databas:** PostgreSQL (Railway addon)
- **ORM:** Entity Framework Core med IDbContextFactory (Blazor-rekommenderat)
- **Bakgrundsjobb:** Hosted Services (BackgroundService) för prisbevakning
- **Flyg-API (v1):** Travelpayouts Flight Data API (ingår i affiliate-konto, gratis)
- **Flyg-API (v2, framtida):** Amadeus Self-Service API (förberett i koden via interface)
- **Innehåll:** Artiklar genereras via Claude Code, importeras via admin-endpoint (ingen Anthropic API i runtime)
- **Email:** Resend (gratis tier: 3000 mail/mån)
- **Cache:** In-memory cache + output caching för sidor
- **Hosting:** Railway (Docker-baserad deploy)
- **CSS:** Tailwind CSS via CDN (enkel setup, inga byggsteg)

## Arkitektur

```
flygio.se/
├── src/
│   └── Flygio/
│       ├── Program.cs                 # App startup, DI, middleware, endpoints
│       ├── Flygio.csproj
│       ├── Dockerfile
│       │
│       ├── Data/
│       │   ├── FlygioDbContext.cs
│       │   ├── DataSeeder.cs
│       │   ├── ArticleSeedData.cs
│       │   └── Migrations/
│       │
│       ├── Models/
│       │   ├── FlightRoute.cs         # Avgång, destination
│       │   ├── PricePoint.cs          # Pris vid en viss tidpunkt
│       │   ├── PriceAlert.cs          # Användarens bevakning
│       │   ├── Article.cs             # SEO-artikel
│       │   ├── AffiliateClick.cs      # Klick-tracking
│       │   └── Subscriber.cs          # Email-prenumerant
│       │
│       ├── Services/
│       │   ├── IFlightSearchService.cs
│       │   ├── TravelpayoutsService.cs
│       │   ├── AmadeusService.cs          # Stub, förberett
│       │   ├── PriceTrackingService.cs    # BackgroundService: hämtar priser
│       │   ├── AlertService.cs            # BackgroundService: skickar alerts
│       │   ├── IEmailService.cs
│       │   ├── EmailService.cs            # Resend-integration
│       │   └── AffiliateService.cs        # Genererar affiliate-länkar
│       │
│       ├── Configuration/
│       │   ├── TravelpayoutsOptions.cs
│       │   ├── ResendOptions.cs
│       │   └── IataData.cs                # Svenska flygplatser + destinationer
│       │
│       ├── Components/
│       │   ├── App.razor                  # Root: CDN-scripts, meta
│       │   ├── Routes.razor
│       │   ├── _Imports.razor
│       │   ├── Layout/
│       │   │   └── MainLayout.razor       # Header, nav, footer
│       │   ├── Pages/
│       │   │   ├── Home.razor             # Startsida
│       │   │   ├── Search.razor           # /sok
│       │   │   ├── RouteDetail.razor      # /flyg/{origin}-{destination}
│       │   │   ├── RouteList.razor        # /rutter
│       │   │   ├── AlertCreate.razor      # /bevakning
│       │   │   ├── AlertConfirm.razor     # /bevakning/bekrafta/{token}
│       │   │   ├── AlertCancel.razor      # /bevakning/avsluta/{token}
│       │   │   ├── GuideList.razor        # /guider
│       │   │   ├── GuideArticle.razor     # /guider/{slug}
│       │   │   ├── Error.razor
│       │   │   └── NotFound.razor
│       │   └── Shared/
│       │       ├── PriceCard.razor
│       │       ├── AffiliateButton.razor
│       │       ├── SearchForm.razor       # @rendermode InteractiveServer
│       │       └── PriceGraph.razor       # Chart.js wrapper
│       │
│       └── wwwroot/
│           ├── app.css
│           └── favicon.png
│
├── tests/
│   └── Flygio.Tests/
│       ├── Flygio.Tests.csproj
│       └── Services/
│
├── flygio.sln
├── PROMPT.md
├── plan.md
├── activity.md
├── railway.toml
└── .gitignore
```

## Flyg-API design

```csharp
public interface IFlightSearchService
{
    Task<List<FlightOffer>> GetLatestPricesAsync(string origin, string destination);
    Task<List<FlightOffer>> GetCheapestAsync(string origin, string destination);
    Task<List<CalendarPrice>> GetMonthMatrixAsync(string origin, string destination);
    Task<List<CalendarPrice>> GetCalendarPricesAsync(string origin, string destination, int month, int year);
    Task<List<PopularRoute>> GetPopularRoutesAsync(string origin = "ARN");
}
```

### v1: TravelpayoutsService (aktiv)
- **Base URL:** `https://api.travelpayouts.com/v2/`
- **Auth:** `X-Access-Token` header med API-token
- **Rate limit:** 60 req/min → SemaphoreSlim + 1.1s delay
- **Caching:** IMemoryCache, 1 timme per endpoint
- **Endpoints:** `/prices/latest`, `/prices/month-matrix`, `/prices/cheap`, `/prices/calendar`, `/city-directions`

### v2: AmadeusService (stub)
- Implementerar `IFlightSearchService`, alla metoder kastar `NotImplementedException`
- Aktiveras via `FLIGHT_API_PROVIDER=amadeus`

## Principer

1. **Blazor Static SSR** - Microsofts primära investering, framtidssäkert
2. **Progressiv leverans** - Varje task resulterar i något körbart
3. **Inga overengineering** - Minimal abstraktion, YAGNI
4. **SEO först** - Korrekt meta, JSON-LD, semantisk HTML
5. **Mobil först** - Responsive design med Tailwind
6. **Affiliate-integration** - Alla flygpriser med klickbara affiliate-länkar
7. **Kostnadsmedveten** - Gratis-tiers, aggressiv caching

## Affiliate-strategi

- **Travelpayouts** (marker: 503994) - primär affiliate-partner
  - Tracking-script: `https://emrld.cc/NTAzOTk0.js?t=503994`
  - Affiliate-länkformat: `https://www.aviasales.com/search/{ORIGIN}{DDMM}{DEST}{DDMM}1?marker=503994`
  - Alla affiliate-länkar: `rel="nofollow sponsored"` + `target="_blank"`

## Miljövariabler

```
DATABASE_URL=postgresql://...           # Railway PostgreSQL
TRAVELPAYOUTS_API_TOKEN=xxx             # Från travelpayouts.com/developers/api
TRAVELPAYOUTS_MARKER=503994             # Affiliate marker ID
FLIGHT_API_PROVIDER=travelpayouts       # Byt till "amadeus" för v2
RESEND_API_KEY=xxx                      # Email-tjänst
ADMIN_API_KEY=xxx                       # Skyddar admin-endpoints
BASE_URL=https://flygio.se
```
