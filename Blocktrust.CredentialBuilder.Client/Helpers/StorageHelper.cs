﻿namespace Blocktrust.CredentialBuilder.Client.Helpers;

using System.Text;
using System.Text.Json;

public static class StorageHelper
{
    // Helper function to convert a JsonDocument to a string
    public static string ToJsonString(this JsonDocument jdoc, bool Indented = false)
    {
        using (var stream = new MemoryStream())
        {
            // TODO Consider adding an Encoder to JsonWriterOptions, in order to avoid extra escaping. See
            // https://learn.microsoft.com/en-US/dotnet/api/system.text.json.jsonwriteroptions.encoder?view=net-7.0
            Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = Indented });
            jdoc.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}