using FluentValidation.Results;

namespace Application.Common.Exceptions
{
    public class ValidationException : ApplicationException
    {
        public ValidationException(IEnumerable<ValidationFailure> failures) 
            : base("Validation failed")
        {
            Errors = failures
                .GroupBy(x => x.PropertyName, x => x.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}