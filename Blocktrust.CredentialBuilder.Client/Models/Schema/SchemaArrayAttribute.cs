namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

using Newtonsoft.Json.Linq;

public class SchemaArrayAttribute : ISchemaAttribute
{
    public string? Description { get; init; }
    public bool? Required { get; init; }
    public int? MinItems { get; init; }
    public int? MaxItems { get; init; }
    public bool? Unique { get; init; }
    public ISchemaAttribute? ArrayItemRule { get; init; }

    public SchemaArrayAttribute(JToken token,  bool? isRequired = null)
    {
        Required = isRequired;
        var descriptionToken = token.SelectToken("description");
        if (descriptionToken is not null && descriptionToken.Type == JTokenType.String)
        {
            Description = descriptionToken.Value<string>();
        }

        var maxItemsToken = token.SelectToken("maxItems");
        if (maxItemsToken is not null && maxItemsToken.Type == JTokenType.Integer)
        {
            MaxItems = maxItemsToken.Value<int>();
        }

        var minItemsToken = token.SelectToken("minItems");
        if (minItemsToken is not null && minItemsToken.Type == JTokenType.Integer)
        {
            MinItems = minItemsToken.Value<int>();
        }

        var defaultToken = token.SelectToken("uniqueItems");
        if (defaultToken is not null && defaultToken.Type == JTokenType.Boolean)
        {
            Unique = defaultToken.Value<bool>();
        }

        var itemsToken = token.SelectToken("items");
        if (itemsToken is not null && itemsToken.Type == JTokenType.Object)
        {
            var items = itemsToken.ToObject<JObject>();
            var itemsTypeToken = items.SelectToken("type");
            if (itemsTypeToken is not null && itemsTypeToken.Type == JTokenType.String)
            {
                var itemsTypeString = itemsTypeToken.Value<string>();
                if (itemsTypeString is not null)
                {
                    switch (itemsTypeString.ToLowerInvariant())
                    {
                        case "string":
                            ArrayItemRule = new SchemaStringAttribute(items);
                            break;
                        case "integer":
                            ArrayItemRule = new SchemaIntegerAttribute(items);
                            break;
                        case "number":
                            ArrayItemRule = new SchemaNumberAttribute(items);
                            break;
                        case "object":
                            ArrayItemRule = new SchemaObjectAttribute(items);
                            break;
                        case "array":
                            ArrayItemRule = new SchemaArrayAttribute(items);
                            break;
                        case "boolean":
                            ArrayItemRule = new SchemaBooleanAttribute(items);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
     