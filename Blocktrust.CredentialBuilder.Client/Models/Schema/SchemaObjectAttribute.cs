namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

using Newtonsoft.Json.Linq;

public class SchemaObjectAttribute  : ISchemaAttribute
{
    public string? Description { get; init; }
    public bool? Required { get; init; }
    public int? MinProperties {get; init; }
    public int? MaxProperties { get; init; }
    public bool? AllowAdditionalProperties { get; init; }

    // This parsing is not complete. It is only implemented to support the current schema.
    // The offical PRISM Manage schema builder is also missing this features.
    
    public SchemaObjectAttribute(JToken token,  bool? isRequired = null)
    {
        Required = isRequired;
        var descriptionToken = token.SelectToken("description");
        if (descriptionToken is not null && descriptionToken.Type == JTokenType.String)
        {
            Description = descriptionToken.Value<string>();
        }

        var minProperties = token.SelectToken("minProperties");
        if (minProperties != null)
        {
            switch (minProperties.Type)
            {
                case JTokenType.Integer:
                    MinProperties = minProperties.Value<int>();
                    break;
                case JTokenType.String:
                    if (int.TryParse(minProperties.Value<string>(), out var minPropertiesParsed))
                    {
                        MinProperties = minPropertiesParsed;
                    }

                    break;
            }
        }

        var maxProperties = token.SelectToken("maxProperties");
        if (maxProperties != null)
        {
            switch (maxProperties.Type)
            {
                case JTokenType.Integer:
                    MaxProperties = maxProperties.Value<int>();
                    break;
                case JTokenType.String:
                    if (int.TryParse(maxProperties.Value<string>(), out var maxPropertiesParsed))
                    {
                        MaxProperties = maxPropertiesParsed;
                    }

                    break;
            }
        }

        var additionalPropertiesToken = token.SelectToken("additionalProperties");
        if (additionalPropertiesToken is not null)
        {
            switch (additionalPropertiesToken.Type)
            {
                case JTokenType.Boolean:
                    AllowAdditionalProperties = additionalPropertiesToken.Value<bool>();
                    break;
                case JTokenType.String:
                    if (bool.TryParse(additionalPropertiesToken.Value<string>(), out var defaultValue))
                    {
                        AllowAdditionalProperties = defaultValue;
                    }

                    break;
            }
        }
    }
}