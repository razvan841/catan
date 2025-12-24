using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Game;

public class Settlement
{
    public Player Owner { get; init; }
    public Vertex Vertex { get; init; }
    public Settlement(Player owner, Vertex vertex)
    {
        Owner = owner;
        Vertex = vertex;
    }
}