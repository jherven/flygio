# Flygio.se - Flygpris-tracker & Reseguide

## Projektöversikt

Flygio.se är en svensk flygpris-tracker kombinerad med AI-genererade reseguider. Användare kan bevaka flygpriser och få notiser vid prisfall. Sajten driver trafik via SEO-optimerade artiklar och monetariseras genom affiliate-länkar till bokningssajter.

## Teknisk stack

- **Backend:** C# / .NET 8, ASP.NET Core
- **Frontend:** Razor Pages med server-side rendering (kritiskt för SEO)
- **Databas:** PostgreSQL (Railway addon)
- **ORM:** Entity Framework Core
- **Bakgrundsjobb:** Hosted Services (IHostedService) för prisbevakning och innehållsgenerering
- **Flyg-API (v1):** Travelpayouts Flight Data API (ingår i affiliate-konto, gratis)
- **Flyg-API (v2, framtida):** Amadeus Self-Service API (förberett i koden via interface/abstraktion)
- **AI-innehåll:** Anthropic Claude API för artikelgenerering
- **Email:** Resend (gratis tier: 3000 mail/mån)
- **Cache:** In-memory cache + output caching för sidor
- **Hosting:** Railway (Docker-baserad deploy)
- **CSS:** Tailwind CSS via CDN (enkel setup, inga byggsteg)

## Arkitektur

```
flygio.se/
├── src/
│   └── Flygio/
│       ├── Program.cs                 # App startup, DI, middleware
│       ├── Flygio.csproj
│       ├── Dockerfile
│       │
│       ├── Data/
│       │   ├── FlygioDbContext.cs
│       │   └── Migrations/
│       │
│       ├── Models/
│       │   ├── FlightRoute.cs         # Avgång, destination, flygbolag
│       │   ├── PricePoint.cs          # Pris vid en viss tidpunkt
│       │   ├── PriceAlert.cs          # Användarens bevakning
│       │   ├── Article.cs             # SEO-artikel
│       │   └── Subscriber.cs          # Email-prenumerant
│       │
│       ├── Services/
│       │   ├── IFlightSearchService.cs    # Interface: abstraktion för flyg-API
│       │   ├── TravelpayoutsService.cs    # v1: Travelpayouts Flight Data API
│       │   ├── AmadeusService.cs          # v2: Amadeus (förberett, ej aktivt)
│       │   ├── PriceTrackingService.cs    # Bakgrundsjobb: hämtar priser
│       │   ├── AlertService.cs            # Kollar bevakningar, skickar mail
│       │   ├── ContentService.cs          # AI-generering av artiklar
│       │   ├── EmailService.cs            # Resend-integration
│       │   └── AffiliateService.cs        # Genererar Travelpayouts affiliate-länkar
│       │
│       ├── Pages/
│       │   ├── Index.cshtml           # Startsida: sökruta + populära rutter
│       │   ├── Search.cshtml          # Sökresultat med priser
│       │   ├── Route.cshtml           # Enskild rutt: prisgraf + historik
│       │   ├── Alert/
│       │   │   └── Create.cshtml      # Skapa prisbevakning
│       │   ├── Guides/
│       │   │   ├── Index.cshtml       # Artikellistning
│       │   │   └── Article.cshtml     # Enskild artikel
│       │   ├── Shared/
│       │   │   ├── _Layout.cshtml     # Huvudlayout med nav, footer
│       │   │   └── _Partials/
│       │   │       ├── _PriceCard.cshtml
│       │   │       └── _AffiliateButton.cshtml
│       │   └── Error.cshtml
│       │
│       └── wwwroot/
│           ├── css/site.css
│           ├── js/site.js             # Minimal JS: prisgraf (Chart.js)
│           ├── robots.txt
│           └── sitemap.xml            # Dynamisk sitemap
│
├── tests/
│   └── Flygio.Tests/
│       ├── Flygio.Tests.csproj
│       └── Services/
│
├── PROMPT.md
├── plan.md
├── activity.md
├── .claude/
│   └── settings.json
├── railway.toml
├── .gitignore
└── flygio.sln
```

## Flyg-API design

Flyg-API:t ska abstraheras bakom ett interface så vi enkelt kan byta implementation:

