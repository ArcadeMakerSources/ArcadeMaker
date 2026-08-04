using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Exceptions
{
    public class NoActivatedRoomException() : EngineException("There is no currently activated room.");
}
