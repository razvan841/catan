namespace Catan.Shared.Networking.Dtos.Server
{
    public class FriendRequestAnswerDto
    {
        public string FromUsername { get; set; } = default!; // requester (initiator)
        public string ToUsername { get; set; } = default!; // responder (current user)
        public bool Answer { get; set; }
    }
}