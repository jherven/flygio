using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Services;

public static class SeoContentSeeder
{
    public static async Task SeedMonthlyPriceTrendArticlesAsync(FlygioDbContext db)
    {
        var months = new (int Month, string Name, string Slug)[]
        {
            (1, "januari", "januari"), (2, "februari", "februari"), (3, "mars", "mars"),
            (4, "april", "april"), (5, "maj", "maj"), (6, "juni", "juni"),
            (7, "juli", "juli"), (8, "augusti", "augusti"), (9, "september", "september"),
            (10, "oktober", "oktober"), (11, "november", "november"), (12, "december", "december")
        };

        foreach (var (month, name, slug) in months)
        {
            var articleSlug = $"billigaste-flygbiljetterna-i-{slug}-2026";
            if (await db.Articles.AnyAsync(a => a.Slug == articleSlug))
                continue;

            var article = new Article
            {
                Title = $"Billigaste flygbiljetterna i {name} 2026",
                Slug = articleSlug,
                MetaDescription = $"Hitta de billigaste flygbiljetterna i {name} 2026. Jämför priser till populära destinationer och boka ditt flyg till bästa pris.",
                CategorySlug = "pristrend",
                IsPublished = true,
                Body = GenerateMonthlyArticle(name, month)
            };

            db.Articles.Add(article);
        }

        await db.SaveChangesAsync();
    }

    public static async Task SeedTravelGuideArticlesAsync(FlygioDbContext db)
    {
        var destinations = new (string City, string Slug, string Country, string AirportCode)[]
        {
            ("Barcelona", "barcelona", "Spanien", "BCN"),
            ("London", "london", "Storbritannien", "LHR"),
            ("Bangkok", "bangkok", "Thailand", "BKK"),
            ("Paris", "paris", "Frankrike", "CDG"),
            ("Rom", "rom", "Italien", "FCO"),
            ("Amsterdam", "amsterdam", "Nederländerna", "AMS"),
            ("Berlin", "berlin", "Tyskland", "BER"),
            ("Istanbul", "istanbul", "Turkiet", "IST"),
            ("Prag", "prag", "Tjeckien", "PRG"),
            ("Budapest", "budapest", "Ungern", "BUD"),
            ("Lissabon", "lissabon", "Portugal", "LIS"),
            ("Málaga", "malaga", "Spanien", "AGP"),
            ("Dublin", "dublin", "Irland", "DUB"),
            ("Wien", "wien", "Österrike", "VIE"),
            ("Aten", "aten", "Grekland", "ATH"),
            ("Dubai", "dubai", "Förenade Arabemiraten", "DXB"),
            ("New York", "new-york", "USA", "JFK"),
            ("Palma de Mallorca", "palma-de-mallorca", "Spanien", "PMI"),
            ("Nice", "nice", "Frankrike", "NCE"),
            ("Alicante", "alicante", "Spanien", "ALC"),
        };

        foreach (var (city, slug, country, code) in destinations)
        {
            var articleSlug = $"resa-till-{slug}";
            if (await db.Articles.AnyAsync(a => a.Slug == articleSlug))
                continue;

            var article = new Article
            {
                Title = $"Resa till {city} — allt du behöver veta",
                Slug = articleSlug,
                MetaDescription = $"Komplett reseguide till {city}, {country}. Praktiska tips, bästa tiden att resa, transport, boende och sevärdheter.",
                CategorySlug = "reseguide",
                IsPublished = true,
                Body = GenerateTravelGuide(city, country, code, slug)
            };

            db.Articles.Add(article);
        }

        await db.SaveChangesAsync();
    }

