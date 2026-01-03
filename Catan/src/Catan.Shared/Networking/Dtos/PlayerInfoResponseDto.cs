using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class PlayerInfoResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Username { get; set; }
        public int Elo { get; set; }
        public string[] Friends { get; set; } = Array.Empty<string>();
    }
}