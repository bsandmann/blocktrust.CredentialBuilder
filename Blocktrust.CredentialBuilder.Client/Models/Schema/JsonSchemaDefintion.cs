namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

using Newtonsoft.Json.Linq;

public class JsonSchemaDefintion
{
    public string Id { get; set; }
    public string Schema { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public Dictionary<string, ISchemaAttribute> Attributes { get; set; }
    public bool? AdditionalProperties { get; set; }


    public JsonSchemaDefintion(object jObject)
    {
        if (!jObject.GetType().Equals(typeof(Newtonsoft.Json.Linq.JObject)))
        {
            throw new Exception("Invalid type. Unable to parse schema.");
        }
        else
        {
            var jObjectSchema = (JObject)jObject;
            var idValue = jObjectSchema.GetValue("$id");
            if (idValue is not null && idValue.Type == JTokenType.String)
            {
                Id = idValue.Value<string>()!;
            }

            var schemaValue = jObjectSchema.GetValue("$schema");
            if (schemaValue is not null && schemaValue.Type == JTokenType.String)
            {
                Schema = schemaValue.Value<string>()!;
            }

            var descriptionValue = jObjectSchema.GetValue("description");
            if (descriptionValue is not null && descriptionValue.Type == JTokenType.String)
            {
                Description = descriptionValue.Value<string>()!;
            }

            var typeValue = jObjectSchema.GetValue("type");
            if (typeValue is not null && typeValue.Type == JTokenType.String)
            {
                Type = typeValue.Value<string>()!;
            }

            var properties = jObjectSchema.GetValue("properties");
            if (properties is not null && properties.Type == JTokenType.Object)
            {
                var credentialSubject = properties["credentialSubject"];
                var requiredAttributes = credentialSubject["required"];
                var requiredAttributesArray = new List<string>();
                if (requiredAttributes is not null && requiredAttributes.Type == JTokenType.Array)
                {
                    requiredAttributesArray = requiredAttributes.ToArray().Select(p => p.Value<string>()).ToList();
                }

                var childProperties = credentialSubject["properties"];
                Attributes = new Dictionary<string, ISchemaAttribute>();
                foreach (var childProperty in childProperties)
                {
                    var childPropertyName = childProperty.Path.Split('.').Last();
                    var isRequired = requiredAttributesArray.Contains(childPropertyName);

                    if (childPropertyName.Equals("required"))
                    {
                        // NOTE: The driver licence excample has the required attributes at the child level.
                        // This implementation does not support that.
                        // should not be here, bases on this: https://w3c.github.io/vc-json-schema/#additional-properties
                    }
                    else if (childPropertyName.Equals("additionalProperties"))
                    {
                        var additionalPropertiesValue = childProperty.First.Value<Boolean>();
                        AdditionalProperties = additionalPropertiesValue;
                    }
                    else
                    {
                        var childPropertyType = childProperty.First["type"].Value<string>();
                        if (childPropertyType.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Attributes.Add(childPropertyName, new SchemaStringAttribute(childProperty.First(), isRequired));
                        }
                        else if (childPropertyType.Equals("integer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Attributes.Add(childPropertyName, new SchemaIntegerAttribute(childProperty.First(), isRequired));
                        }
                        else if (childPropertyType.Equals("number", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Attributes.Add(childPropertyName, new SchemaNumberAttribute(childProperty.First(), isRequired));
                        }
                        else if (childPropertyType.Equals("array", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Attributes.Add(childPropertyName, new SchemaArrayAttribute(childProperty.First(), isRequired));
                        }
                        else if (childPropertyType.Equals("boolean", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Attributes.Add(childPropertyName, new SchemaBooleanAttribute(childProperty.First(), isRequired));
                        }
                        else if (childPropertyType.Equals("object", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Attributes.Add(childPropertyName, new SchemaObjectAttribute(childProperty.First(), isRequired));
                        }
                    }
                }
            }
        }
    }
}