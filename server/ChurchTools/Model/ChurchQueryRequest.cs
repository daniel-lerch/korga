using System.Collections.Generic;
using System.Text.Json;

namespace ChurchTools.Model;

public class ChurchQueryRequest<T> where T : IChurchQueryResult
{
    public ChurchQueryRequest(JsonElement filter)
    {
        Filter = filter;
    }

    public string PrimaryEntityAlias => T.PrimaryEntityAlias;
    public IEnumerable<string> ResponseFields => T.ResponseFields;
    public JsonElement Filter { get; set; }
}

public interface IChurchQueryResult
{
    static abstract string PrimaryEntityAlias { get; }
    static abstract IEnumerable<string> ResponseFields { get; }
}
