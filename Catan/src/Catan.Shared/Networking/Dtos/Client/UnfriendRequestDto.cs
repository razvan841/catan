using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos.Client
{
    public class UnfriendRequestDto
    {
        public string TargetUsername { get; set; } = string.Empty;
    }
}