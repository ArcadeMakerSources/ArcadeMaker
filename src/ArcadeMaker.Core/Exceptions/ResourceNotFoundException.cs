using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Exceptions;

public class ResourceNotFoundException(int ID) : EngineException($"A resource with ID {ID} was not found.")
{
    
}