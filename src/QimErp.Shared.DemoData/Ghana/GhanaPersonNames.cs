namespace QimErp.Shared.DemoData.Ghana;

/// <summary>
/// Curated pools of Ghanaian (Akan day names + common Christian/Muslim names),
/// West African, and international names. Lifted from QimErp.IAM.Seeding.Demo's
/// RandomDataGenerator so demo seeding has a single source of truth.
/// </summary>
public static class GhanaPersonNames
{
    public static readonly IReadOnlyList<string> MaleFirstNames =
    [
        // Ghanaian — Akan day names (Male)
        "Kwame", "Kwasi", "Kwadwo", "Kwabena", "Kwaku", "Yaw", "Kofi",
        "Kojo", "Kwesi", "Ato", "Ebo", "Kobina", "Yoofi", "Fiifi",
        // Ghanaian — common male names
        "Kweku", "Ekow", "Akwasi", "Osei", "Mensah", "Boakye", "Nana",
        "Richmond", "Emmanuel", "Samuel", "Isaac", "Daniel", "Joseph",
        "Elijah", "Benjamin", "Solomon", "Francis", "Peter", "Stephen",
        "Michael", "David", "Christopher", "Joshua", "Patrick", "Anthony",
        "Nicholas", "Andrew", "Matthew", "Mark", "Philip", "Simon",
        "Jonathan", "Timothy", "Dominic", "Eric", "Felix", "Gabriel",
        "Henry", "Vincent", "Martin", "Lawrence", "Augustine", "Clement",
        "Dennis", "Edwin", "Frederick", "George", "Gilbert", "Herbert",
        "Ivan", "Jerome", "Kenneth", "Leonard", "Maurice", "Norman",
        "Oscar", "Paul", "Quentin", "Raymond", "Richard", "Stanley",
        "Thomas", "Victor", "Walter", "Xavier", "Yeboah", "Zachary",
        "Albert", "Bernard", "Charles", "Donald", "Edward", "Frank",
        "Gerald", "Harold", "Ian", "James", "Kevin", "Louis",
        "Maxwell", "Nathan", "Oliver", "Percy", "Ralph", "Sebastian",
        "Theodore", "Ulysses", "Vernon", "William", "Yves", "Aaron",
        // West African — Male
        "Chukwudi", "Uchenna", "Oluwaseun", "Ibrahim", "Mohammed", "Mamadou",
        "Sekou", "Ousmane", "Abdoulaye", "Amadou", "Bakary", "Cheikh",
        "Demba", "Ebrima", "Foday", "Gibril", "Habib", "Ismaila",
        "Jallow", "Karamo", "Lamin", "Modou", "Ndongo", "Omar",
        "Pa", "Salifu", "Tijan", "Yusupha", "Adama", "Boubacar",
        "Drissa", "Fode", "Ibrahima", "Karim", "Malik", "Musa",
        "Osman", "Samba", "Yaya", "Alassane", "Bocar", "Daouda",
        "Elhadji", "Fallou", "Hassan", "Idrissa", "Moussa", "Suleiman",
        // International — Male
        "John", "Robert", "Ryan", "Tyler", "Dylan", "Brandon",
        "Justin", "Kyle", "Sean", "Adam", "Brian",
        "Connor", "Evan", "Jason", "Keith", "Lucas",
        "Marcus", "Neil", "Owen", "Ross", "Scott",
        "Travis", "Aiden", "Blake", "Caleb", "Derek",
        "Ethan", "Gavin", "Hunter", "Jacob", "Landon", "Mason",
        "Noah", "Parker", "Riley", "Shane", "Trevor", "Wesley"
    ];

