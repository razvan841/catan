using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class LeaderboardResponseDto
    {
        public LeaderboardEntryDto[] Entries { get; set; } = Array.Empty<LeaderboardEntryDto>();
    }
}