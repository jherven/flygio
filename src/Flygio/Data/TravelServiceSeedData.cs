using Flygio.Models;

namespace Flygio.Data;

public static class TravelServiceSeedData
{
    public static List<(TravelService Service, string[] CategorySlugs)> GetServices() =>
    [
        // ─── Flyg ───
        (new TravelService
        {
            Name = "Skyscanner", Slug = "skyscanner",
            Description = "En av världens största flygsökmotorer med priser från hundratals flygbolag och resebyråer.",
            LongDescription = "Skyscanner är en av de mest populära sökmotorerna för flygbiljetter i världen. Tjänsten jämför priser från hundratals flygbolag och onlineresebyråer för att hjälpa dig hitta den billigaste biljetten. Med funktioner som \"Hela månaden\" och \"Billigaste månaden\" kan du enkelt hitta de mest prisvärda tiderna att flyga. Skyscanner erbjuder också sökning efter hotell och hyrbil.",
            WebsiteUrl = "https://www.skyscanner.se", Rating = 9.2m, IsFeatured = true, IsPopular = true,
            Pros = "[\"Mycket brett utbud av flygbolag\",\"Flexibel datumssökning\",\"Prisvarningar\",\"Även hotell och hyrbil\"]",
            Cons = "[\"Vidarebefordrar till tredjepartssajter\",\"Priset kan ändras vid bokning\"]"
        }, ["flyg"]),

        (new TravelService
        {
            Name = "Momondo", Slug = "momondo",
            Description = "Flygprisjämförare känd för att ofta hitta de lägsta priserna genom djup sökning.",
            LongDescription = "Momondo är en metasökmotor för flygresor som jämför priser från flygbolag, resebyråer och andra bokningssajter. Tjänsten är känd för sin \"prisinsikt\"-funktion som visar om det aktuella priset är bra eller om det kan löna sig att vänta. Momondo tillhör samma koncern som Kayak (Booking Holdings).",
            WebsiteUrl = "https://www.momondo.se", Rating = 8.8m, IsPopular = true,
            Pros = "[\"Ofta lägsta pris\",\"Bra prisinsikt-funktion\",\"Ren och enkel design\"]",
            Cons = "[\"Begränsad kundservice\",\"Bokar inte direkt\"]"
        }, ["flyg"]),

        (new TravelService
        {
            Name = "Google Flights", Slug = "google-flights",
            Description = "Googles egna flygssökmotor med snabb sökning och smarta funktioner.",
            LongDescription = "Google Flights är Googles inbyggda flygssöktjänst som erbjuder snabb och kraftfull sökning bland flygbolag världen över. Med funktioner som priskarta, flexibla datum och prishistorik är det ett utmärkt verktyg för att planera resor. Google Flights visar också koldioxidutsläpp per flyg.",
            WebsiteUrl = "https://www.google.com/travel/flights", Rating = 9.0m, IsFeatured = true, IsPopular = true,
            Pros = "[\"Extremt snabb sökning\",\"Priskarta\",\"Prishistorik och prognoser\",\"CO2-information\"]",
            Cons = "[\"Visar inte alltid alla lågprisbolag\",\"Inget lojalitetsprogram\"]"
        }, ["flyg"]),

        (new TravelService
        {
            Name = "Kayak", Slug = "kayak",
            Description = "Kraftfull metasökmotor för flyg, hotell och hyrbil med avancerade filter.",
            LongDescription = "Kayak är en stor metasökmotor som jämför priser från hundratals resebokningstjänster. Med funktioner som prisprognoser, flexibla sökningar och \"Explore\"-kartan kan du hitta inspiration och de bästa priserna. Kayak tillhör Booking Holdings och integrerar med flera andra resetjänster.",
            WebsiteUrl = "https://www.kayak.se", Rating = 8.7m, IsPopular = true,
            Pros = "[\"Avancerade filter\",\"Prisprognoser\",\"Explore-funktion\",\"Jämför många källor\"]",
            Cons = "[\"Kan vara överväldigande\",\"Annonser blandat med resultat\"]"
        }, ["flyg", "hotell", "hyrbil"]),

        (new TravelService
        {
            Name = "Kiwi.com", Slug = "kiwi-com",
            Description = "Innovativ tjänst som kombinerar flyg från olika bolag till unika rutter.",
            LongDescription = "Kiwi.com skiljer sig från andra flygssökmotorer genom att kombinera flygningar från olika flygbolag som normalt inte samarbetar. Detta kan ge unika och billiga rutter som du inte hittar någon annanstans. Tjänsten erbjuder också en \"Kiwi Guarantee\" som skyddar dig vid missade anslutningar.",
            WebsiteUrl = "https://www.kiwi.com", Rating = 7.8m,
            Pros = "[\"Unika kombinationsrutter\",\"Kiwi Guarantee-skydd\",\"Ofta lägre priser\"]",
            Cons = "[\"Komplexa bokningar\",\"Separat incheckning vid byte\",\"Kundservice kan vara långsam\"]"
        }, ["flyg"]),

        (new TravelService
        {
            Name = "Norwegian", Slug = "norwegian",
            Description = "Skandinaviens största lågprisflygbolag med bra utbud från Sverige.",
            LongDescription = "Norwegian är ett av Europas ledande lågprisflygbolag med bas i Norge. Från Sverige erbjuder de ett brett nätverk av direktflyg till populära destinationer i Europa och Thailand. Norwegian har ett modernt flygplansflotta och erbjuder olika biljettyper från LowFare till PremiumFlex.",
            WebsiteUrl = "https://www.norwegian.com", Rating = 7.5m,
            Pros = "[\"Många direktflyg från Sverige\",\"Bra grundpris\",\"Modern flotta\",\"CashPoints-program\"]",
            Cons = "[\"Tillägg för bagage och mat\",\"Begränsad flexibilitet på billigaste biljetter\"]"
        }, ["flyg"]),

        (new TravelService
        {
            Name = "SAS", Slug = "sas",
            Description = "Skandinaviens flaggbärare med brett ruttnät och EuroBonus-program.",
            LongDescription = "SAS (Scandinavian Airlines) är Skandinaviens största flygbolag med ett omfattande ruttnät från Sverige till Europa, Nordamerika och Asien. Som medlem i Star Alliance erbjuder SAS sömlösa anslutningar världen över. EuroBonus-programmet ger möjlighet att samla poäng på flyg och partnerköp.",
            WebsiteUrl = "https://www.flysas.com", Rating = 7.8m,
            Pros = "[\"Stort ruttnät\",\"EuroBonus-program\",\"Star Alliance-medlem\",\"Bra service\"]",
            Cons = "[\"Generellt dyrare än lågpris\",\"Ibland krånglig bokning\"]"
        }, ["flyg"]),

        (new TravelService
        {
            Name = "WizzAir", Slug = "wizzair",
            Description = "Ungersk lågprisflygbolag med billiga flyg till Östeuropa och bortom.",
            LongDescription = "Wizz Air är ett europeiskt ultralågprisflygbolag med baser i flera länder. Från Sverige (främst Skavsta) erbjuder de billiga flyg till destinationer i Östeuropa, Medelhavet och Mellanöstern. Wizz Air har en av Europas yngsta och mest bränsleeffektiva flygplansflottor.",
            WebsiteUrl = "https://www.wizzair.com", Rating = 6.8m,
            Pros = "[\"Mycket låga grundpriser\",\"Ung flotta\",\"Wizz Discount Club\"]",
            Cons = "[\"Dyra tillägg\",\"Begränsat handbagage\",\"Ofta perifera flygplatser\"]"
        }, ["flyg"]),

        // ─── Hotell ───
        (new TravelService
        {
            Name = "Booking.com", Slug = "booking-com",
            Description = "Världens största hotellbokningssajt med miljontals boenden i alla prisklasser.",
            LongDescription = "Booking.com är den dominerande plattformen för hotellbokning globalt med över 28 miljoner boendealternativ i 228 länder. Från lyxhotell till vandrarhem och lägenheter hittar du allt här. Genius-lojalitetsprogrammet ger rabatter och förmåner, och många boenden erbjuder gratis avbokning.",
            WebsiteUrl = "https://www.booking.com", Rating = 9.0m, IsFeatured = true, IsPopular = true,
            Pros = "[\"Enormt utbud\",\"Gratis avbokning ofta\",\"Genius-rabatter\",\"Bra filter och kartsökning\"]",
            Cons = "[\"Inte alltid billigast\",\"Priserna exkluderar ibland skatter\"]"
        }, ["hotell"]),

        (new TravelService
        {
            Name = "Hotels.com", Slug = "hotels-com",
            Description = "Populär hotellsajt med \"samla 10 nätter, få 1 gratis\"-program.",
            LongDescription = "Hotels.com är en av de största hotellbokningssajterna med boenden i över 200 länder. Deras unika belöningsprogram ger dig var 10:e natt gratis (baserat på snittpriser). Sajten är del av Expedia Group och erbjuder ett brett utbud med bra prisgaranti.",
            WebsiteUrl = "https://www.hotels.com", Rating = 8.3m,
            Pros = "[\"Samla nätter-program\",\"Bra app\",\"Prisgaranti\",\"Secret Prices för medlemmar\"]",
            Cons = "[\"Belöningsprogrammet har ändrats\",\"Färre filter än Booking.com\"]"
        }, ["hotell"]),

        (new TravelService
        {
            Name = "Agoda", Slug = "agoda",
            Description = "Ledande i Asien med ofta lägst priser på hotell i Thailand, Japan m.fl.",
            LongDescription = "Agoda är en hotellbokningssajt som är särskilt stark i Asien-Stillahavsregionen. Tillhör Booking Holdings (samma koncern som Booking.com) men har ofta egna, konkurrenskraftiga priser. Agoda erbjuder också lägenheter och villor, och har ett PointsMAX-program.",
            WebsiteUrl = "https://www.agoda.com", Rating = 8.0m,
            Pros = "[\"Ofta bäst pris i Asien\",\"Bra mobilapp\",\"Insider Deals\",\"Många betalningsalternativ\"]",
            Cons = "[\"Svårare att navigera\",\"Avbokningsregler kan vara strikta\"]"
        }, ["hotell"]),

        (new TravelService
        {
            Name = "Trivago", Slug = "trivago",
            Description = "Hotellprisjämförare som söker bland hundratals bokningssajter.",
            LongDescription = "Trivago är en metasökmotor för hotell som jämför priser från över 400 bokningssajter. Istället för att boka direkt visar Trivago var du hittar det bästa priset och skickar dig vidare till den bokningssajten. Det gör det enkelt att se om du verkligen får bästa deal.",
            WebsiteUrl = "https://www.trivago.se", Rating = 7.5m,
            Pros = "[\"Jämför många sajter samtidigt\",\"Enkelt gränssnitt\",\"Bra filter\"]",
            Cons = "[\"Bokar inte direkt\",\"Inte alltid uppdaterade priser\"]"
        }, ["hotell"]),

        (new TravelService
        {
            Name = "Hostelworld", Slug = "hostelworld",
            Description = "Världens ledande plattform för vandrarhem och budgetboende.",
            LongDescription = "Hostelworld är den största bokningsplattformen för vandrarhem med över 17 000 hostels i 178 länder. Perfekt för budgetresenärer och backpackers. Sajten har detaljerade recensioner, foton och rating-system specifikt för vandrarhemskvaliteter som atmosfär, renlighet och läge.",
            WebsiteUrl = "https://www.hostelworld.com", Rating = 7.8m,
            Pros = "[\"Störst utbud av vandrarhem\",\"Detaljerade recensioner\",\"Bra för solobackpackers\"]",
            Cons = "[\"Begränsat till budgetboenden\",\"Bokningsavgift\"]"
        }, ["hotell"]),

        (new TravelService
        {
            Name = "Airbnb", Slug = "airbnb",
            Description = "Världens största plattform för privatuthyrning av lägenheter och unika boenden.",
            LongDescription = "Airbnb har revolutionerat resebranschen genom att göra det möjligt för privatpersoner att hyra ut sina hem till resenärer. Med allt från rum i delade lägenheter till hela villor, slott och trädkojor erbjuder Airbnb unika boenden du inte hittar på traditionella hotellsajter. Plattformen erbjuder också upplevelser (Airbnb Experiences).",
            WebsiteUrl = "https://www.airbnb.se", Rating = 8.5m, IsPopular = true,
            Pros = "[\"Unika boenden\",\"Ofta billigare för grupper\",\"Lokalt boende\",\"Airbnb Experiences\"]",
            Cons = "[\"Varierande kvalitet\",\"Städavgifter\",\"Mindre konsumentskydd\"]"
        }, ["hotell", "aktiviteter"]),

        (new TravelService
        {
            Name = "Expedia", Slug = "expedia",
            Description = "En av världens största onlineresebyråer med flyg, hotell och paket.",
            LongDescription = "Expedia är en av de största onlineresebyråerna i världen och erbjuder flyg, hotell, hyrbil, paketresor och aktiviteter. Genom att boka paket (flyg + hotell) kan du ofta spara pengar. One Key-lojalitetsprogrammet ger förmåner och poäng på alla bokningar.",
            WebsiteUrl = "https://www.expedia.se", Rating = 8.2m, IsPopular = true,
            Pros = "[\"Allt-i-ett-plattform\",\"Bra paketpriser\",\"One Key-program\",\"Prisgaranti\"]",
            Cons = "[\"Kundservice kan vara svår att nå\",\"Inte alltid billigast på enstaka produkter\"]"
        }, ["flyg", "hotell", "hyrbil"]),

        // ─── Hyrbil ───
        (new TravelService
        {
            Name = "Rentalcars.com", Slug = "rentalcars-com",
            Description = "Världens största hyrbilsjämförare med priser från alla stora uthyrare.",
            LongDescription = "Rentalcars.com (del av Booking Holdings) är den ledande jämförelsesajten för hyrbil och söker bland över 900 uthyrningsföretag på 60 000 platser världen över. Med deras \"Full Protection\" försäkring kan du slippa uthyrarens dyra tilläggsförsäkringar.",
            WebsiteUrl = "https://www.rentalcars.com", Rating = 8.5m, IsFeatured = true, IsPopular = true,
            Pros = "[\"Jämför alla stora uthyrare\",\"Full Protection-försäkring\",\"Gratis avbokning\",\"Bra kundservice\"]",
            Cons = "[\"Inte alltid billigast vid direktbokning\",\"Tilläggsförsäkring kostar extra\"]"
        }, ["hyrbil"]),

        (new TravelService
        {
            Name = "AutoEurope", Slug = "autoeurope",
            Description = "Hyrbilsjämförare med över 20 000 uthämtningsplatser och prisgaranti.",
            LongDescription = "AutoEurope har i över 60 år hjälpt resenärer att hitta de bästa hyrbilspriserna. De samarbetar med alla stora uthyrningskedjor och erbjuder prisgaranti - hittar du ett lägre pris matchar de det. AutoEurope har bra kundservice på svenska.",
            WebsiteUrl = "https://www.autoeurope.se", Rating = 8.2m,
            Pros = "[\"Prisgaranti\",\"Svensk kundservice\",\"Lång erfarenhet\",\"Gratis avbokning\"]",
            Cons = "[\"Webbsidan kan vara daterad\",\"Inte alltid lägst pris\"]"
        }, ["hyrbil"]),

        (new TravelService
        {
            Name = "Sixt", Slug = "sixt",
            Description = "Premium hyrbilsuthyrare med modern flotta och bra service.",
            LongDescription = "Sixt är en tysk hyrbilskoncern med verksamhet i över 100 länder. Känd för sin moderna bilflotta med premiumbilar och bra kundservice. Sixt erbjuder även bildelning (Sixt Share) och prenumerationstjänster. Sixt+-appen ger tillgång till hela deras ekosystem.",
            WebsiteUrl = "https://www.sixt.se", Rating = 8.0m,
            Pros = "[\"Modern bilflotta\",\"Premium upplevelse\",\"Bra app\",\"Sixt+-program\"]",
            Cons = "[\"Kan vara dyrare\",\"Försäkringstillägg\"]"
        }, ["hyrbil"]),

        (new TravelService
        {
            Name = "Europcar", Slug = "europcar",
            Description = "En av Europas största biluthyrare med brett utbud och många stationer.",
            LongDescription = "Europcar är en av de ledande biluthyrarna i Europa med kontor i över 140 länder. De erbjuder allt från småbilar till skåpbilar och lyxbilar. Privilege-lojalitetsprogrammet ger rabatter och förmåner vid återkommande hyror.",
            WebsiteUrl = "https://www.europcar.se", Rating = 7.3m,
            Pros = "[\"Stort nätverk i Europa\",\"Bred fordonsflotta\",\"Privilege-program\"]",
            Cons = "[\"Priserna kan vara höga\",\"Tillägg för utrustning\"]"
        }, ["hyrbil"]),

        (new TravelService
        {
            Name = "Hertz", Slug = "hertz",
            Description = "Världens äldsta och en av de största biluthyrningskedjorna.",
            LongDescription = "Hertz är en av världens mest kända biluthyrare med verksamhet i 150 länder. Med Gold Plus Rewards-programmet kan du hoppa över kön och gå direkt till bilen. Hertz erbjuder också premiumbilar genom Hertz Dream Cars-programmet.",
            WebsiteUrl = "https://www.hertz.se", Rating = 7.5m,
            Pros = "[\"Global närvaro\",\"Gold Plus Rewards\",\"Premiumbilar tillgängliga\"]",
            Cons = "[\"Ofta dyrare vid direktbokning\",\"Försäkringsalternativ kan vara förvirrande\"]"
        }, ["hyrbil"]),

        // ─── Reseförsäkring ───
        (new TravelService
        {
            Name = "ERV", Slug = "erv",
            Description = "Europas ledande reseförsäkringsbolag med brett utbud av reseskydd.",
            LongDescription = "ERV (Europäische Reiseversicherung) är ett av Europas största reseförsäkringsbolag med över 100 års erfarenhet. De erbjuder allt från grundläggande avbeställningsskydd till omfattande reseförsäkringar med sjukvård, bagage och ansvarsskydd. ERV har bra rykte för snabb skadehantering.",
            WebsiteUrl = "https://www.erv.se", Rating = 8.5m, IsFeatured = true, IsPopular = true,
            Pros = "[\"Lång erfarenhet\",\"Brett utbud av försäkringar\",\"Bra skadehantering\",\"Tydliga villkor\"]",
            Cons = "[\"Kan vara dyrare än konkurrenter\",\"Begränsad digital upplevelse\"]"
        }, ["reseforsakring"]),

        (new TravelService
        {
            Name = "Europeiska", Slug = "europeiska",
            Description = "Välkänt svenskt reseförsäkringsbolag med enkla och prisvärda försäkringar.",
            LongDescription = "Europeiska ERV är det svenska varumärket för ERV och erbjuder reseförsäkringar anpassade för den svenska marknaden. Med enkla och tydliga produkter som grund- och premiumförsäkring är det lätt att hitta rätt skydd. Många svenska resenärer känner igen varumärket från resebyrån.",
            WebsiteUrl = "https://www.europeiska.se", Rating = 8.2m,
            Pros = "[\"Svenskt bolag\",\"Enkla produkter\",\"Bra kundservice\",\"Prisvärt\"]",
            Cons = "[\"Begränsat utbud jämfört med storbolagen\"]"
        }, ["reseforsakring"]),

        (new TravelService
        {
            Name = "Gouda", Slug = "gouda",
            Description = "Prisvärda reseförsäkringar med bra skydd och enkel onlinebokning.",
            LongDescription = "Gouda Reseförsäkring erbjuder prisvärda reseförsäkringar för alla typer av resor. Med enkel onlinebokning och tydliga villkor är det lätt att teckna rätt försäkring. Gouda erbjuder även årsförsäkringar för dem som reser ofta.",
            WebsiteUrl = "https://www.gouda.se", Rating = 7.8m,
            Pros = "[\"Prisvärt\",\"Enkel bokning\",\"Årsförsäkring tillgänglig\"]",
            Cons = "[\"Begränsad kundservice kvällar/helger\"]"
        }, ["reseforsakring"]),

        (new TravelService
        {
            Name = "IF", Slug = "if-forsakring",
            Description = "Nordens största försäkringsbolag med reseförsäkring som del av hemförsäkringen.",
            LongDescription = "IF Skadeförsäkring är ett av Nordens största försäkringsbolag. Deras hemförsäkring inkluderar ofta grundläggande reseskydd, men de erbjuder även utökade reseförsäkringar för längre resor och specialbehov. Som befintlig IF-kund kan du enkelt lägga till reseskydd.",
            WebsiteUrl = "https://www.if.se", Rating = 7.5m,
            Pros = "[\"Ingår ofta i hemförsäkring\",\"Enkelt för befintliga kunder\",\"Stort bolag med resurser\"]",
            Cons = "[\"Reseskyddet i hemförsäkringen kan vara begränsat\",\"Dyrare separat\"]"
        }, ["reseforsakring"]),

        (new TravelService
        {
            Name = "Hedvig", Slug = "hedvig",
            Description = "Modern digital försäkring med enkel app och snabb skadehantering.",
            LongDescription = "Hedvig är ett svenskt insurtech-bolag som erbjuder försäkringar via sin app. Deras hemförsäkring inkluderar reseskydd och allt hanteras digitalt. Hedvig utmärker sig med sin transparenta prismodell och snabba AI-drivna skadehantering.",
            WebsiteUrl = "https://www.hedvig.com", Rating = 8.0m,
            Pros = "[\"Modern digital upplevelse\",\"Snabb skadehantering via app\",\"Transparent prissättning\"]",
            Cons = "[\"Begränsat till hemförsäkring + reseskydd\",\"Relativt nytt bolag\"]"
        }, ["reseforsakring"]),

        // ─── Paketresor ───
        (new TravelService
        {
            Name = "TUI", Slug = "tui",
            Description = "Världens största researrangör med charter och paketresor från Sverige.",
            LongDescription = "TUI är världens största rese- och turistkoncern med ett brett utbud av paketresor, hotell och upplevelser. Från Sverige erbjuder TUI charter till populära sol- och baddestinationer med egna flygplan (TUI fly). TUI har egna hotellkedjor och erbjuder allt-i-ett-paket.",
            WebsiteUrl = "https://www.tui.se", Rating = 8.0m, IsFeatured = true, IsPopular = true,
            Pros = "[\"Stort utbud av charterresor\",\"Egna flyg och hotell\",\"Bra barnrabatter\",\"Svenska reseledare\"]",
            Cons = "[\"Begränsat till paketresor\",\"Kan vara dyrare än att boka separat\"]"
        }, ["paketresor"]),

        (new TravelService
        {
            Name = "Ving", Slug = "ving",
            Description = "Sveriges mest kända researrangör med sol- och badresor sedan 1956.",
            LongDescription = "Ving är en av Sveriges äldsta och mest kända researrangörer med paketresor sedan 1956. De erbjuder sol- och badresor, stadsresor och skidresor till destinationer i Europa, Asien och Karibien. Ving har egna hotellkoncept som Sunwing och Ocean Beach Club.",
            WebsiteUrl = "https://www.ving.se", Rating = 7.8m, IsPopular = true,
            Pros = "[\"Lång erfarenhet\",\"Egna hotellkoncept\",\"Bra familjeerbjudanden\"]",
            Cons = "[\"Begränsat till paketresor\",\"Priserna kan vara höga i högsäsong\"]"
        }, ["paketresor"]),

        (new TravelService
        {
            Name = "Apollo", Slug = "apollo",
            Description = "Stor nordisk researrangör med brett utbud av charter och paketresor.",
            LongDescription = "Apollo är en av Nordens ledande researrangörer och tillhör DER Touristik Group. De erbjuder charter och paketresor till populära destinationer i Medelhavet, Kanarieöarna, Nordafrika och Asien. Apollo har egna hotellkoncept och erbjuder både familje- och vuxenresor.",
            WebsiteUrl = "https://www.apollo.se", Rating = 7.8m,
            Pros = "[\"Brett destinationsutbud\",\"Egna hotellkoncept\",\"Bra priser\",\"Flexibla avbokningsvillkor\"]",
            Cons = "[\"Hemsidan kan vara långsam\",\"Begränsade flygtider ibland\"]"
        }, ["paketresor"]),

        (new TravelService
        {
            Name = "STS Alpresor", Slug = "sts-alpresor",
            Description = "Sveriges ledande skidresearrangör med resor till Alperna.",
            LongDescription = "STS Alpresor är specialiserade på skidresor till de franska, österrikiska och italienska Alperna. Med lång erfarenhet erbjuder de paketresor med flyg, transfer, boende och liftkort. STS Alpresor har även sommarresor till Alperna med vandring och cykling.",
            WebsiteUrl = "https://www.stsalpresor.se", Rating = 7.5m,
            Pros = "[\"Specialister på skidresor\",\"Allt-i-ett-paket\",\"Bra destinationskunskap\"]",
            Cons = "[\"Begränsat till Alperna\",\"Säsongsbaserat\"]"
        }, ["paketresor"]),

        (new TravelService
        {
            Name = "Fritidsresor", Slug = "fritidsresor",
            Description = "TUI-ägd researrangör med populära charterresor från Sverige.",
            LongDescription = "Fritidsresor är ett TUI-varumärke och en av Sveriges mest kända researrangörer. De erbjuder charterresor till populära sol- och baddestinationer runt Medelhavet, Kanarieöarna och långväga mål. Fritidsresor är kända för sina koncepthotell och familjevänliga upplägg.",
            WebsiteUrl = "https://www.fritidsresor.se", Rating = 7.5m,
            Pros = "[\"Etablerat varumärke\",\"Bra koncepthotell\",\"Familjevänligt\"]",
            Cons = "[\"Begränsat till paketresor\",\"Del av TUI (överlappar)\"]"
        }, ["paketresor"]),

        (new TravelService
        {
            Name = "Ticket", Slug = "ticket",
            Description = "Nordisk researrangör med fokus på flexibla resor och god service.",
            LongDescription = "Ticket är en nordisk researrangör som erbjuder både paketresor och skräddarsydda resor. Med kontor och butiker runt om i Norden erbjuder de personlig service och rådgivning. Ticket har ett brett utbud av destinationer och erbjuder allt från charterflyg till lyxresor.",
            WebsiteUrl = "https://www.ticket.se", Rating = 7.3m,
            Pros = "[\"Personlig service\",\"Fysiska butiker\",\"Flexibla resor\"]",
            Cons = "[\"Kan vara dyrare\",\"Begränsat onlineutbud\"]"
        }, ["paketresor"]),

        // ─── Aktiviteter ───
        (new TravelService
        {
            Name = "GetYourGuide", Slug = "getyourguide",
            Description = "Världens största plattform för turer, aktiviteter och upplevelser.",
            LongDescription = "GetYourGuide är en av världens ledande plattformar för att boka turer, aktiviteter och upplevelser på resmålet. Med över 100 000 aktiviteter i tusentals städer kan du boka allt från guidade stadsvandringar och matupplevelser till äventyrsaktiviteter och inträdesbiljetter till sevärdheter.",
            WebsiteUrl = "https://www.getyourguide.com", Rating = 8.8m, IsFeatured = true, IsPopular = true,
            Pros = "[\"Enormt utbud\",\"Enkel bokning\",\"Gratis avbokning ofta\",\"Verifierade recensioner\"]",
            Cons = "[\"Priserna kan vara högre än att boka direkt\",\"Inte alltid lokala guider\"]"
        }, ["aktiviteter"]),

        (new TravelService
        {
            Name = "Viator", Slug = "viator",
            Description = "Stor aktivitetsplattform (Tripadvisor) med turer och upplevelser världen över.",
            LongDescription = "Viator, som ägs av Tripadvisor, är en av de största plattformarna för att boka aktiviteter och upplevelser på resan. Med miljontals recensioner och ett brett utbud av turer, entréer och upplevelser i över 300 destinationer är Viator ett go-to för resaktiviteter.",
            WebsiteUrl = "https://www.viator.com", Rating = 8.3m,
            Pros = "[\"Kopplat till Tripadvisor-recensioner\",\"Brett utbud\",\"Bra filter\",\"Prisgaranti\"]",
            Cons = "[\"Kan vara dyrare än att boka direkt\",\"Vissa aktiviteter har begränsad info\"]"
        }, ["aktiviteter"]),

        (new TravelService
        {
            Name = "Klook", Slug = "klook",
            Description = "Asien-specialist med aktiviteter, transport och upplevelser.",
            LongDescription = "Klook är en reseteknologiplattform som startade i Hongkong och är särskilt stark i Asien-Stillahavsregionen. De erbjuder aktiviteter, entréer, transport (tåg, SIM-kort, flygplatstransfer) och upplevelser. Klook har ofta bättre priser än att boka på plats i Asien.",
            WebsiteUrl = "https://www.klook.com", Rating = 8.0m,
            Pros = "[\"Bäst i Asien\",\"Ofta lägre pris än på plats\",\"Mobilbiljetter\",\"Snabb bekräftelse\"]",
            Cons = "[\"Begränsat utbud i Europa\",\"Kundservice kan vara långsam\"]"
        }, ["aktiviteter"]),

        (new TravelService
        {
            Name = "Tiqets", Slug = "tiqets",
            Description = "Specialiserad på museumbiljetter och sevärdheter med snabb inpassering.",
            LongDescription = "Tiqets är specialiserade på att sälja inträdesbiljetter till museer, sevärdheter och attraktioner världen över. Deras fokus ligger på att erbjuda skip-the-line-biljetter så att du slipper köa. Tiqets erbjuder mobilbiljetter med QR-kod för smidig inpassering.",
            WebsiteUrl = "https://www.tiqets.com", Rating = 8.2m,
            Pros = "[\"Skip-the-line-biljetter\",\"Mobilbiljetter\",\"Sista-minuten-bokning\",\"Enkel avbokning\"]",
            Cons = "[\"Begränsat till sevärdheter/museer\",\"Ibland dyrare än att köpa på plats\"]"
        }, ["aktiviteter"]),
    ];
}
