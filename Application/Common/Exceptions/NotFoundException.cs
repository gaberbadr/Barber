namespace Application.Common.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string entityName, object key) 
            : base($"{entityName} with ID {key} was not found.")
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }
    }
}