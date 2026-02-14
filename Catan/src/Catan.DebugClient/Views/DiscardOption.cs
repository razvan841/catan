using System.ComponentModel;
using Catan.Shared.Game;

namespace Catan.DebugClient.Views
{
    public class DiscardOption
    {
        public ResourceType Resource { get; }
        public int Available { get; }
        public int Selected { get; set; }

        public DiscardOption(ResourceType resource, int available)
        {
            Resource = resource;
            Available = available;
            Selected = 0;
        }
    }
}