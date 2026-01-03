using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class EloResponseDto
    {
        public EloResponseEntryDto[] Entries { get; set; } = Array.Empty<EloResponseEntryDto>();
    }
}