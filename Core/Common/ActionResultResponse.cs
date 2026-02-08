namespace Core.Common;

public class ActionResultResponse<T> : ActionResponse
{
    public ActionResultResponse(T result) : base(success: true)
    {
        Result = result;
    }

    public ActionResultResponse(Exception error) : base(error)
    {
        
    }

    public T Result { get; set; }
}
