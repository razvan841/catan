using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class HealthResponseDto
    {
        public bool Success { get; set; }
        public DateTime ServerTime  { get; set; }
    }
}