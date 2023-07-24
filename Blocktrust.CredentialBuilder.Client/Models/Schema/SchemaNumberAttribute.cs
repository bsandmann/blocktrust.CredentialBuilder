namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

using Newtonsoft.Json.Linq;

public class SchemaNumberAttribute : ISchemaAttribute
{
    public string? Description { get; init; }
    public EnumSchemaIntegerFormat Format { get; init; }
    public bool? Required { get; init; }
    public double? Minimum { get; init; }
    public double? Maximum { get; init; }
    public HashSet<double>? Enum { get; init; }
    public double? MultipleOf { get; init; }
    public double? DefaultValue { get; init; }

    public SchemaNumberAttribute(JToken token,  bool? isRequired = null)
    {
        var descriptionToken = token.SelectToken("description");
        if (descriptionToken is not null && descriptionToken.Type == JTokenType.String)
        {
            Description = descriptionToken.Value<string>();
        }

        var formatToken = token.SelectToken("format");
        if (formatToken is not null && formatToken.Type == JTokenType.String)
        {
            var formatString = formatToken.Value<string>();
            var parseResult = System.Enum.TryParse<EnumSchemaIntegerFormat>(formatString, ignoreCase: true, out var format);
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
                    Maximum = maximumToken.Value<double>();
                    break;
                case JTokenType.String:
                    if (double.TryParse(maximumToken.Value<string>(), out var maximum))
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
                    Minimum = minimumToken.Value<double>();
                    break;
                case JTokenType.String:
                    if (double.TryParse(minimumToken.Value<string>(), out var minimum))
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
                    DefaultValue = defaultToken.Value<double>();
                    break;
                case JTokenType.String:
                    if (double.TryParse(defaultToken.Value<string>(), out var defaultValue))
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
                    MultipleOf = multipleOfToken.Value<double>();
                    break;
                case JTokenType.String:
                    if (double.TryParse(multipleOfToken.Value<string>(), out var multipleOf))
                    {
                        MultipleOf = multipleOf;
                    }

                    break;
            }
        }

        var enumToken = token.SelectToken("enum");
        if (enumToken is not null && enumToken.Type == JTokenType.Array)
        {
            Enum = new HashSet<double>();
            foreach (var enumValue in enumToken)
            {
                switch (enumValue.Type)
                {
                    case JTokenType.Float:
                        Enum.Add(enumValue.Value<double>());
                        break;
                    case JTokenType.String:
                        if (double.TryParse(enumValue.Value<string>(), out var parsedEnumValue))
                        {
                            Enum.Add(parsedEnumValue);
                        }
                        break;
                }
            }
        }
    }
}