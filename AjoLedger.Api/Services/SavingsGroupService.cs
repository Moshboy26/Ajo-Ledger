
using AjoLedger.Api.Models;
using AjoLedger.Api.Repositories;

namespace AjoLedger.Api.Services;

public class SavingsGroupService
{
    private readonly ISavingsGroupRepository _repository;

    public SavingsGroupService(
        ISavingsGroupRepository repository)
    {
        _repository = repository;
    }

    public SavingsGroup Create(
        string name,
        decimal contributionAmount,
        int maxMembers)
    {
        var group = new SavingsGroup
        {
            Name = name.Trim(),
            ContributionAmount = contributionAmount,
            MaxMembers = maxMembers
        };

        return _repository.Create(group);
    }

    public IEnumerable<SavingsGroup> GetAll()
    {
        return _repository.GetAll();
    }

    public SavingsGroup? GetById(Guid id)
    {
        return _repository.GetById(id);
    }

    public bool Update(
        Guid id,
        string name,
        decimal contributionAmount,
        int maxMembers)
    {
        var group = _repository.GetById(id);

        if (group is null)
        {
            return false;
        }

        group.Name = name.Trim();
        group.ContributionAmount = contributionAmount;
        group.MaxMembers = maxMembers;

        return _repository.Update(group);
    }

    public bool Deactivate(Guid id)
    {
        return _repository.Deactivate(id);
    }
}
