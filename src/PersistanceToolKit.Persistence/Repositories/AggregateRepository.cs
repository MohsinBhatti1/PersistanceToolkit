using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Abstractions.Repositories;
using PersistanceToolkit.Domain;
using PersistanceToolKit.Persistence.Persistance;

namespace PersistanceToolkit.Repositories
{
    public class AggregateRepository<T> : EntityRepository<T>, IAggregateRepository<T> where T : Entity, IAggregateRoot
    {
        public AggregateRepository(BaseContext dbContext, ISystemUser systemUser) : base(dbContext, systemUser)
        {
        }
    }
}