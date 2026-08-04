using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Exceptions
{
    public class LoadingException(string msg, Exception? innerEx = null) : EngineException(msg, innerEx);
}
