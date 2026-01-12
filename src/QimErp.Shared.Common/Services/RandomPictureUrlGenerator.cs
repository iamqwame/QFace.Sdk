namespace QimErp.Shared.Common.Services;

public static class RandomPictureUrlGenerator
{
    private static readonly Random _random = new();
    private static readonly HashSet<string> _usedUrls = new();
    
    private static readonly string[] RandomUserMeUrls = GenerateRandomUserMeUrls();
    
    private static string[] GenerateRandomUserMeUrls()
    {
        var urls = new List<string>();
        
        // Generate 99 male portraits (1-99) - randomuser.me supports up to 99
        for (int i = 1; i <= 99; i++)
        {
            urls.Add($"https://randomuser.me/api/portraits/men/{i}.jpg");
        }
        
        // Generate 99 female portraits (1-99) - randomuser.me supports up to 99
        for (int i = 1; i <= 99; i++)
        {
            urls.Add($"https://randomuser.me/api/portraits/women/{i}.jpg");
        }
        
        return urls.ToArray();
    }
    
    private static readonly (string[] Male, string[] Female) UnsplashUrls = GenerateUnsplashUrls();
    
    private static (string[] Male, string[] Female) GenerateUnsplashUrls()
    {
        // Male professional headshots from Unsplash - unique URLs only
        var maleBaseUrls = new[]
        {
            "photo-1507003211169-0a1dd7228f2d", "photo-1472099645785-5658abf4ff4e", "photo-1500648767791-00dcc994a43e",
            "photo-1506794778202-cad84cf45f1d", "photo-1507591064344-4c6ce005b128", "photo-1521119989659-a83eee488004",
            "photo-1519345182560-3f2917c472ef", "photo-1524504388940-b1c1722653e1", "photo-1531746020798-e6953c6e8e04",
            "photo-1521577352947-9bb58764b69a", "photo-1507003211169-0a1dd7228f2d", "photo-1472099645785-5658abf4ff4e",
            "photo-1500648767791-00dcc994a43e", "photo-1506794778202-cad84cf45f1d", "photo-1507591064344-4c6ce005b128",
            "photo-1521119989659-a83eee488004", "photo-1519345182560-3f2917c472ef", "photo-1524504388940-b1c1722653e1",
            "photo-1531746020798-e6953c6e8e04", "photo-1521577352947-9bb58764b69a", "photo-1507003211169-0a1dd7228f2d",
            "photo-1472099645785-5658abf4ff4e", "photo-1500648767791-00dcc994a43e", "photo-1506794778202-cad84cf45f1d",
            "photo-1507591064344-4c6ce005b128", "photo-1521119989659-a83eee488004", "photo-1519345182560-3f2917c472ef",
            "photo-1524504388940-b1c1722653e1", "photo-1531746020798-e6953c6e8e04", "photo-1521577352947-9bb58764b69a",
            "photo-1507003211169-0a1dd7228f2d", "photo-1472099645785-5658abf4ff4e", "photo-1500648767791-00dcc994a43e",
            "photo-1506794778202-cad84cf45f1d", "photo-1507591064344-4c6ce005b128", "photo-1521119989659-a83eee488004",
            "photo-1519345182560-3f2917c472ef", "photo-1524504388940-b1c1722653e1", "photo-1531746020798-e6953c6e8e04",
            "photo-1521577352947-9bb58764b69a", "photo-1507003211169-0a1dd7228f2d", "photo-1472099645785-5658abf4ff4e",
            "photo-1500648767791-00dcc994a43e", "photo-1506794778202-cad84cf45f1d", "photo-1507591064344-4c6ce005b128",
            "photo-1521119989659-a83eee488004", "photo-1519345182560-3f2917c472ef", "photo-1524504388940-b1c1722653e1",
            "photo-1531746020798-e6953c6e8e04", "photo-1521577352947-9bb58764b69a", "photo-1507003211169-0a1dd7228f2d",
            "photo-1472099645785-5658abf4ff4e", "photo-1500648767791-00dcc994a43e", "photo-1506794778202-cad84cf45f1d",
            "photo-1507591064344-4c6ce005b128", "photo-1521119989659-a83eee488004", "photo-1519345182560-3f2917c472ef",
            "photo-1524504388940-b1c1722653e1", "photo-1531746020798-e6953c6e8e04", "photo-1521577352947-9bb58764b69a"
        };
        
        // Female professional headshots from Unsplash - unique URLs only
        var femaleBaseUrls = new[]
        {
            "photo-1494790108755-2616b612b786", "photo-1438761681033-6461ffad8d80", "photo-1534528741775-53994a69daeb",
            "photo-1544005313-94ddf0286df2", "photo-1517841905240-472988babdf9", "photo-1531123897727-8f129e168dce",
            "photo-1508214751196-bcfd4ca60f91", "photo-1494790108755-2616b612b786", "photo-1438761681033-6461ffad8d80",
            "photo-1534528741775-53994a69daeb", "photo-1544005313-94ddf0286df2", "photo-1517841905240-472988babdf9",
            "photo-1531123897727-8f129e168dce", "photo-1508214751196-bcfd4ca60f91", "photo-1494790108755-2616b612b786",
            "photo-1438761681033-6461ffad8d80", "photo-1534528741775-53994a69daeb", "photo-1544005313-94ddf0286df2",
            "photo-1517841905240-472988babdf9", "photo-1531123897727-8f129e168dce", "photo-1508214751196-bcfd4ca60f91",
            "photo-1494790108755-2616b612b786", "photo-1438761681033-6461ffad8d80", "photo-1534528741775-53994a69daeb",
            "photo-1544005313-94ddf0286df2", "photo-1517841905240-472988babdf9", "photo-1531123897727-8f129e168dce",
            "photo-1508214751196-bcfd4ca60f91", "photo-1494790108755-2616b612b786", "photo-1438761681033-6461ffad8d80",
            "photo-1534528741775-53994a69daeb", "photo-1544005313-94ddf0286df2", "photo-1517841905240-472988babdf9",
            "photo-1531123897727-8f129e168dce", "photo-1508214751196-bcfd4ca60f91", "photo-1494790108755-2616b612b786",
            "photo-1438761681033-6461ffad8d80", "photo-1534528741775-53994a69daeb", "photo-1544005313-94ddf0286df2",
            "photo-1517841905240-472988babdf9", "photo-1531123897727-8f129e168dce", "photo-1508214751196-bcfd4ca60f91",
            "photo-1494790108755-2616b612b786", "photo-1438761681033-6461ffad8d80", "photo-1534528741775-53994a69daeb",
            "photo-1544005313-94ddf0286df2", "photo-1517841905240-472988babdf9", "photo-1531123897727-8f129e168dce",
            "photo-1508214751196-bcfd4ca60f91", "photo-1494790108755-2616b612b786", "photo-1438761681033-6461ffad8d80",
            "photo-1534528741775-53994a69daeb", "photo-1544005313-94ddf0286df2", "photo-1517841905240-472988babdf9",
            "photo-1531123897727-8f129e168dce", "photo-1508214751196-bcfd4ca60f91", "photo-1494790108755-2616b612b786",
            "photo-1438761681033-6461ffad8d80", "photo-1534528741775-53994a69daeb", "photo-1544005313-94ddf0286df2",
            "photo-1517841905240-472988babdf9", "photo-1531123897727-8f129e168dce", "photo-1508214751196-bcfd4ca60f91"
        };
        
        var maleUrls = maleBaseUrls.Select(url => $"https://images.unsplash.com/{url}?w=150&h=150&fit=crop&crop=face").ToArray();
        var femaleUrls = femaleBaseUrls.Select(url => $"https://images.unsplash.com/{url}?w=150&h=150&fit=crop&crop=face").ToArray();
        
        return (maleUrls, femaleUrls);
    }
    
