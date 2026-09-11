namespace SubastaYa.Application.Interfaces.Services
{
    public interface IQueryHandler<in TQuery, TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
