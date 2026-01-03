using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}