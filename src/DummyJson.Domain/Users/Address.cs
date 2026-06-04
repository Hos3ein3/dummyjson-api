using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Users;

/// <summary>
/// Address value object embedded in User.
/// </summary>
public sealed class Address : ValueObject
{
    private Address() { }

    public Address(
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        double? latitude = null,
        double? longitude = null)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}
