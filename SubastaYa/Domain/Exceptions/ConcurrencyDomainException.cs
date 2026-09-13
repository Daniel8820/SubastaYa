namespace SubastaYa.Domain.Exceptions
{
    public class ConcurrencyDomainException : DomainException
    {
        public ConcurrencyDomainException(string message) : base(message) { }
    }
}