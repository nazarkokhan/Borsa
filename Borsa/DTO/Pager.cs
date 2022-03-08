using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Borsa.DTO;

public class Pager<TData>
{
    public Pager(IReadOnlyCollection<TData> data, long totalCount)
    {
        Data = data;
        TotalCount = totalCount;
    }

    [DataMember]
    [JsonPropertyName("data")]
    public IReadOnlyCollection<TData> Data { get; }

    [JsonPropertyName("totalCount")]
    public long TotalCount { get; }
}