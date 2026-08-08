using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors
{
    //this behavior will excute before the handler , its like interceptor in angular 
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                // will run all validation rules one by one and returns the validation result
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                // now need to check for any failure
                var failures = validationResults.SelectMany(e => e.Errors).Where(f => f != null).ToList();

                if (failures.Count != 0)
                {
                    //send the failures to custom ValidationException we do have
                    throw new ValidationException(failures);
                }
            }

            // if no validation errors, proceed to the next behavior or handler
            return await next();
        }
    }
}
