using Fcs.Campaign.Application.Abstractions.Authentication;

namespace Fcs.Campaign.CommomTestsUtilities.TestDoubles;

public sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;
    public string? KeycloakUserId { get; set; } = Guid.NewGuid().ToString();
    public IReadOnlyCollection<string> Roles { get; set; } = ["GestorONG"];
}
