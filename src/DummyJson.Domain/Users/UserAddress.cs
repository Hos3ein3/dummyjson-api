using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Users;

public sealed class UserAddress : MongoEntity
{
    public Guid UserId { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
