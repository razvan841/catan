using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class WhisperRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}