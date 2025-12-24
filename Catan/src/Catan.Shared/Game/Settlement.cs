using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Game;

public class Settlement
{
    public required Player Owner { get; init; }
    // Possibly store location
    public required Vertex Vertex { get; init; }
}