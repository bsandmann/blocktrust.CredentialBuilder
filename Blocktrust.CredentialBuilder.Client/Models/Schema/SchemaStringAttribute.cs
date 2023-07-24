namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

using Newtonsoft.Json.Linq;

public class SchemaStringAttribute : ISchemaAttribute
{
    public string? Description { get; init; }
    public EnumSchemaStringFormat Format { get; init; }
    public bool? Required { get; init; }
    public string? Pattern { get; init; }
    public int? MinLength { get; init; }
    public int? MaxLength { get; init; }
    public HashSet<string>? Enum { get; init; }
    public string? DefaultValue { get; init; }

    public SchemaStringAttribute(JToken token,  bool? isRequired = null)
    {
        Required = isRequired;
        var descriptionToken = token.SelectToken("description");
        if (descriptionToken is not null && descriptionToken.Type == JTokenType.String)
        {
            Description = descriptionToken.Value<string>();
        }

        var formatToken = token.SelectToken("format");
        if (formatToken is not null && formatToken.Type == JTokenType.String)
        {
            var formatString = formatToken.Value<string>();
            var parseResult = System.Enum.TryParse<EnumSchemaStringFormat>(formatString, ignoreCase: true, out var format);
            if (parseResult)
            {
                Format = format;
            }
        }

        var patternToken = token.SelectToken("pattern");
        if (patternToken is not null && patternToken.Type == JTokenType.String)
        {
            Pattern = patternToken.Value<string>();
        }

        var maxLengthToken = token.SelectToken("maxLength");
        if (maxLengthToken is not null && maxLengthToken.Type == JTokenType.Integer)
        {
            MaxLength = maxLengthToken.Value<int>();
        }

        var minLengthToken = token.SelectToken("minLength");
        if (minLengthToken is not null && minLengthToken.Type == JTokenType.Integer)
        {
            MinLength = minLengthToken.Value<int>();
        }

        var defaultToken = token.SelectToken("default");
        if (defaultToken is not null && defaultToken.Type == JTokenType.String)
        {
            DefaultValue = defaultToken.Value<string>();
        }

        var enumToken = token.SelectToken("enum");
        if (enumToken is not null && enumToken.Type == JTokenType.Array)
        {
            Enum = new HashSet<string>();
            foreach (var enumValue in enumToken)
            {
                if (enumValue.Type == JTokenType.String)
                {
                    Enum.Add(enumValue.Value<string>());
                }
            }
        }
    }
}