    private static string GenerateMonthlyArticle(string monthName, int month)
    {
        var season = month switch
        {
            >= 1 and <= 3 => "vinter",
            >= 4 and <= 6 => "vår",
            >= 7 and <= 8 => "sommar",
            >= 9 and <= 11 => "höst",
            12 => "vinter",
            _ => ""
        };

        var seasonTip = month switch
        {
            1 or 2 or 3 => "Vintern är generellt lågsäsong för flygpriser till de flesta europeiska destinationer, vilket innebär att du kan hitta riktigt bra deals. Undantaget är sportlovsveckorna (vecka 7–9) då priserna till alperna och soldestinationer stiger kraftigt.",
            4 or 5 => "Våren erbjuder en fin balans mellan bra väder och rimliga priser. Påskhelgen kan driva upp priserna, men i övrigt är detta en utmärkt tid att resa. Medelhavsländerna har behagliga temperaturer utan sommarens trängsel.",
            6 => "Juni markerar starten på högsäsongen för flygpriser. Skolavslutning och midsommar driver upp priserna, särskilt till populära soldestinationer. Boka tidigt för bäst priser.",
            7 or 8 => "Sommaren är högsäsong med de högsta flygpriserna. Charterflygen fyller kapaciteten och lågprisbolagen höjer basepriserna. Tips: res i början av juli eller slutet av augusti för något lägre priser.",
            9 or 10 => "Hösten är ofta \"sweet spot\" för resenärer – bra väder i södra Europa kombinerat med betydligt lägre priser. September och oktober är särskilt bra för Medelhavet.",
            11 => "November är en av de billigaste månaderna att flyga. Undantaget är resa runt alla helgons dag. I övrigt kan du hitta riktiga fyndpriser, särskilt till stortstäder.",
            12 => "December har extremt varierande priser. Tidiga december (1–15) kan erbjuda bra deals, men jul- och nyårsperioden har årets högsta priser på de flesta rutter.",
            _ => ""
        };

        var popularDests = month switch
        {
            1 or 2 or 3 => "<li><a href=\"/flyg-till/bangkok\"><strong>Bangkok</strong></a> — Lågsäsong = låga priser, men bra väder i Thailand</li>\n<li><a href=\"/flyg-till/malaga\"><strong>Málaga</strong></a> — Solgaranti och vintervärme</li>\n<li><a href=\"/flyg-till/alicante\"><strong>Alicante</strong></a> — Populär övervintringsdestination</li>\n<li><a href=\"/flyg-till/london\"><strong>London</strong></a> — Kulturhelg till lågt pris</li>",
            4 or 5 => "<li><a href=\"/flyg-till/barcelona\"><strong>Barcelona</strong></a> — Perfekt vårväder</li>\n<li><a href=\"/flyg-till/rom\"><strong>Rom</strong></a> — Undvik sommarhettan</li>\n<li><a href=\"/flyg-till/lissabon\"><strong>Lissabon</strong></a> — Charmig vårstad</li>\n<li><a href=\"/flyg-till/aten\"><strong>Aten</strong></a> — Behagliga temperaturer</li>",
            6 or 7 or 8 => "<li><a href=\"/flyg-till/palma-de-mallorca\"><strong>Palma de Mallorca</strong></a> — Klassisk sommaröns</li>\n<li><a href=\"/flyg-till/nice\"><strong>Nice</strong></a> — Franska Rivieran</li>\n<li><a href=\"/flyg-till/dublin\"><strong>Dublin</strong></a> — Svalare alternativ</li>\n<li><a href=\"/flyg-till/prag\"><strong>Prag</strong></a> — Prisvärd stadssemester</li>",
            9 or 10 => "<li><a href=\"/flyg-till/barcelona\"><strong>Barcelona</strong></a> — Bästa tiden att besöka</li>\n<li><a href=\"/flyg-till/istanbul\"><strong>Istanbul</strong></a> — Fantastisk höststad</li>\n<li><a href=\"/flyg-till/budapest\"><strong>Budapest</strong></a> — Charmigt och billigt</li>\n<li><a href=\"/flyg-till/malaga\"><strong>Málaga</strong></a> — Fortfarande strandväder</li>",
            11 or 12 => "<li><a href=\"/flyg-till/bangkok\"><strong>Bangkok</strong></a> — Högsäsong men fantastiskt väder</li>\n<li><a href=\"/flyg-till/dubai\"><strong>Dubai</strong></a> — Behaglig vintertemperatur</li>\n<li><a href=\"/flyg-till/wien\"><strong>Wien</strong></a> — Magiska julmarknader</li>\n<li><a href=\"/flyg-till/prag\"><strong>Prag</strong></a> — Julstämning till lågt pris</li>",
            _ => ""
        };

        return $@"<h2>Flygpriser i {monthName} 2026</h2>
<p>{monthName} är en {season}månad som erbjuder specifika möjligheter för den prismedvetna resenären. Här guidar vi dig till de bästa flygdealsen.</p>

<h3>Pristrend för {monthName}</h3>
<p>{seasonTip}</p>

<h3>Bästa destinationerna i {monthName}</h3>
<ul>
{popularDests}
</ul>

<h3>Tips för att hitta billiga flygbiljetter i {monthName}</h3>
<ul>
<li><strong>Boka i tid</strong> — För Europaflygningar, boka 6–8 veckor före avresa</li>
<li><strong>Var flexibel med datum</strong> — Priser kan variera stort mellan dagarna</li>
<li><strong>Jämför flygbolag</strong> — Använd <a href=""/"">Flygio</a> för att jämföra priser</li>
<li><strong>Överväg alternativa flygplatser</strong> — Skavsta och Landvetter kan ha bättre priser</li>
</ul>

<h3>Populära flygbolag i {monthName}</h3>
<p>Under {monthName} erbjuder följande flygbolag ofta konkurrenskraftiga priser från Sverige:</p>
<ul>
<li><strong>SAS</strong> — Bra utbud från alla svenska flygplatser</li>
<li><strong>Norwegian</strong> — Konkurrenskraftiga priser på populära rutter</li>
<li><strong>Ryanair</strong> — Lägsta basepriserna till europeiska städer</li>
<li><strong>Wizz Air</strong> — Billiga alternativ till Östeuropa</li>
</ul>

<p>Jämför alltid priser via <a href=""/"">Flygio</a> för att hitta de bästa flygbiljetterna i {monthName} 2026. Vi samlar priser från flera flygbolag och resebyryer så att du slipper söka överallt.</p>";
    }

