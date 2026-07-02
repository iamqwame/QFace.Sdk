namespace QimErp.Shared.Common.AppSettings.Contracts;

public interface IStructuredSettingsMapper<TResponse>
{
    bool IsStructuredSettingKey(string key);

    string CategoryForKey(string key);

    Dictionary<string, object> ToSettingsDictionary(TResponse response);

    TResponse ToStructuredResponse(Dictionary<string, string> values);
}
