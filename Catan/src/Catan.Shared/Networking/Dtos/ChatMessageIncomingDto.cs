using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Shared.Networking.Dtos
{
    public class ChatMessageIncomingDto
    {
        public string FromUsername { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}