namespace Korga.ChurchTools.Api;

public class PaginatedResponse<T>
{
	public PaginatedResponse(T data, PaginatedResponse<T>.ListInformation meta)
	{
		Data = data;
		Meta = meta;
	}

	public T Data { get; set; }
    public ListInformation Meta { get; set; }

    public class ListInformation
    {
		public ListInformation(int count, int all, PaginatedResponse<T>.Pagination pagination)
		{
			Count = count;
			All = all;
			Pagination = pagination;
		}

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
