namespace ChurchTools.Model;

public class Response<T>
{
    public Response(T data)
    {
        Data = data;
    }

    public T Data { get; set; }
}
