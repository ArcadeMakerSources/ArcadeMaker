using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Exceptions;

public class EngineException(string message, Exception? innerException = null) : Exception(message, innerException)
{

}