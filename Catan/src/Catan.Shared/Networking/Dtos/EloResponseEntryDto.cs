using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class EloResponseEntryDto
    {
        public string Username { get; set; } = "";
        public int Elo { get; set; }
    }
}