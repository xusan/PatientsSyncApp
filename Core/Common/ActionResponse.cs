namespace Core.Common;

public class ActionResponse
{
    public ActionResponse(bool success = true)
    {
        Success = success;
    }

    public ActionResponse(Exception error) : this(false)
    {
        Error = error;
    }

    public bool Success { get; set; }    
    public Exception Error { get; set; }
}

