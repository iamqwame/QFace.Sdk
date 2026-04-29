namespace QimErp.Shared.DemoData.Ghana;

/// <summary>
/// 28 real Ghanaian commercial banks. Used to populate the <c>BankingDetails</c> VO
/// on Employee so demo salaries are paid into a recognisable bank.
/// Account numbers generated alongside are random 13-digit sequences (format-conformant
/// for most Ghana banks but NOT real accounts).
/// </summary>
public static class GhanaBanks
{
    public sealed record Bank(string Name, string SwiftCode, string SortCode, string PrimaryBranch);

    public static readonly IReadOnlyList<Bank> All =
    [
        new("Absa Bank Ghana", "BARCGHAC", "030101", "Ridge Towers, Accra"),
        new("Access Bank Ghana", "ABNGGHAC", "200101", "Starlets 91 Road, Accra"),
        new("Agricultural Development Bank", "ADNTGHAC", "080101", "Independence Avenue, Accra"),
        new("Bank of Africa Ghana", "AFRIGHAC", "240101", "C 131/3 Farrar Avenue, Accra"),
        new("CalBank PLC", "CABKGHAC", "180101", "23 Independence Avenue, Accra"),
        new("Consolidated Bank Ghana", "CBGHGHAC", "210101", "Manet Tower 3, Airport City, Accra"),
        new("Ecobank Ghana", "ECOCGHAC", "130101", "Ecobank Ghana Head Office, Accra"),
        new("Fidelity Bank Ghana", "FBLIGHAC", "240201", "Ridge Towers, Accra"),
        new("First Atlantic Bank", "FIRAGHAC", "170101", "Atlantic Place, Stanbic Heights, Accra"),
        new("FBNBank Ghana", "FBNGGHAC", "150101", "12 Airport City, Accra"),
        new("First National Bank Ghana", "FIRNGHAC", "330101", "6th Floor, Accra Financial Centre"),
        new("GCB Bank PLC", "GHCBGHAC", "040101", "High Street, Accra"),
        new("Guaranty Trust Bank Ghana", "GTBIGHAC", "230101", "25A Castle Road, Ridge, Accra"),
        new("National Investment Bank", "NIBGGHAC", "060101", "37 Kwame Nkrumah Avenue, Accra"),
        new("OmniBSIC Bank Ghana", "BSICGHAC", "260101", "Ringway Estates, Accra"),
        new("Prudential Bank", "PUBKGHAC", "190101", "8 John Hammond Road, Ridge, Accra"),
        new("Republic Bank Ghana", "HFCAGHAC", "220101", "Ebankese, No. 35 Sixth Avenue, Accra"),
        new("Société Générale Ghana", "SGSSGHAC", "090101", "Ring Road Central, Accra"),
        new("Stanbic Bank Ghana", "SBICGHAC", "190201", "Stanbic Heights, Airport City, Accra"),
        new("Standard Chartered Bank Ghana", "SCBLGHAC", "020101", "High Street, Accra"),
        new("United Bank for Africa Ghana", "UNAFGHAC", "120101", "Heritage Towers, Accra"),
        new("Universal Merchant Bank", "UMBGGHAC", "100101", "57 Examination Loop, Accra"),
        new("Zenith Bank Ghana", "ZEBLGHAC", "120201", "Zenith Heights, Premier Towers, Accra"),
        new("ARB Apex Bank", "ARBAGHAC", "510101", "5 Sixth Crescent, Asylum Down, Accra"),
        new("National Savings & Credit Bank", "NSCBGHAC", "070101", "Avenida Hotel Annex, Accra"),
        new("Bank of Ghana", "BOGHGHAC", "010101", "1 Thorpe Road, Accra"),
        new("Development Bank Ghana", "DBGHGHAC", "350101", "5 Sixth Avenue Ridge, Accra"),
        new("ADB Bank", "ADBGGHAC", "080201", "Independence Avenue, Accra")
    ];

    /// <summary>Excluding the central bank (BoG) — employees never have salary accounts there.</summary>
    public static readonly IReadOnlyList<Bank> CommercialBanks =
        All.Where(b => b.Name != "Bank of Ghana").ToList();
}
