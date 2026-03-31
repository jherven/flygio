using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Services;

public static class DestinationSeeder
{
    public static async Task SeedAsync(FlygioDbContext db)
    {
        if (await db.Destinations.AnyAsync())
            return;

        var destinations = GetSeedDestinations();
        db.Destinations.AddRange(destinations);
        await db.SaveChangesAsync();
    }

    private static List<Destination> GetSeedDestinations() =>
    [
        new()
        {
            City = "Barcelona",
            Country = "Spanien",
            Slug = "barcelona",
            AirportCode = "BCN",
            MetaDescription = "Hitta billiga flyg till Barcelona från Sverige. Jämför priser, se aktuella erbjudanden och boka din resa till Kataloniens huvudstad.",
            Description = "<h2>Flyg till Barcelona från Sverige</h2><p>Barcelona är en av de mest populära destinationerna för svenska resenärer. Med sin fantastiska arkitektur, stränder och matkultur lockar staden miljontals besökare varje år.</p><h3>Bästa tiden att flyga</h3><p>Vår (april–juni) och höst (september–oktober) erbjuder behagligt väder och lägre priser. Sommaren är högsäsong med högre flygpriser.</p><h3>Flygplatsen</h3><p>Barcelona-El Prat (BCN) ligger ca 15 km från centrum. Aerobus tar dig till Plaça Catalunya på 35 minuter.</p>",
            IsPublished = true
        },
        new()
        {
            City = "London",
            Country = "Storbritannien",
            Slug = "london",
            AirportCode = "LHR",
            MetaDescription = "Billiga flyg till London från Sverige. Jämför priser till Heathrow, Gatwick och Stansted. Boka din Londonresa till bästa pris.",
            Description = "<h2>Flyg till London från Sverige</h2><p>London är en av världens mest besökta städer och lätt att nå från Sverige med flera dagliga avgångar. Välj mellan Heathrow, Gatwick, Stansted och Luton.</p><h3>Bästa tiden att flyga</h3><p>Januari–mars erbjuder ofta de lägsta priserna. Undvik skollov och storhelger för bäst deals.</p><h3>Tips</h3><p>Jämför priser till alla Londons flygplatser – Stansted och Luton har ofta billigare flygningar med lågprisbolag som Ryanair.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Bangkok",
            Country = "Thailand",
            Slug = "bangkok",
            AirportCode = "BKK",
            MetaDescription = "Hitta billiga flyg till Bangkok från Sverige. Jämför flygpriser, bästa restiderna och tips för din Thailandsresa.",
            Description = "<h2>Flyg till Bangkok från Sverige</h2><p>Bangkok är porten till Sydostasien och en av svenskarnas favoritdestinationer. Med direktflyg från Stockholm tar resan ca 11 timmar.</p><h3>Bästa tiden att flyga</h3><p>Lågsäsong (maj–oktober) ger de lägsta priserna, ofta från 4 000 kr tur och retur. Högsäsong (november–mars) kostar normalt 6 000–9 000 kr.</p><h3>Tips</h3><p>Flyg med mellanlandning via Doha, Dubai eller Istanbul kan ge betydligt lägre priser än direktflyg.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Málaga",
            Country = "Spanien",
            Slug = "malaga",
            AirportCode = "AGP",
            MetaDescription = "Billiga flyg till Málaga och Costa del Sol från Sverige. Jämför priser och hitta bästa erbjudandena till Solkusten.",
            Description = "<h2>Flyg till Málaga från Sverige</h2><p>Málaga är porten till Costa del Sol och en av de mest populära soldestinationerna för svenskar. Staden erbjuder både strandliv och kulturupplevelser.</p><h3>Bästa tiden att flyga</h3><p>Hela året passar för Málaga, men bäst priser hittar du september–november och februari–april. Sommaren är högsäsong.</p><h3>Tips</h3><p>Málaga är en utmärkt bas för att utforska Andalusien – Granada, Sevilla och Córdoba nås enkelt med tåg eller buss.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Aten",
            Country = "Grekland",
            Slug = "aten",
            AirportCode = "ATH",
            MetaDescription = "Hitta billiga flyg till Aten från Sverige. Jämför flygpriser och upptäck Greklands huvudstad med dess antika historia.",
            Description = "<h2>Flyg till Aten från Sverige</h2><p>Aten kombinerar antik historia med modernt stadsliv. Akropolis, Plaka och fantastisk grekisk mat gör staden till ett perfekt resmål.</p><h3>Bästa tiden att flyga</h3><p>Vår och höst (april–juni, september–oktober) är idealiskt – behagliga temperaturer och lägre priser. Sommaren kan vara extremt varm.</p><h3>Tips</h3><p>Aten är också en utmärkt utgångspunkt för att ta färjan till de grekiska öarna.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Alicante",
            Country = "Spanien",
            Slug = "alicante",
            AirportCode = "ALC",
            MetaDescription = "Billiga flyg till Alicante från Sverige. Jämför priser till Costa Blanca och hitta din drömresa till den spanska solkusten.",
            Description = "<h2>Flyg till Alicante från Sverige</h2><p>Alicante och Costa Blanca är ett klassiskt resmål för svenska solsökare. Vackra stränder, charmiga byar och prisvärt boende lockar året runt.</p><h3>Bästa tiden att flyga</h3><p>Vinter och tidig vår är populärt bland övervintrare. Lägsta flygpriserna hittar du januari–mars och oktober–november.</p><h3>Tips</h3><p>Alicante-Elche flygplats ligger nära staden och har bra bussförbindelser längs hela Costa Blanca.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Palma de Mallorca",
            Country = "Spanien",
            Slug = "palma-de-mallorca",
            AirportCode = "PMI",
            MetaDescription = "Hitta billiga flyg till Palma de Mallorca från Sverige. Jämför priser och boka din resa till Balearernas största ö.",
            Description = "<h2>Flyg till Palma de Mallorca från Sverige</h2><p>Mallorca är en av Medelhavets mest älskade öar med allt från strandliv till bergsvandringar i Serra de Tramuntana.</p><h3>Bästa tiden att flyga</h3><p>Maj–juni och september–oktober erbjuder perfekt väder och rimliga priser. Sommaren (juli–augusti) är högsäsong med många charterflyg.</p><h3>Tips</h3><p>Hyr bil för att utforska öns vackra inland och dolda vikar som inte nås med kollektivtrafik.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Paris",
            Country = "Frankrike",
            Slug = "paris",
            AirportCode = "CDG",
            MetaDescription = "Billiga flyg till Paris från Sverige. Jämför flygpriser till Charles de Gaulle och Orly. Boka din Parisresa idag.",
            Description = "<h2>Flyg till Paris från Sverige</h2><p>Paris behöver ingen introduktion – Eiffeltornet, Louvren, Notre-Dame och världens bästa matkultur väntar. Från Sverige tar flyget ca 2,5 timmar.</p><h3>Bästa tiden att flyga</h3><p>Januari–mars erbjuder lägst priser. Vår (april–juni) är underbart men dyrare. Undvik julhelgen för budget-resor.</p><h3>Tips</h3><p>Jämför priser till både Charles de Gaulle (CDG) och Orly (ORY) – ibland finns bättre deals till den mindre flygplatsen.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Rom",
            Country = "Italien",
            Slug = "rom",
            AirportCode = "FCO",
            MetaDescription = "Hitta billiga flyg till Rom från Sverige. Jämför priser och planera din resa till den eviga staden.",
            Description = "<h2>Flyg till Rom från Sverige</h2><p>Rom – den eviga staden – erbjuder oändligt med historia, konst och fantastisk mat. Colosseum, Vatikanen och Trastevere är bara början.</p><h3>Bästa tiden att flyga</h3><p>November–mars (utom jul) har de lägsta priserna. Vår och höst erbjuder bäst väder utan sommarens trängsel.</p><h3>Tips</h3><p>Fiumicino (FCO) är huvudflygplatsen. Leonardo Express-tåget tar dig till Roma Termini på 32 minuter.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Amsterdam",
            Country = "Nederländerna",
            Slug = "amsterdam",
            AirportCode = "AMS",
            MetaDescription = "Billiga flyg till Amsterdam från Sverige. Jämför flygpriser och boka din resa till kanalernas stad.",
            Description = "<h2>Flyg till Amsterdam från Sverige</h2><p>Amsterdam är perfekt för en weekend eller längre vistelse. Kanaler, museer, cykelkultur och ett livligt nattliv gör staden unik.</p><h3>Bästa tiden att flyga</h3><p>Tulpansäsongen (april–maj) är magisk men dyrare. Bäst priser hittar du januari–mars och november.</p><h3>Tips</h3><p>Schiphol flygplats har utmärkt tågförbindelse till Amsterdam Centraal – bara 15 minuter.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Berlin",
            Country = "Tyskland",
            Slug = "berlin",
            AirportCode = "BER",
            MetaDescription = "Hitta billiga flyg till Berlin från Sverige. Jämför priser och upptäck Tysklands kreativa och historiska huvudstad.",
            Description = "<h2>Flyg till Berlin från Sverige</h2><p>Berlin är en av Europas mest spännande städer – kreativ, historisk och prisvärd. Från museer i världsklass till hippa kvarter som Kreuzberg och Friedrichshain.</p><h3>Bästa tiden att flyga</h3><p>Hela året passar, men bäst priser hittar du januari–mars. Sommaren erbjuder massor av utomhusevenemang.</p><h3>Tips</h3><p>Berlin Brandenburg (BER) flygplats har snabbtåg till Hauptbahnhof på ca 30 minuter.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Istanbul",
            Country = "Turkiet",
            Slug = "istanbul",
            AirportCode = "IST",
            MetaDescription = "Billiga flyg till Istanbul från Sverige. Jämför flygpriser och utforska staden där Europa möter Asien.",
            Description = "<h2>Flyg till Istanbul från Sverige</h2><p>Istanbul är en fascinerande stad som sträcker sig över två kontinenter. Hagia Sofia, Blå moskén, Stora basaren och fantastisk turkisk mat gör den till ett unikt resmål.</p><h3>Bästa tiden att flyga</h3><p>Vår (april–juni) och höst (september–november) erbjuder bäst väder och rimliga priser. Turkish Airlines har ofta bra erbjudanden från Stockholm.</p><h3>Tips</h3><p>Istanbul Airport (IST) är Turkish Airlines huvudhubb med utmärkta anslutningar vidare till Asien och Mellanöstern.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Köpenhamn",
            Country = "Danmark",
            Slug = "kopenhamn",
            AirportCode = "CPH",
            MetaDescription = "Hitta billiga flyg till Köpenhamn från Sverige. Jämför priser eller ta Öresundståget till den danska huvudstaden.",
            Description = "<h2>Flyg till Köpenhamn från Sverige</h2><p>Köpenhamn är den närmaste storstaden för många svenskar. Nyhavn, Tivoli, fantastisk mat och skandinavisk design gör det till en perfekt weekendresa.</p><h3>Bästa tiden att flyga</h3><p>Hela året fungerar utmärkt. Sommaren (juni–augusti) har bäst väder men högre priser. Vintermånaderna erbjuder julstämning och lägre priser.</p><h3>Tips</h3><p>Överväg Öresundståget från Malmö/Lund som alternativ – ofta billigare och smidigare än flyg.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Dublin",
            Country = "Irland",
            Slug = "dublin",
            AirportCode = "DUB",
            MetaDescription = "Billiga flyg till Dublin från Sverige. Jämför priser och boka din resa till Irlands gröna och livliga huvudstad.",
            Description = "<h2>Flyg till Dublin från Sverige</h2><p>Dublin erbjuder en unik mix av historia, pubar, litteratur och vacker natur. Temple Bar, Guinness Storehouse och det omgivande landskapet gör staden till ett fantastiskt resmål.</p><h3>Bästa tiden att flyga</h3><p>Maj–september har bäst väder. Lägsta priserna hittar du januari–mars och november. St. Patrick's Day (17 mars) är festligt men dyrt.</p><h3>Tips</h3><p>Ryanair har ofta mycket billiga flygningar till Dublin från flera svenska flygplatser.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Lissabon",
            Country = "Portugal",
            Slug = "lissabon",
            AirportCode = "LIS",
            MetaDescription = "Hitta billiga flyg till Lissabon från Sverige. Jämför flygpriser till Portugals soliga och charmiga huvudstad.",
            Description = "<h2>Flyg till Lissabon från Sverige</h2><p>Lissabon är en av Europas mest charmiga huvudstäder. Kullarna, spårvagnarna, pastéis de nata och fado-musiken skapar en unik atmosfär.</p><h3>Bästa tiden att flyga</h3><p>Mars–maj och september–november erbjuder perfekt väder och lägre priser. Sommaren är varm och dyrare.</p><h3>Tips</h3><p>Lissabon är också en utmärkt utgångspunkt för dagsutflykter till Sintra, Cascais och Algarve-kusten.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Prag",
            Country = "Tjeckien",
            Slug = "prag",
            AirportCode = "PRG",
            MetaDescription = "Billiga flyg till Prag från Sverige. Jämför flygpriser och upptäck en av Europas vackraste och mest prisvärda städer.",
            Description = "<h2>Flyg till Prag från Sverige</h2><p>Prag är en av Europas mest fotogeniska städer med Karlsbron, Prags borg och en charmig gammelstad. Dessutom är mat, öl och boende avsevärt billigare än i Västeuropa.</p><h3>Bästa tiden att flyga</h3><p>Vår (april–juni) och höst (september–oktober) är idealiskt. Julmarknaden i december är magisk men drar mycket folk.</p><h3>Tips</h3><p>Flygplatsen ligger ca 30 minuter från centrum med buss 119 + metro.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Budapest",
            Country = "Ungern",
            Slug = "budapest",
            AirportCode = "BUD",
            MetaDescription = "Hitta billiga flyg till Budapest från Sverige. Jämför priser och utforska Ungerns vackra huvudstad vid Donau.",
            Description = "<h2>Flyg till Budapest från Sverige</h2><p>Budapest delas av Donau i Buda och Pest, med termalbad, fantastisk arkitektur och ett pulserande nattliv. Staden är dessutom en av Europas mest prisvärda huvudstäder.</p><h3>Bästa tiden att flyga</h3><p>Mars–maj och september–november erbjuder bäst väder och pris. Wizz Air har ofta billiga flygningar från Sverige.</p><h3>Tips</h3><p>Besök de berömda termalbaden – Széchenyi och Gellért är de mest kända.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Wien",
            Country = "Österrike",
            Slug = "wien",
            AirportCode = "VIE",
            MetaDescription = "Billiga flyg till Wien från Sverige. Jämför flygpriser och besök den kejserliga huvudstaden med kultur och kafétradition.",
            Description = "<h2>Flyg till Wien från Sverige</h2><p>Wien är en stad för kultur- och matälskare. Schönbrunn, Stephansdom, operan och de legendariska kaffehusen gör varje besök minnesvärt.</p><h3>Bästa tiden att flyga</h3><p>Vår och höst är perfekt. Julmarknader i december är magiska. Sommaren kan vara varm men priserna stiger.</p><h3>Tips</h3><p>Wiens flygplats har snabbtåg (CAT) till Wien Mitte på bara 16 minuter.</p>",
            IsPublished = true
        },
        new()
        {
            City = "München",
            Country = "Tyskland",
            Slug = "munchen",
            AirportCode = "MUC",
            MetaDescription = "Hitta billiga flyg till München från Sverige. Jämför priser till Bayerns huvudstad – ölkultur, alpnärhet och kultur.",
            Description = "<h2>Flyg till München från Sverige</h2><p>München erbjuder en unik blandning av tradition och modernitet. Marienplatz, Englischer Garten, ölhallar och närhet till alperna gör staden till ett mångsidigt resmål.</p><h3>Bästa tiden att flyga</h3><p>Året runt – Oktoberfest (september) är populärast men dyrast. Vår och tidig sommar erbjuder bra priser och fint väder.</p><h3>Tips</h3><p>München är en perfekt bas för dagsutflykter till Neuschwanstein och de bayerska alperna.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Nice",
            Country = "Frankrike",
            Slug = "nice",
            AirportCode = "NCE",
            MetaDescription = "Billiga flyg till Nice och Franska Rivieran från Sverige. Jämför priser och boka din sol- och kulturresa.",
            Description = "<h2>Flyg till Nice från Sverige</h2><p>Nice är hjärtat av Franska Rivieran. Promenaden vid Baie des Anges, den charmiga gamla stan och närhet till Monaco och Cannes gör det till en drömresa.</p><h3>Bästa tiden att flyga</h3><p>Maj–juni och september erbjuder bäst väder utan sommarens trängsel. Lägsta priserna hittar du under vintern.</p><h3>Tips</h3><p>Nice Côte d'Azur flygplats ligger bara 6 km från centrum – spårvagn linje 2 tar dig dit på 20 minuter.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Oslo",
            Country = "Norge",
            Slug = "oslo",
            AirportCode = "OSL",
            MetaDescription = "Hitta billiga flyg till Oslo från Sverige. Jämför priser till den norska huvudstaden med fjord, museer och natur.",
            Description = "<h2>Flyg till Oslo från Sverige</h2><p>Oslo erbjuder en perfekt kombination av stadskultur och natur. Operahuset, Vigelandsparken och närliggande fjordar gör det till en lockande weekend-destination.</p><h3>Bästa tiden att flyga</h3><p>Sommaren (juni–augusti) har bäst väder. Vintern erbjuder skidmöjligheter nära staden. Billigast att flyga januari–mars.</p><h3>Tips</h3><p>Överväg tåg eller buss som alternativ – ofta billigare och smidigare från Stockholm och Göteborg.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Helsingfors",
            Country = "Finland",
            Slug = "helsingfors",
            AirportCode = "HEL",
            MetaDescription = "Billiga flyg till Helsingfors från Sverige. Jämför priser till Finlands designhuvudstad vid Östersjön.",
            Description = "<h2>Flyg till Helsingfors från Sverige</h2><p>Helsingfors är en kompakt och trivsam stad känd för design, bastu-kultur och havsfront. Sveaborg, Designdistriktet och Salutorget är höjdpunkter.</p><h3>Bästa tiden att flyga</h3><p>Juni–augusti har bäst väder med vita nätter. December erbjuder julstämning. Billigast att flyga januari–mars.</p><h3>Tips</h3><p>Färja från Stockholm är ett populärt alternativ som ger en upplevelse i sig.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Warszawa",
            Country = "Polen",
            Slug = "warszawa",
            AirportCode = "WAW",
            MetaDescription = "Hitta billiga flyg till Warszawa från Sverige. Jämför flygpriser till Polens dynamiska huvudstad.",
            Description = "<h2>Flyg till Warszawa från Sverige</h2><p>Warszawa har genomgått en enorm förvandling och är idag en spännande blandning av historia och modernitet. Den återuppbyggda gamla stan, palatser och ett livligt matscen väntar.</p><h3>Bästa tiden att flyga</h3><p>Maj–september har bäst väder. Vinter kan vara kallt men priserna är låga. Wizz Air erbjuder billiga flygningar.</p><h3>Tips</h3><p>Chopin-flygplatsen ligger bara 10 km från centrum med bra bussförbindelser.</p>",
            IsPublished = true
        },
        new()
        {
            City = "New York",
            Country = "USA",
            Slug = "new-york",
            AirportCode = "JFK",
            MetaDescription = "Billiga flyg till New York från Sverige. Jämför priser till JFK och Newark. Boka din resa till den stad som aldrig sover.",
            Description = "<h2>Flyg till New York från Sverige</h2><p>New York är en av världens mest ikoniska städer. Manhattan, Central Park, Broadway och oändliga matupplevelser gör varje besök oförglömligt.</p><h3>Bästa tiden att flyga</h3><p>Januari–mars erbjuder lägsta priserna (från ca 3 500 kr tur och retur). Vår och höst har bäst väder. SAS har direktflyg från Stockholm.</p><h3>Tips</h3><p>Jämför JFK, Newark (EWR) och även La Guardia (LGA) – prisskillnaden kan vara stor.</p>",
            IsPublished = true
        },
        new()
        {
            City = "Dubai",
            Country = "Förenade Arabemiraten",
            Slug = "dubai",
            AirportCode = "DXB",
            MetaDescription = "Hitta billiga flyg till Dubai från Sverige. Jämför flygpriser och boka din resa till lyxens och solens stad.",
            Description = "<h2>Flyg till Dubai från Sverige</h2><p>Dubai erbjuder en unik mix av lyx, shopping, stränder och modern arkitektur. Burj Khalifa, Dubai Mall och ökensafari är populära upplevelser.</p><h3>Bästa tiden att flyga</h3><p>November–mars har behagligast klimat. Sommaren är extremt varm men billigast att flyga. Emirates har direktflyg från Stockholm.</p><h3>Tips</h3><p>Dubai är även en utmärkt mellanlandningsdestination på väg till Asien eller Australien.</p>",
            IsPublished = true
        },
    ];
}
