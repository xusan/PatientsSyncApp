using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApp.AppServices.Common;

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
    public string Msg { get; set; }
    public Exception Error { get; set; }
}