    public static readonly IReadOnlyList<string> FemaleFirstNames =
    [
        // Ghanaian — Akan day names (Female)
        "Akosua", "Adwoa", "Abenaa", "Akua", "Yaa", "Afua", "Ama",
        "Esi", "Abena", "Efua", "Afia", "Adjoa", "Araba", "Ekua",
        // Ghanaian — common female names
        "Naa", "Fifi", "Mansa", "Serwaa",
        "Grace", "Patience", "Comfort", "Mercy", "Blessing", "Mary",
        "Elizabeth", "Abigail", "Esther", "Ruth", "Joyce", "Janet",
        "Sophia", "Victoria", "Linda", "Juliet", "Francisca", "Lydia",
        "Hannah", "Rachel", "Sarah", "Rebecca", "Deborah", "Martha",
        "Priscilla", "Eugenia", "Florence", "Gloria", "Helena", "Isabella",
        "Jennifer", "Katherine", "Lucy", "Margaret", "Naomi", "Olivia",
        "Patricia", "Rose", "Susanna", "Theresa", "Veronica", "Wilhelmina",
        "Agnes", "Beatrice", "Clara", "Dorothy", "Edith", "Felicia",
        "Gertrude", "Harriet", "Irene", "Josephine", "Kate", "Laura",
        "Matilda", "Nora", "Pamela", "Rita", "Stella", "Tina",
        "Ursula", "Vivian", "Wendy", "Yvonne", "Zoe", "Alice",
        "Barbara", "Christine", "Diana", "Emma", "Frances", "Georgina",
        "Helen", "Ivy", "Joan", "Karen", "Louise", "Michelle",
        "Nancy", "Ophelia", "Penelope", "Regina", "Sandra", "Teresa",
        "Una", "Violet", "Winifred", "Yolanda", "Zelda", "Anastasia",
        // West African — Female
        "Chioma", "Adaeze", "Ngozi", "Aisha", "Fatima", "Aminata", "Mariama",
        "Fatoumata", "Kadiatou", "Hawa", "Isatou", "Binta",
        "Ramata", "Sira", "Oumou", "Ndeye", "Awa", "Khady", "Mariam",
        "Aissatou", "Coumba", "Dieynaba", "Fanta", "Maimouna", "Nafi",
        "Rokia", "Salimata", "Yacine", "Bintou", "Djelika",
        "Fatou", "Kadidia", "Korka", "Maimuna", "Nabou", "Oureye",
        "Penda", "Ramatoulaye", "Safiatou", "Tenin", "Umu", "Wassa",
        "Yama", "Zeynab", "Alima", "Bintu", "Cumba", "Dienaba",
        // International — Female
        "Jane", "Emily", "Jessica", "Ashley", "Amanda", "Melissa",
        "Laura", "Kimberly", "Angela", "Nicole",
        "Donna", "Carol", "Sharon", "Catherine",
        "Lisa",
        "Samantha", "Tiffany", "Vanessa", "Whitney", "Anna", "Brooke",
        "Chloe", "Danielle", "Elena", "Faith", "Gabrielle", "Hailey",
        "Isabel", "Jasmine", "Kelly", "Leah", "Madison", "Natalie",
        "Paige", "Quinn", "Riley", "Taylor"
    ];

    public static readonly IReadOnlyList<string> MaleMiddleNames =
    [
        // Ghanaian male middle names — heavy on Akan day names + common
        "Kwame", "Kofi", "Kwabena", "Yaw", "Kwesi", "Kwaku", "Kojo",
        "Nana", "Mensah", "Osei", "Asante", "Boakye", "Owusu", "Agyei",
        "Emmanuel", "Samuel", "Joseph", "Daniel", "Isaac", "Elijah",
        "Benjamin", "Francis", "Peter", "Stephen", "Philip", "Paul",
        "Timothy", "David", "Michael", "Christopher", "Andrew", "Matthew",
        "Anthony", "Joshua", "Jonathan", "Patrick", "Simon", "Mark",
        "Thomas", "James", "John", "Robert", "George", "Charles",
        "Edward", "Richard", "William", "Henry", "Albert", "Frederick",
        "Yeboah", "Agyeman", "Appiah", "Ofori", "Opoku", "Adomako",
        "Frimpong", "Danso", "Amoah", "Bonsu", "Gyamfi", "Darko",
        // West African male middle
        "Olusegun", "Oluwaseun", "Ibrahim", "Mohammed", "Abdul",
        "Mamadou", "Sekou", "Diallo", "Ousmane", "Abdoulaye", "Amadou",
        "Bakary", "Cheikh", "Demba", "Ebrima", "Foday", "Lamin",
        "Modou", "Omar", "Salifu", "Adama", "Boubacar", "Idrissa",
        // International male middle
        "Alexander", "Lee", "Ray", "Jay", "Wayne", "Roy",
        "Alan", "Brian", "Carl", "Dean", "Earl", "Glen",
        "Keith", "Lance", "Neil", "Owen", "Ross", "Sean",
        "Todd", "Wade", "Blake", "Chase", "Drew", "Grant"
    ];

