namespace Blocktrust.CredentialBuilder.Client.Models.Dids;

using global::System.Text.Json.Serialization;
using Services;

public class LocalDid
{
    [JsonPropertyName("did")] public string Did { get; set; }
    [JsonPropertyName("lfDid")] public string LongFormDid { get; set; }
    [JsonPropertyName("p")] public bool IsPublished { get; set; }

    [JsonConstructor]
    public LocalDid()
    {
    }

    public LocalDid(string longFormDid)
    {
        var split = longFormDid.Split(':');
        Did = String.Concat(split[0], ':', split[1], ':', split[2]);
        LongFormDid = longFormDid;
        IsPublished = false;
    }

    public void Published()
    {
        IsPublished = true;
    }
}