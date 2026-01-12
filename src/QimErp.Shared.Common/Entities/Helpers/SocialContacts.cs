namespace QimErp.Shared.Common.Entities.Helpers;

public class SocialContacts
{
    public string? LinkedIn { get; private set; }
    public string? Twitter { get; private set; }
    public string? Facebook { get; private set; }
    public string? Instagram { get; private set; }
    public string? Skype { get; private set; }
    public string? Pinterest { get; private set; }
    public string? GitHub { get; private set; }
    public string? Website { get; private set; }

    // EF Core requires a parameterless constructor
    private SocialContacts() { }

    private SocialContacts(
        string? linkedIn,
        string? twitter,
        string? facebook,
        string? instagram,
        string? skype,
        string? pinterest,
        string? gitHub,
        string? website)
    {
        LinkedIn = linkedIn;
        Twitter = twitter;
        Facebook = facebook;
        Instagram = instagram;
        Skype = skype;
        Pinterest = pinterest;
        GitHub = gitHub;
        Website = website;
    }

    // Factory method to create a clean instance
    public static SocialContacts Create(
        string? linkedIn = null,
        string? twitter = null,
        string? facebook = null,
        string? instagram = null,
        string? skype = null,
        string? pinterest = null,
        string? gitHub = null,
        string? website = null)
    {
        return new SocialContacts(linkedIn, twitter, facebook, instagram, skype, pinterest, gitHub, website);
    }

    // With-methods for immutability: each returns a new instance
    public SocialContacts WithLinkedIn(string? linkedIn) =>
        new(linkedIn, Twitter, Facebook, Instagram, Skype, Pinterest, GitHub, Website);

    public SocialContacts WithTwitter(string? twitter) =>
        new(LinkedIn, twitter, Facebook, Instagram, Skype, Pinterest, GitHub, Website);

    public SocialContacts WithFacebook(string? facebook) =>
        new(LinkedIn, Twitter, facebook, Instagram, Skype, Pinterest, GitHub, Website);

    public SocialContacts WithInstagram(string? instagram) =>
        new(LinkedIn, Twitter, Facebook, instagram, Skype, Pinterest, GitHub, Website);

    public SocialContacts WithSkype(string? skype) =>
        new(LinkedIn, Twitter, Facebook, Instagram, skype, Pinterest, GitHub, Website);

    public SocialContacts WithPinterest(string? pinterest) =>
        new(LinkedIn, Twitter, Facebook, Instagram, Skype, pinterest, GitHub, Website);

    public SocialContacts WithGitHub(string? gitHub) =>
        new(LinkedIn, Twitter, Facebook, Instagram, Skype, Pinterest, gitHub, Website);

    public SocialContacts WithWebsite(string? website) =>
        new(LinkedIn, Twitter, Facebook, Instagram, Skype, Pinterest, GitHub, website);

   
}