    private static string GenerateTravelGuide(string city, string country, string code, string slug)
    {
        return $@"<h2>Resa till {city} — Din kompletta reseguide</h2>
<p>{city} i {country} är ett populärt resmål för svenska resenärer. Här hittar du allt du behöver veta innan du bokar din resa.</p>

<h3>Flyg till {city}</h3>
<p>Den huvudsakliga flygplatsen i {city} har IATA-koden <strong>{code}</strong>. Från Sverige flyger flera flygbolag direkt eller med mellanlandning till {city}. <a href=""/flyg-till/{slug}"">Jämför flygpriser till {city}</a> för att hitta det bästa erbjudandet.</p>

<h3>Bästa tiden att resa</h3>
<p>Bästa tiden att besöka {city} beror på dina preferenser. Generellt erbjuder vår (april–juni) och höst (september–oktober) den bästa kombinationen av bra väder och rimliga priser. Sommaren kan vara varm och dyr, medan vintern erbjuder de lägsta priserna.</p>

<h3>Boende i {city}</h3>
<p>Det finns gott om boendealternativ i {city}, från budgethotell till lyxiga femstjärniga anläggningar. <a href=""/hotell/{slug}"">Jämför hotellpriser i {city}</a> för att hitta bästa erbjudandet.</p>

<h3>Transport på plats</h3>
<p>Från flygplatsen till centrum finns det vanligtvis flera transportalternativ: buss, tåg och taxi. Inom {city} kan du röra dig med kollektivtrafik, taxi eller <a href=""/hyrbil/{slug}"">hyrbil</a>.</p>

<h3>Sevärdheter och aktiviteter</h3>
<p>{city} erbjuder en rad sevärdheter och <a href=""/aktiviteter/{slug}"">aktiviteter</a> för alla smaker. Från kulturella upplevelser till kulinariska upptäckter finns det mycket att se och göra.</p>

<h3>Mat och dryck</h3>
<p>Det lokala köket i {country} är en viktig del av reseupplevelsen. {city} erbjuder allt från gatumat till finare restauranger. Våga testa de lokala specialiteterna!</p>

<h3>Praktisk information</h3>
<ul>
<li><strong>Valuta:</strong> Se aktuell växelkurs innan du reser</li>
<li><strong>Språk:</strong> Engelska talas generellt bra i turistområden</li>
<li><strong>Visum:</strong> Kontrollera eventuella visumkrav för {country}</li>
<li><strong>Reseförsäkring:</strong> Rekommenderas alltid</li>
</ul>

<h3>Boka din resa</h3>
<p>Redo att boka? <a href=""/flyg-till/{slug}"">Sök flygbiljetter till {city}</a> och <a href=""/hotell/{slug}"">jämför hotellpriser</a> direkt på Flygio. Du kan också kolla in våra <a href=""/artiklar"">researtiklar</a> för fler tips och inspiration.</p>";
    }
}
