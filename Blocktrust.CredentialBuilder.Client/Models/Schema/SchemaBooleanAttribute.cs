namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

using Newtonsoft.Json.Linq;

public class SchemaBooleanAttribute : ISchemaAttribute
{
    public string? Description { get; init; }
    public bool? Required { get; init; }
    public bool? DefaultValue { get; init; }

    public SchemaBooleanAttribute(JToken token,  bool? isRequired = null)
    {
        Required = isRequired;
        var descriptionToken = token.SelectToken("description");
        if (descriptionToken is not null && descriptionToken.Type == JTokenType.String)
        {
            Description = descriptionToken.Value<string>();
        }

        var defaultToken = token.SelectToken("default");
        if (defaultToken is not null)
        {
            switch (defaultToken.Type)
            {
                case JTokenType.Boolean:
                    DefaultValue = defaultToken.Value<bool>();
                    break;
                case JTokenType.String:
                    if (bool.TryParse(defaultToken.Value<string>(), out var defaultValue))
                    {
                        DefaultValue = defaultValue;
                    }
                    break;
            }
        }
    }
}