    public static readonly IReadOnlyList<string> FemaleMiddleNames =
    [
        // Ghanaian female middle
        "Ama", "Akua", "Abena", "Yaa", "Afua", "Akosua", "Adwoa",
        "Naa", "Adjoa", "Esi", "Efua", "Serwaa", "Afia", "Araba",
        "Grace", "Faith", "Hope", "Patience", "Comfort", "Mercy",
        "Ruth", "Esther", "Abigail", "Elizabeth", "Mary", "Joyce",
        "Hannah", "Sarah", "Rebecca", "Deborah", "Martha", "Priscilla",
        "Rose", "Sophia", "Victoria", "Linda", "Juliet", "Lydia",
        "Rachel", "Naomi", "Agnes", "Beatrice", "Clara", "Edith",
        "Helena", "Isabella", "Jennifer", "Katherine", "Lucy", "Margaret",
        "Olivia", "Patricia", "Susanna", "Theresa", "Veronica", "Alice",
        "Christine", "Diana", "Emma", "Frances", "Helen", "Joan",
        // West African female middle
        "Ngozi", "Adaeze", "Fatima", "Aisha", "Aminata", "Mariama",
        "Fatoumata", "Kadiatou", "Hawa", "Isatou", "Binta", "Ramata",
        "Sira", "Oumou", "Ndeye", "Awa", "Khady", "Mariam",
        "Aissatou", "Coumba", "Fanta", "Maimouna", "Rokia", "Salimata",
        // International female middle
        "Marie", "Anne", "Louise", "Jane", "Mae", "Lynn",
        "Ann", "Nicole", "Michelle", "Amanda", "Christine", "Catherine",
        "Claire", "Sophie", "Elaine", "Faye", "Gail", "Holly",
        "Iris", "Joy", "Kate", "Leigh", "Paige", "Quinn"
    ];

    public static readonly IReadOnlyList<string> LastNames =
    [
        // Ghanaian surnames
        "Mensah", "Owusu", "Asante", "Boateng", "Agyeman", "Adjei", "Osei",
        "Frimpong", "Danso", "Amoah", "Opoku", "Acheampong", "Appiah", "Boakye",
        "Ofori", "Adomako", "Yeboah", "Ansah", "Bonsu", "Gyamfi", "Kyei",
        "Darko", "Akoto", "Addai", "Wiredu", "Asiedu", "Antwi", "Sarpong",
        "Amponsah", "Addo", "Awuah", "Baah", "Nyarko", "Oduro", "Ntim",
        "Tetteh", "Quartey", "Lartey", "Nortey", "Okine", "Amarteifio",
        "Sackey", "Hammond", "Nelson", "Ankrah", "Sowah", "Tawiah",
        "Acquah", "Agyapong", "Aidoo", "Amankwah", "Amoako",
        "Aryeetey", "Asare", "Amoateng", "Annan", "Attah", "Awuku",
        "Baidoo", "Buabeng", "Dadzie", "Edusei", "Essel", "Gyan",
        "Kumi", "Manson", "Nyame", "Obeng", "Quansah",
        // West African surnames
        "Adeyemi", "Okafor", "Nwankwo", "Eze", "Okonkwo", "Diallo", "Traore",
        "Kamara", "Sesay", "Koroma", "Bah", "Jalloh", "Conteh", "Fofana",
        "Cisse", "Toure", "Kone", "Coulibaly", "Sanogo", "Bamba",
        "Sangare", "Konate", "Dembele", "Keita", "Diarra", "Doumbia",
        "Sidibe", "Kaboré", "Ouédraogo", "Sawadogo", "Ndiaye", "Diop",
        "Sow", "Fall", "Kane", "Gueye", "Sarr", "Ba",
        "Sy", "Thiam", "Dia", "Faye", "Ndoye", "Seck",
        // International surnames
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Wilson", "Anderson", "Thomas", "Taylor",
        "Moore", "Jackson", "Martin", "Lee", "Thompson", "White", "Harris",
        "Clark", "Lewis", "Robinson", "Walker", "Allen", "King", "Wright",
        "Scott", "Green", "Baker", "Adams", "Hill", "Mitchell",
        "Campbell", "Roberts", "Carter", "Phillips", "Evans", "Turner", "Collins",
        "Edwards", "Stewart", "Morris", "Murphy", "Cook", "Rogers", "Morgan",
        "Peterson", "Cooper", "Reed", "Bailey", "Bell", "Gomez", "Kelly",
        "Howard", "Ward", "Cox", "Diaz", "Richardson", "Wood", "Watson",
        "Brooks", "Bennett", "Gray", "Reyes", "Cruz", "Hughes",
        "Price", "Myers", "Long", "Foster", "Sanders", "Ross", "Morales",
        "Powell", "Sullivan", "Russell", "Ortiz", "Jenkins", "Gutierrez", "Perry",
        "Butler", "Barnes", "Fisher", "Henderson", "Coleman", "Simmons", "Patterson"
    ];
}