```csharp
public interface IFlightSearchService
{
    Task<List<FlightOffer>> SearchFlightsAsync(string origin, string destination, DateTime departureDate, DateTime? returnDate = null);
    Task<List<PricePoint>> GetPriceHistoryAsync(string origin, string destination);
    Task<List<PopularRoute>> GetPopularRoutesAsync(string origin = "ARN");
    Task<Dictionary<string, decimal>> GetCalendarPricesAsync(string origin, string destination, int month, int year);
}
```

### v1: TravelpayoutsService (implementera nu)
- **Base URL:** `https://api.travelpayouts.com/v2/`
- **Auth:** `X-Access-Token` header med API-token
- **Rate limit:** 200 req/timme
- **Endpoints att använda:**
  - `/prices/latest` - senaste priser per rutt
  - `/prices/month-matrix` - prismatris per månad (kalendervy)
  - `/prices/cheap` - billigaste priser
  - `/prices/direct` - direktflyg-priser
  - `/prices/calendar` - priser per dag (för prisgraf)
  - `/city-directions` - populära destinationer från en stad
- **Caching:** Cacha alla svar i 1 timme (datan uppdateras sällan)

### v2: AmadeusService (förberett, ej aktivt)
- Implementera interfacet `IFlightSearchService`
- OAuth2 client credentials flow
- Aktiveras via miljövariabel `FLIGHT_API_PROVIDER=amadeus`
- Registrering i DI baserat på config

## Principer

1. **Enkel arkitektur** - Razor Pages, inte Blazor eller SPA. SSR för SEO.
2. **Progressiv leverans** - Varje task ska resultera i något körbart.
3. **Inga overengineering** - Minimal abstraktion, YAGNI. Undantag: IFlightSearchService-interfacet för API-byte.
4. **SEO först** - Alla publika sidor ska ha korrekt meta, structured data (JSON-LD), semantisk HTML.
5. **Mobil först** - Responsive design med Tailwind.
6. **Affiliate-integration** - Alla flygpriser ska ha klickbara affiliate-länkar.
7. **Kostnadsmedveten** - Använd gratis-tiers och cacha aggressivt.

## Arbetsflöde per iteration

1. Läs `activity.md` för att förstå nuvarande status.
2. Läs `plan.md` och välj den högst prioriterade uppgiften där `passes: false`.
3. Implementera uppgiften fullständigt.
4. Verifiera att koden kompilerar (`dotnet build`) och att eventuella tester passerar (`dotnet test`).
5. Uppdatera `plan.md` - sätt `passes: true` på avklarad uppgift.
6. Logga vad som gjordes i `activity.md` med timestamp.
7. Committa alla ändringar med ett beskrivande meddelande.
8. Om alla uppgifter har `passes: true`, sätt EXIT_SIGNAL: true i slutet av activity.md.

## Verifiering

- Alla sidor ska returnera 200 OK
- `dotnet build` ska lyckas utan varningar
- `dotnet test` ska passera
- Sidor ska ha korrekt `<title>`, `<meta description>`, och Open Graph-taggar

## Affiliate-strategi

- **Travelpayouts** (marker: 503994) - primär affiliate-partner
  - Tracking-script: `https://emrld.cc/NTAzOTk0.js?t=503994` (lägg i _Layout.cshtml)
  - Deep links för bokningar via Aviasales/Kiwi/WayAway
  - Affiliate-länkformat: `https://www.aviasales.com/search/{origin}{destination}{date}?marker=503994`
- Fallback: direktlänkar till Skyscanner/Google Flights med UTM-parametrar

## Miljövariabler som krävs

```
# Databas
DATABASE_URL=postgresql://...           # Railway PostgreSQL

# Flyg-API (v1 - krävs för deploy)
TRAVELPAYOUTS_API_TOKEN=xxx             # Från travelpayouts.com/developers/api
TRAVELPAYOUTS_MARKER=503994             # Affiliate marker ID

# Flyg-API (v2 - valfritt, framtida)
FLIGHT_API_PROVIDER=travelpayouts       # Byt till "amadeus" för v2
AMADEUS_CLIENT_ID=xxx                   # Amadeus API (ej konfigurerat än)
AMADEUS_CLIENT_SECRET=xxx

# Övrigt
ANTHROPIC_API_KEY=xxx                   # Claude för innehåll
RESEND_API_KEY=xxx                      # Email-tjänst
BASE_URL=https://flygio.se
```
