using Newtonsoft.Json.Linq;

namespace WebApiTest.Extended;

// Validates the schema features present in this Swagger snapshot, not a general JSON Schema implementation.
internal static class ResponseSchema
{
    public static void Validate(JToken value, JToken schema, string location = "$", int depth = 0)
    {
        Assert.True(depth < 100, "Schema nesting exceeds supported depth.");
        schema = ApiSpecification.Resolve(schema);
        if (value.Type == JTokenType.Null)
        {
            // Optional CLR properties are commonly nullable although Swagger 2 omits nullable metadata.
            return;
        }
        var type = schema.Value<string>("type");
        var valid = type switch {
            "object" => value is JObject, "array" => value is JArray,
            "string" => value.Type == JTokenType.String, "integer" => value.Type == JTokenType.Integer,
            "number" => value.Type is JTokenType.Integer or JTokenType.Float,
            "boolean" => value.Type == JTokenType.Boolean, null => true, _ => false };
        Assert.True(valid, $"{location}: expected schema type {type}; values omitted.");
        if (schema["enum"] is JArray allowed) Assert.True(allowed.Any(v => JToken.DeepEquals(v, value)), $"{location}: unknown enum value.");
        if (value is JObject obj)
        {
            foreach (var field in schema["required"] ?? new JArray())
                Assert.True(obj[field.ToString()] != null && obj[field.ToString()]!.Type != JTokenType.Null, $"{location}: required {field} missing/null.");
            if (schema["properties"] is JObject properties)
                foreach (var property in properties.Properties())
                    if (obj[property.Name] is JToken child) Validate(child, property.Value, $"{location}.{property.Name}", depth + 1);
            if (schema["additionalProperties"] is JObject extra)
                foreach (var property in obj.Properties().Where(p => schema["properties"]?[p.Name] == null))
                    Validate(property.Value, extra, $"{location}.{property.Name}", depth + 1);
        }
        if (value is JArray array && schema["items"] is JToken itemSchema)
            for (var i = 0; i < array.Count; i++) Validate(array[i], itemSchema, $"{location}[{i}]", depth + 1);
    }
}

public class ResponseSchemaContracts
{
    [Fact, Trait("Suite", "Offline")]
    public void RejectsWrongTypeInsideNestedArrays()
    {
        var schema = JObject.Parse("{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"amount\":{\"type\":\"string\"}}}}");
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ResponseSchema.Validate(JArray.Parse("[{\"amount\":12}]"), schema));
    }
    [Fact, Trait("Suite", "Offline")]
    public void RejectsMissingOrNullRequiredValues()
    {
        var schema = JObject.Parse("{\"type\":\"object\",\"required\":[\"id\"]}");
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ResponseSchema.Validate(JObject.Parse("{}"), schema));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ResponseSchema.Validate(JObject.Parse("{\"id\":null}"), schema));
    }
    [Fact, Trait("Suite", "Offline")]
    public void ResolvesRealZeroHashResponseSchema()
    {
        ResponseSchema.Validate(JObject.Parse("{\"error\":null,\"result\":{\"page\":1,\"total_pages\":0,\"message\":[]}}"),
            JObject.Parse("{\"$ref\":\"#/definitions/ZeroHashAccountsListResponseZeroHashBaseResponse\"}"));
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ResponseSchema.Validate(JObject.Parse("{\"result\":{\"page\":\"wrong\"}}"),
            JObject.Parse("{\"$ref\":\"#/definitions/ZeroHashAccountsListResponseZeroHashBaseResponse\"}")));
    }
}
