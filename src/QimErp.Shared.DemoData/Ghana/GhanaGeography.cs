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
            ["Greater Accra"] =
            [
                "Accra", "Tema", "Madina", "Kasoa", "Nungua", "Teshie", "La",
                "Dansoman", "Kaneshie", "Lapaz", "Achimota", "Adenta", "Ashongman",
                "Dome", "Kwabenya", "Legon", "Pokuase", "Amasaman", "Nsawam",
                "Oyibi", "Prampram", "Dodowa", "Shai Hills", "Afienya", "Ashaiman",
                "Kpone", "Ningo", "Dawhenya", "Sakumono"
            ],
            ["Ashanti"] =
            [
                "Kumasi", "Obuasi", "Ejisu", "Juaben", "Konongo", "Bekwai",
                "Agona", "Mampong", "Ejura", "Offinso", "Tepa", "Kenyasi",
                "Nkawie", "Toase", "Adum", "Asokwa", "Bantama"
            ],
            ["Northern"] = ["Tamale", "Yendi", "Salaga", "Damongo", "Bole"],
            ["Western"] = ["Sekondi", "Takoradi", "Tarkwa", "Axim", "Half Assini"],
            ["Western North"] = ["Sefwi Wiawso", "Bibiani", "Juaboso", "Akontombra"],
            ["Brong Ahafo"] = ["Sunyani", "Techiman", "Berekum", "Wenchi"],
            ["Ahafo"] = ["Goaso", "Hwidiem", "Kenyasi", "Bechem"],
            ["Bono"] = ["Sunyani", "Berekum", "Dormaa Ahenkro", "Wenchi"],
            ["Bono East"] = ["Techiman", "Kintampo", "Atebubu", "Nkoranza", "Duayaw Nkwanta"],
            ["Central"] = ["Cape Coast", "Winneba", "Saltpond", "Elmina", "Mankessim", "Agona Swedru", "Apam", "Anomabo", "Ajumako", "Kasoa"],
            ["Eastern"] = ["Koforidua", "Oda", "Nkawkaw", "Akim Oda", "Suhum", "Begoro"],
            ["Volta"] = ["Ho", "Hohoe", "Kpando", "Keta", "Aflao", "Sogakope"],
            ["Oti"] = ["Dambai", "Jasikan", "Kadjebi", "Krachi"],
            ["Upper East"] = ["Bolgatanga", "Bawku", "Navrongo"],
            ["Upper West"] = ["Wa", "Lawra", "Tumu", "Jirapa"],
            ["North East"] = ["Nalerigu", "Walewale", "Gambaga"],
            ["Savannah"] = ["Damongo", "Salaga", "Bole", "Sawla"]
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
