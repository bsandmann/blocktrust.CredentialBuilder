namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

using Newtonsoft.Json.Linq;

public class SchemaIntegerAttribute : ISchemaAttribute
{
    public string? Description { get; init; }
    public EnumSchemaIntegerFormat Format { get; init; }
    public bool? Required { get; init; }
    public int? Minimum { get; init; }
    public int? Maximum { get; init; }
    public HashSet<int>? Enum { get; init; }
    public int? MultipleOf { get; init; }
    public int? DefaultValue { get; init; }

    public SchemaIntegerAttribute(JToken token,  bool? isRequired = null)
    {
        Required = isRequired;
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
            var parseResult = EnumSchemaIntegerFormat.TryParse<EnumSchemaIntegerFormat>(formatString, ignoreCase: true, out var format);
            if (parseResult)
            {
                Format = format;
            }
        }

        var maximumToken = token.SelectToken("maximum");
        if (maximumToken != null)
        {
            switch (maximumToken.Type)
            {
                case JTokenType.Float:
                    Maximum = maximumToken.Value<int>();
                    break;
                case JTokenType.String:
                    if (int.TryParse(maximumToken.Value<string>(), out var maximum))
                    {
                        Maximum = maximum;
                    }

                    break;
            }
        }

        var minimumToken = token.SelectToken("minimum");
        if (minimumToken is not null)
        {
            switch (minimumToken.Type)
            {
                case JTokenType.Float:
                    Minimum = minimumToken.Value<int>();
                    break;
                case JTokenType.String:
                    if (int.TryParse(minimumToken.Value<string>(), out var minimum))
                    {
                        Minimum = minimum;
                    }

                    break;
            }
        }

        var defaultToken = token.SelectToken("default");
        if (defaultToken is not null)
        {
            switch (defaultToken.Type)
            {
                case JTokenType.Float:
                    DefaultValue = defaultToken.Value<int>();
                    break;
                case JTokenType.String:
                    if (int.TryParse(defaultToken.Value<string>(), out var defaultValue))
                    {
                        DefaultValue = defaultValue;
                    }

                    break;
            }
        }

        var multipleOfToken = token.SelectToken("multipleOf");
        if (multipleOfToken is not null)
        {
            switch (multipleOfToken.Type)
            {
                case JTokenType.Float:
                    MultipleOf = multipleOfToken.Value<int>();
                    break;
                case JTokenType.String:
                    if (int.TryParse(multipleOfToken.Value<string>(), out var multipleOf))
                    {
                        MultipleOf = multipleOf;
                    }

                    break;
            }
        }

        var enumToken = token.SelectToken("enum");
        if (enumToken is not null && enumToken.Type == JTokenType.Array)
        {
            Enum = new HashSet<int>();
            foreach (var enumValue in enumToken)
            {
                switch (enumValue.Type)
                {
                    case JTokenType.Float:
                        Enum.Add(enumValue.Value<int>());
                        break;
                    case JTokenType.String:
                        if (int.TryParse(enumValue.Value<string>(), out var parsedEnumValue))
                        {
                            Enum.Add(parsedEnumValue);
                        }

                        break;
                }
            }
        }
    }
}