using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Services;

public static class ArticleSeeder
{
    public static async Task SeedAsync(FlygioDbContext db)
    {
        if (await db.Articles.AnyAsync())
            return;

        var articles = GetSeedArticles();
        db.Articles.AddRange(articles);
        await db.SaveChangesAsync();
    }

    private static List<Article> GetSeedArticles() =>
    [
        new()
        {
            Title = "Billiga flyg från Stockholm – Så hittar du bästa priserna",
            Slug = "billiga-flyg-fran-stockholm",
            MetaDescription = "Hitta billiga flygbiljetter från Stockholm Arlanda, Bromma och Skavsta. Tips och tricks för att boka flyg till lägsta pris från Sveriges huvudstad.",
            CategorySlug = "tips",
            IsPublished = true,
            Body = @"<h2>Flyg billigt från Stockholm</h2>
<p>Stockholm är Sveriges största flyghubb med tre flygplatser: <strong>Arlanda (ARN)</strong>, <strong>Bromma (BMA)</strong> och <strong>Skavsta (NYO)</strong>. Tack vare konkurrensen mellan flygbolag och flygplatser finns det goda möjligheter att hitta riktigt bra priser.</p>

<h3>Jämför alla tre flygplatser</h3>
<p>Ett av de bästa tipsen är att söka avgångar från samtliga tre Stockholmsflygplatser. Lågprisbolag som Ryanair och Wizz Air flyger ofta från Skavsta, medan SAS och Norwegian har sina hubbar på Arlanda. Bromma erbjuder framför allt inrikesflyg och vissa europeiska destinationer.</p>

<h3>Boka i rätt tid</h3>
<p>Generellt sett får du bäst priser om du bokar <strong>6–8 veckor</strong> före avresa för europeiska flyg och <strong>2–3 månader</strong> i förväg för långdistansflyg. Undvik att boka för sent – de sista veckorna stiger priserna kraftigt.</p>

<h3>Populära rutter från Stockholm</h3>
<ul>
<li><strong>London</strong> – Från ca 400 kr enkel resa med lågprisbolag</li>
<li><strong>Barcelona</strong> – Populär helårsrutt med priser från 600 kr</li>
<li><strong>Bangkok</strong> – Bäst priser under lågsäsong, från ca 4 000 kr tur och retur</li>
<li><strong>New York</strong> – Direktflyg med SAS, men charterbolag kan vara billigare</li>
</ul>

<h3>Använd prisbevakning</h3>
<p>Med Flygio kan du enkelt jämföra priser från flera flygbolag. Sök på din önskade rutt och datum för att se aktuella priser direkt.</p>"
        },
        new()
        {
            Title = "Bästa tiden att boka flyg – Guide för 2025",
            Slug = "basta-tiden-att-boka-flyg",
            MetaDescription = "När ska man boka flyg för att få bäst pris? Vår guide visar den optimala bokningstiden för inrikes, Europa och långdistansflyg.",
            CategorySlug = "tips",
            IsPublished = true,
            Body = @"<h2>När bör du boka ditt flyg?</h2>
<p>Att boka vid rätt tidpunkt kan spara dig hundratals, ibland tusentals kronor. Men den ""perfekta"" bokningstiden varierar beroende på destination och säsong.</p>

<h3>Inrikesflyg i Sverige</h3>
<p>För inrikesflyg rekommenderar vi att boka <strong>2–4 veckor</strong> innan avresa. Priserna tenderar att vara stabila men hoppar upp kraftigt den sista veckan, särskilt för affärsresenärer.</p>

<h3>Flyg inom Europa</h3>
<p>Den optimala bokningstiden för flygningar inom Europa är <strong>6–10 veckor</strong> före avresa. Under högsäsong (juni–augusti och jul/nyår) bör du boka ännu tidigare – gärna 3–4 månader i förväg.</p>

<h3>Långdistansflyg</h3>
<p>För destinationer utanför Europa, som Thailand, USA eller Japan, gäller <strong>2–5 månader</strong> i förväg. Priserna för långdistans fluktuerar mer, så prisbevakning kan vara extra värdefullt här.</p>

<h3>Vilken veckodag är billigast?</h3>
<p>Det finns en myt om att tisdagar alltid är billigast. I verkligheten varierar det, men <strong>vardagar</strong> (tisdag–torsdag) tenderar att ha lägre priser för avgång jämfört med helger. Det handlar mer om flexibilitet – ju mer flexibel du är med datum, desto bättre priser hittar du.</p>

<h3>Undvik dessa perioder</h3>
<ul>
<li><strong>Sportlov (vecka 7–9)</strong> – Flyg till alperna och solen stiger kraftigt</li>
<li><strong>Midsommar</strong> – Inrikesflyg och Norgeflyg blir dyra</li>
<li><strong>Jul och nyår</strong> – Högsta priserna på året för de flesta destinationer</li>
<li><strong>Skolavslutning/skolstart</strong> – Familjeresor driver upp priserna</li>
</ul>"
        },
        new()
        {
            Title = "Flyga billigt till Thailand från Sverige – Komplett guide",
            Slug = "billiga-flyg-till-thailand",
            MetaDescription = "Allt du behöver veta om att hitta billiga flyg från Sverige till Thailand. Bästa bokningstips, flygbolag och rutter till Bangkok, Phuket och Krabi.",
            CategorySlug = "destinationer",
            IsPublished = true,
            Body = @"<h2>Hitta billiga flyg till Thailand</h2>
<p>Thailand är en av svenskarnas absolut mest populära semesterdestinationer. Varje år reser hundratusentals svenskar till landets stränder och städer. Med rätt strategi kan du hitta riktigt bra flygpriser.</p>

<h3>Bästa flygbolagen</h3>
<p>Från Sverige till Thailand finns flera alternativ:</p>
<ul>
<li><strong>Thai Airways</strong> – Direktflyg Stockholm–Bangkok (ca 11 timmar)</li>
<li><strong>SAS</strong> – Via Köpenhamn till Bangkok</li>
<li><strong>Qatar Airways</strong> – Via Doha, ofta bra priser</li>
<li><strong>Emirates</strong> – Via Dubai med bra komfort</li>
<li><strong>Turkish Airlines</strong> – Via Istanbul, ofta billigast</li>
</ul>

<h3>Bästa tiden att flyga</h3>
<p>Lågsäsong i Thailand (maj–oktober) ger de lägsta flygpriserna. En tur och retur-biljett kan då kosta <strong>från 4 000–5 000 kr</strong>. Under högsäsong (november–mars) ligger priserna normalt kring <strong>6 000–9 000 kr</strong>.</p>

<h3>Bangkok vs. Phuket</h3>
<p>Flyg till <strong>Bangkok (BKK)</strong> är nästan alltid billigare än direkt till <strong>Phuket (HKT)</strong>. Om du vill till södra Thailand kan det löna sig att flyga till Bangkok och ta ett billigt inrikesflyg med AirAsia eller Nok Air vidare till Phuket, Krabi eller Koh Samui.</p>

<h3>Bokningstips</h3>
<ul>
<li>Boka <strong>3–5 månader</strong> före avresa för bäst pris</li>
<li>Var flexibel med datum – en dag hit eller dit kan spara tusenlappar</li>
<li>Överväg mellanlandning – det är ofta billigare och ger en chans att utforska en extra stad</li>
<li>Använd Flygio för att jämföra priser från olika flygbolag</li>
</ul>"
        },
        new()
        {
            Title = "10 tips för att hitta billiga flygbiljetter",
            Slug = "tips-hitta-billiga-flygbiljetter",
            MetaDescription = "Våra 10 bästa tips för att hitta billiga flygbiljetter. Från bokningstidpunkt till flexibilitet – allt du behöver veta för att flyga billigare.",
            CategorySlug = "tips",
            IsPublished = true,
            Body = @"<h2>Så hittar du de billigaste flygbiljetterna</h2>
<p>Att hitta billiga flyg handlar inte om tur – det handlar om strategi. Här är våra 10 bästa tips.</p>

<h3>1. Var flexibel med datum</h3>
<p>Prisskillnaden mellan den dyraste och billigaste dagen kan vara enorm. Att flyga en onsdag istället för en fredag kan spara hundratals kronor.</p>

<h3>2. Jämför alltid priser</h3>
<p>Boka aldrig första bästa flyg. Använd Flygio för att jämföra priser från olika flygbolag och hitta det bästa alternativet.</p>

<h3>3. Boka tidigt – men inte för tidigt</h3>
<p>Den optimala bokningstiden är 6–8 veckor före avresa för Europaflygningar. Bokar du för tidigt betalar du ofta ""grundpris"" utan rabatter.</p>

<h3>4. Undvik högsäsong</h3>
<p>Skollov, helgdagar och storhelger driver upp priserna. Om du kan resa utanför dessa perioder sparar du mycket.</p>

<h3>5. Överväg alternativa flygplatser</h3>
<p>Ibland är det billigare att flyga från en närliggande flygplats. Stockholmare bör kolla Skavsta och Västerås, göteborgare kan titta på Ängelholm.</p>

<h3>6. Boka enkel resa separat</h3>
<p>Ibland är det billigare att boka tur och retur som två enkla resor med olika flygbolag. Det kräver lite mer jobb men kan spara pengar.</p>

<h3>7. Rensa cookies och sök inkognito</h3>
<p>Vissa flygbolag och sökmotorer kan höja priserna om de ser att du sökt samma rutt flera gånger. Sök i privat/inkognito-läge för att undvika detta.</p>

<h3>8. Följ flygbolagens nyhetsbrev</h3>
<p>Många flygbolag skickar ut exklusiva erbjudanden via nyhetsbrev. SAS EuroBonus, Norwegian Reward och Ryanairs nyhetsbrev kan ge tillgång till kampanjpriser.</p>

<h3>9. Var öppen för mellanlandningar</h3>
<p>Direktflyg är bekväma men ofta dyrare. En mellanlandning kan spara dig uppemot 50% på längre resor.</p>

<h3>10. Kontrollera bagagekostnader</h3>
<p>Ett ""billigt"" flygpris kan bli dyrt om du måste betala extra för bagage. Jämför totalpriset inklusive bagage för en rättvis bild.</p>"
        },
        new()
        {
            Title = "Flyg från Göteborg – Bästa destinationerna och priserna",
            Slug = "flyg-fran-goteborg",
            MetaDescription = "Hitta billiga flyg från Göteborg Landvetter. Populära destinationer, bästa flygbolagen och tips för att resa billigt från västkusten.",
            CategorySlug = "destinationer",
            IsPublished = true,
            Body = @"<h2>Flyga från Göteborg Landvetter</h2>
<p><strong>Göteborg Landvetter (GOT)</strong> är Sveriges näst största flygplats och erbjuder ett brett utbud av destinationer i Europa och världen. Från västkusten kan du nå de flesta europeiska storstäder utan mellanlandning.</p>

<h3>Populära destinationer från Göteborg</h3>
<ul>
<li><strong>London</strong> – Ryanair och SAS erbjuder dagliga avgångar, priser från ca 350 kr</li>
<li><strong>Amsterdam</strong> – KLM flyger flera gånger dagligen, bra hub för vidare resor</li>
<li><strong>Barcelona</strong> – Populär semesterrutt med priser från 500 kr</li>
<li><strong>Alicante/Málaga</strong> – Solkusten lockar, särskilt under vinterhalvåret</li>
<li><strong>Köpenhamn</strong> – Kort hop för vidare anslutning, eller weekendresa</li>
</ul>

<h3>Flygbolag på Landvetter</h3>
<p>Landvetter trafikeras av en bra mix av nätverks- och lågprisbolag:</p>
<ul>
<li><strong>SAS</strong> – Störst utbud, hub i Köpenhamn för vidare resor</li>
<li><strong>Norwegian</strong> – Bra priser på populära rutter</li>
<li><strong>Ryanair</strong> – Lägsta basepriserna till flera europeiska städer</li>
<li><strong>Wizz Air</strong> – Billiga flygningar till Östeuropa</li>
<li><strong>KLM</strong> – Amsterdam-anslutning till hela världen</li>
<li><strong>Lufthansa</strong> – Frankfurt/München-hub för globala resor</li>
</ul>

<h3>Tips för Göteborg-resenärer</h3>
<p>Landvetter ligger ca 25 minuter från centrala Göteborg. Flygbussarna är det enklaste sättet att ta sig till flygplatsen. Boka bussresa i förväg för rabatterat pris.</p>
<p>Jämför alltid priser via Flygio – ibland kan det löna sig att ta tåget till Köpenhamn Kastrup för ännu fler avgångar och potentiellt lägre priser.</p>"
        },
        new()
        {
            Title = "Weekendresor med flyg från Sverige – Inspiration och tips",
            Slug = "weekendresor-fran-sverige",
            MetaDescription = "Planera den perfekta weekendresan med flyg från Sverige. Bästa resmålen för en lång helg, priser och praktiska tips för korta flygresor.",
            CategorySlug = "inspiration",
            IsPublished = true,
            Body = @"<h2>Perfekta weekendresor med flyg</h2>
<p>En lång helg i en europeisk storstad är det perfekta sättet att ladda batterierna. Från Sverige når du massor av fantastiska destinationer på bara 1–3 timmars flygtid.</p>

<h3>Bästa weekenddestinationerna</h3>

<h4>Köpenhamn (1h)</h4>
<p>Närmaste storstadsupplevelsen. Strosa i Nyhavn, besök Tivoli och njut av dansk gastronomi. Flyg från ca 300 kr eller ta Öresundståget.</p>

<h4>London (2,5h)</h4>
<p>Oändligt med museer, shopping och matupplevelser. Boka flyg 4–6 veckor i förväg för bästa priserna. Lågprisbolag från Skavsta kan ge flygpriser under 400 kr.</p>

<h4>Berlin (1,5h)</h4>
<p>Kreativ, historisk och prisvärd. Berlin erbjuder allt från världsklass-museer till hippa kvarter. Bra hotellpriser jämfört med andra europeiska huvudstäder.</p>

<h4>Amsterdam (2h)</h4>
<p>Kanaler, konst och kafékultur. Van Gogh-museet, Anne Frank-huset och Jordaan-kvarteret – perfekt för en kulturell weekend.</p>

<h4>Prag (2h)</h4>
<p>En av Europas vackraste städer till låga priser. Öl, mat och boende är avsevärt billigare än i Västeuropa. Flyg från ca 500 kr.</p>

<h4>Barcelona (3h)</h4>
<p>Sol, strand och tapas. Gaudís arkitektur, La Rambla och fantastisk matkultur. Perfekt för en lång weekend, särskilt under vår och höst.</p>

<h3>Tips för weekendresor</h3>
<ul>
<li><strong>Res med handbagage</strong> – Spara pengar och tid på flygplatsen</li>
<li><strong>Välj tidiga avgångar</strong> – Maximera tiden på resmålet</li>
<li><strong>Boka centralt boende</strong> – Tid är pengar på en kort resa</li>
<li><strong>Undvik röda dagar</strong> – Priserna skenar på helgdagar</li>
<li>Jämför priser med Flygio för att hitta billigaste flygalternativet</li>
</ul>"
        }
    ];
}
