namespace Korga.Server.ChurchTools;

public class ChurchToolsResponse<T>
{
    public T Data { get; set; }
    public ListInformation? Meta { get; set; }

    public class ListInformation
    {
        public int Count { get; set; }
        public int All { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Current { get; set; }
        public int LastPage { get; set; }
    }
}
