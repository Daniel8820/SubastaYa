namespace SubastaYa.Application.Interfaces.Services
{
    public interface ICommandHandler<in TCommand, TResult>
    {
        Task<TResult> HandleAsync(TCommand command);
    }
    public interface ICommandHandler<in TCommand>
    {
        Task HandleAsync(TCommand command);
    }
}
