namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepo userRepo { get; }
        IMessageRepo messageRepo { get; }
        ILikesRepo likesRepo { get; }

        Task<bool> Complete();
        bool HasChanges();

    }
}
