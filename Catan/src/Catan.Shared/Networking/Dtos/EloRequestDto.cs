using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class EloRequestDto
    {
        public string[] Usernames { get; set; } = Array.Empty<string>();
    }
}