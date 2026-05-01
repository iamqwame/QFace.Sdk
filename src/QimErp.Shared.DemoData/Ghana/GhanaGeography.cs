namespace QimErp.Shared.DemoData.Ghana;

public static class GhanaGeography
{
    public static readonly IReadOnlyList<string> Regions =
    [
        "Greater Accra", "Ashanti", "Northern", "Western", "Western North",
        "Brong Ahafo", "Ahafo", "Bono", "Bono East", "Central",
        "Eastern", "Volta", "Oti", "Upper East", "Upper West",
        "North East", "Savannah"
    ];

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> CitiesByRegion =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // Greater Accra: capital + every recognised suburb / township so a 250-branch
            // Corporate seed can land unique station names without #N disambiguation.
            ["Greater Accra"] =
            [
                "Accra", "Tema", "Madina", "Kasoa", "Nungua", "Teshie", "La", "Labadi",
                "Dansoman", "Kaneshie", "Lapaz", "Achimota", "Adenta", "Ashongman",
                "Dome", "Kwabenya", "Legon", "East Legon", "West Legon", "American House",
                "Pokuase", "Amasaman", "Nsawam", "Oyibi", "Prampram", "Dodowa",
                "Shai Hills", "Afienya", "Ashaiman", "Kpone", "Ningo", "Dawhenya",
                "Sakumono", "Spintex", "Airport Residential", "Cantonments", "Roman Ridge",
                "Labone", "Osu", "Ridge", "Asylum Down", "Adabraka", "Tudu", "Korle Bu",
                "Korle Gonno", "Mamprobi", "Chorkor", "Jamestown", "Usshertown",
                "Adabraka", "Pig Farm", "Kokomlemle", "Alajo", "Abeka", "Tesano",
                "Dzorwulu", "Abelemkpe", "North Kaneshie", "South Industrial Area",
                "Tema Newtown", "Sakumono Estate", "Community 1", "Community 4",
                "Community 8", "Community 11", "Community 18", "Community 25",
                "Tema Industrial Area", "Michel Camp", "Klagon", "Lashibi"
            ],
            // Ashanti: Kumasi metropolis + every Garden City suburb + secondary towns
            ["Ashanti"] =
            [
                "Kumasi", "Obuasi", "Ejisu", "Juaben", "Konongo", "Bekwai",
                "Agona", "Mampong", "Ejura", "Offinso", "Tepa", "Kenyasi",
                "Nkawie", "Toase", "Adum", "Asokwa", "Bantama", "Suame",
                "Tafo", "Manhyia", "Bomso", "Ahodwo", "Nhyiaeso", "Patasi",
                "Oforikrom", "Asawase", "Ayigya", "Asuoyeboa", "Atonsu", "Ayeduase",
                "Kentinkrono", "Daban", "Tanoso", "Atasemanso", "Edwenase", "Buokrom",
                "New Tafo", "Ahensan", "Sokoban", "Santasi", "Atwima", "Kuntenase",
                "Effiduase", "Asante Akyem", "Amansie", "Akumadan"
            ],
            ["Northern"] =
            [
                "Tamale", "Yendi", "Salaga", "Damongo", "Bole", "Savelugu",
                "Tolon", "Kumbungu", "Karaga", "Gushegu", "Saboba", "Chereponi",
                "Nanton", "Kpandai", "Zabzugu", "Bimbilla"
            ],
            ["Western"] =
            [
                "Sekondi", "Takoradi", "Tarkwa", "Axim", "Half Assini", "Prestea",
                "Bogoso", "Aboso", "Nsuaem", "Apinto", "Anaji", "Effia",
                "Adum Banso", "Mpohor", "Shama", "Inchaban", "New Amanful"
            ],
            ["Western North"] =
            [
                "Sefwi Wiawso", "Bibiani", "Juaboso", "Akontombra", "Sefwi Bekwai",
                "Sefwi Asawinso", "Enchi", "Awaso", "Dadieso", "Asankrangwa"
            ],
            ["Brong Ahafo"] =
            [
                "Sunyani", "Techiman", "Berekum", "Wenchi", "Kintampo", "Drobo",
                "Sampa", "Nkoranza", "Atebubu", "Yeji", "Nsoatre"
            ],
            ["Ahafo"] =
            [
                "Goaso", "Hwidiem", "Kenyasi", "Bechem", "Mim", "Acherensua",
                "Ntotroso", "Kukuom", "Duayaw Nkwanta"
            ],
            ["Bono"] =
            [
                "Sunyani", "Berekum", "Dormaa Ahenkro", "Wenchi", "Drobo",
                "Sampa", "Nsoatre", "Chiraa", "Odumase", "Fiapre"
            ],
            ["Bono East"] =
            [
                "Techiman", "Kintampo", "Atebubu", "Nkoranza", "Duayaw Nkwanta",
                "Yeji", "Prang", "Kwame Danso", "Ejura"
            ],
            ["Central"] =
            [
                "Cape Coast", "Winneba", "Saltpond", "Elmina", "Mankessim",
                "Agona Swedru", "Apam", "Anomabo", "Ajumako", "Kasoa",
                "Komenda", "Twifo Praso", "Assin Foso", "Assin Manso", "Senya Beraku",
                "Awutu", "Bawjiase", "Diaso", "Dunkwa-on-Offin", "Twifo Hemang",
                "Abura Dunkwa", "Breman Asikuma", "Gomoa Afransi", "Nyakrom"
            ],
            ["Eastern"] =
            [
                "Koforidua", "Oda", "Nkawkaw", "Akim Oda", "Suhum", "Begoro",
                "Mpraeso", "Akropong", "Aburi", "Akwapim Mampong", "Larteh",
                "Mamfe", "Nsawam", "Aburi Botanical", "Asamankese", "Akim Tafo",
                "New Abirem", "Akwatia", "Kibi", "Apedwa", "Donkorkrom", "Anyinam"
            ],
            ["Volta"] =
            [
                "Ho", "Hohoe", "Kpando", "Keta", "Aflao", "Sogakope", "Anloga",
                "Akatsi", "Denu", "Adidome", "Akpafu", "Have", "Kete-Krachi",
                "Vakpo", "Peki", "Kpedze", "Anyirawasi", "Adaklu"
            ],
            ["Oti"] =
            [
                "Dambai", "Jasikan", "Kadjebi", "Krachi", "Kete-Krachi",
                "Nkwanta", "Kpassa", "Worawora", "Brewaniase"
            ],
            ["Upper East"] =
            [
                "Bolgatanga", "Bawku", "Navrongo", "Sandema", "Paga",
                "Zebilla", "Garu", "Tempane", "Pusiga", "Binduri", "Bongo",
                "Tongo", "Zuarungu", "Walewale"
            ],
            ["Upper West"] =
            [
                "Wa", "Lawra", "Tumu", "Jirapa", "Nadowli", "Issa",
                "Daffiama", "Funsi", "Hamile", "Wechiau", "Gwollu", "Lambussie"
            ],
            ["North East"] =
            [
                "Nalerigu", "Walewale", "Gambaga", "Yagaba", "Chereponi",
                "Bunkpurugu", "Yunyoo", "Mamprugu Moagduri"
            ],
            ["Savannah"] =
            [
                "Damongo", "Salaga", "Bole", "Sawla", "Buipe", "Daboya",
                "Tolon", "Kpalbe", "Larabanga", "Mole"
            ]
        };

    public static readonly IReadOnlyList<string> Streets =
    [
        // Accra
        "Oxford Street", "Independence Avenue", "Ring Road", "Liberation Road", "Castle Road",
        "High Street", "Cantonments Road", "Osu Road", "Kanda Highway", "Spintex Road",
        "Airport Road", "Achimota Road", "Abossey Okai Road", "Asylum Down Road", "Barnes Road",
        "Derby Avenue", "Dzorwulu Road", "East Legon Road", "Farrar Avenue",
        "Gifford Road", "Graphic Road", "Haatso Road", "John Evans Road",
        "Kanda Road", "Kojo Thompson Road", "Labadi Road", "Labone Road", "Legon Road",
        "Mango Tree Avenue", "Nima Road", "Osu Badu Street", "Pig Farm Road", "Roman Ridge Road",
        "Shiashie Road", "Tetteh Quarshie Interchange", "Tesano Road", "Tudu Road", "West Legon Road",
        // Kumasi
        "Asafo Market Road", "Adum Road", "Harper Road", "Hudson Road", "Kejetia Road",
        "Lake Road", "Manhyia Road", "Nhyiaeso Road", "Oforikrom Road", "Santasi Road",
        "Suame Road", "Tafo Road", "Asokwa Road", "Bantama Road", "Bomso Road",
        // Generic
        "First Circular Road", "Second Circular Road", "Third Circular Road", "Fourth Circular Road",
        "Main Street", "Market Street", "Station Road", "School Lane", "Church Street",
        "Mission Road", "Hospital Road", "Stadium Road",
        "University Road", "College Road", "Technical Road", "Commercial Road",
        "SSNIT Flats", "Dzorwulu Extension", "North Dzorwulu",
        "Atomic Road", "Haatso-Atomic Road"
    ];

    public static readonly IReadOnlyDictionary<string, string> RegionGpsPrefix =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Greater Accra"] = "GA",
            ["Ashanti"] = "AK",
            ["Northern"] = "NM",
            ["Western"] = "WS",
            ["Western North"] = "WN",
            ["Brong Ahafo"] = "BA",
            ["Ahafo"] = "AF",
            ["Bono"] = "BS",
            ["Bono East"] = "BE",
            ["Central"] = "CM",
            ["Eastern"] = "EM",
            ["Volta"] = "VR",
            ["Oti"] = "OR",
            ["Upper East"] = "UE",
            ["Upper West"] = "UW",
            ["North East"] = "NE",
            ["Savannah"] = "SV"
        };
}
