namespace QimErp.Shared.Common.Entities.Helpers;

public class Note
{
    public string? Internal { get; private set; }
    public string? External { get; private set; }

    public static readonly string[] InitialMessages =
    [
        "Thank you for choosing us! We value your trust and confidence.",
        "Your support drives us to deliver even better experiences. Thank you for being part of our journey!",
        "We are grateful for your loyalty and look forward to serving you again.",
        "Your satisfaction is at the heart of what we do. Let us know how we can help you further!",
        "It’s our pleasure to serve you. Thank you for your business and support!",
        "We’re thrilled to have you as a valued customer. Your satisfaction is our priority!",
        "Thank you for giving us the opportunity to serve you. We appreciate your trust in us.",
        "Your feedback and patronage inspire us to grow and improve every day. Thank you!",
        "We appreciate your continued support and look forward to a long-lasting relationship!",
        "Thank you for choosing us. We hope to continue exceeding your expectations!"
    ];


    private static Queue<string> _externalMessages = CreateMessageQueue();

    public static Note Create()
    {
        return new Note();
    }

    public Note WithInternal(string? internalNote)
    {
        Internal = internalNote;
        return this;
    }

    public Note WithExternal(string? externalNote = null)
    {
        External = externalNote ?? GetNextExternalMessage();
        return this;
    }

    private static string GetNextExternalMessage()
    {
        lock (_externalMessages)
        {
            var message = _externalMessages.Dequeue();
            _externalMessages.Enqueue(message);
            return message;
        }
    }

    public static void ResetExternalMessages()
    {
        lock (_externalMessages)
        {
            _externalMessages = CreateMessageQueue();
        }
    }

    private static Queue<string> CreateMessageQueue()
    {
        return new Queue<string>(InitialMessages);
    }
}
