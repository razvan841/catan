using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class PlayerInfoRequestDto
    {
        public string Username { get; set; } = string.Empty;
    }
}