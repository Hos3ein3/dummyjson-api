using DummyJson.Application.Common.Interfaces;
using DummyJson.Domain.Users;

namespace DummyJson.Application.Common.Repository;

public interface IUserAddressRepository : IMongoRepository<UserAddress>
{
}
