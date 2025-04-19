namespace MiniMart.Application.Contracts
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}
