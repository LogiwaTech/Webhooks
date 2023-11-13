using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Core.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, string>
    where TRequest : class, IRequest<string>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;
        public async Task<string> Handle(TRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();
            var context = new ValidationContext<TRequest>(request);
            var errors = _validators
                .Select(x => x.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .GroupBy(
                    x => x.PropertyName,
                    x => x.ErrorMessage,
                    (propertyName, errorMessages) => new
                    {
                        Key = propertyName,
                        Values = errorMessages.Distinct().ToArray()
                    })
                .Select(s => $"PropertyName:{s.Key} ValidationError(s):{string.Join(',', s.Values)}");
            if (errors.Any())
                return $"Error - ValidationError - {string.Join(Environment.NewLine, errors)}";
            return await next();
        }
    }
}
