
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Repositories;

public interface ISavingsGroupRepository
{
    SavingsGroup Create(SavingsGroup group);

    IEnumerable<SavingsGroup> GetAll();

    SavingsGroup? GetById(Guid id);

    bool Update(SavingsGroup group);

    bool Deactivate(Guid id);
}
