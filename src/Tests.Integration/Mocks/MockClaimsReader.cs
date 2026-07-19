namespace Goodtocode.AgentFramework.Tests.Integration.Mocks;

internal sealed class MockClaimsReader : IClaimsReader
{
    public Guid ObjectId { get; set; } = new Guid("b1a7e2c3-4d5f-4e6a-8b9c-1a2b3c4d5e6f");
    public Guid TenantId { get; set; } = new Guid("f6e5d4c3-b2a1-9c8b-7e6d-5c4b3a2b1c0d");
    public string FirstName { get; set; } = "John";
    public string LastName { get; set; } = "Doe";
    public string Email { get; set; } = "john.doe@goodtocode.com";
    public ICollection<string> Scopes { get; set; } = ["read", "write"];
    public ICollection<string> Roles { get; set; } = ["Admin"];
    public ICollection<string> Groups { get; set; } = ["Group1", "Group2"];
}