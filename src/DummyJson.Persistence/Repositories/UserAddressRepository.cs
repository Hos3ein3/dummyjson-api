using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Context;

namespace DummyJson.Persistence.Repositories;

public sealed class UserAddressRepository : MongoRepository<UserAddress>, IUserAddressRepository
{
    public UserAddressRepository(MongoDbContext context) : base(context)
    {
    }
}
