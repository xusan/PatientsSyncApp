using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApp.AppServices.Common;

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