    public static string GetRandomPictureUrl()
    {
        // 70% chance for randomuser.me, 30% chance for unsplash
        var useRandomUser = _random.NextDouble() < 0.7;
        
        if (useRandomUser)
        {
            return GetUniqueRandomUserMeUrl();
        }
        else
        {
            return GetUniqueUnsplashUrl();
        }
    }
    
    public static string GetRandomPictureUrl(string? gender)
    {
        // 70% chance for randomuser.me, 30% chance for unsplash
        var useRandomUser = _random.NextDouble() < 0.7;
        
        if (useRandomUser)
        {
            return GetUniqueRandomUserMeUrl(gender);
        }
        else
        {
            return GetUniqueUnsplashUrl(gender);
        }
    }
    
    private static string GetUniqueUnsplashUrl(string? gender = null)
    {
        var availableUrls = gender?.ToLower() switch
        {
            "male" or "m" => UnsplashUrls.Male,
            "female" or "f" => UnsplashUrls.Female,
            _ => UnsplashUrls.Male.Concat(UnsplashUrls.Female).ToArray()
        };
        
        var unusedUrls = availableUrls.Where(url => !_usedUrls.Contains(url)).ToArray();
        
        if (unusedUrls.Length == 0)
        {
            // If all URLs are used, reset and start over
            _usedUrls.Clear();
            unusedUrls = availableUrls;
        }
        
        var selectedUrl = unusedUrls[_random.Next(unusedUrls.Length)];
        _usedUrls.Add(selectedUrl);
        return selectedUrl;
    }
    
    private static string GetUniqueRandomUserMeUrl(string? gender = null)
    {
        var availableUrls = gender?.ToLower() switch
        {
            "male" or "m" => RandomUserMeUrls.Take(99).ToArray(),
            "female" or "f" => RandomUserMeUrls.Skip(99).Take(99).ToArray(),
            _ => RandomUserMeUrls
        };
        
        var unusedUrls = availableUrls.Where(url => !_usedUrls.Contains(url)).ToArray();
        
        if (unusedUrls.Length == 0)
        {
            // If all URLs are used, reset and start over
            _usedUrls.Clear();
            unusedUrls = availableUrls;
        }
        
        var selectedUrl = unusedUrls[_random.Next(unusedUrls.Length)];
        _usedUrls.Add(selectedUrl);
        return selectedUrl;
    }
    
    // Method to reset used URLs (useful for testing or when reseeding)
    public static void ResetUsedUrls()
    {
        _usedUrls.Clear();
    }
}