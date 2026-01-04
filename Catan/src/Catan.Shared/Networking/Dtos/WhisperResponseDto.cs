using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class WhisperResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = default!;
    }
}