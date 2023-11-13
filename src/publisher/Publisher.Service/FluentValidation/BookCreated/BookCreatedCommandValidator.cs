using FluentValidation;
using Publisher.Service.Commands.BookCreated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Service.FluentValidation.BookCreated
{
    public class BookCreatedCommandValidator : AbstractValidator<BookCreatedCommand>
    {
        public BookCreatedCommandValidator()
        {
            RuleFor(r => r.BookName).NotNull().NotEmpty().WithMessage("Book name cannot be null or empty.");
            RuleFor(r => r.Author).NotNull().NotEmpty().WithMessage("Author cannot be null or empty");
            RuleFor(r => r.Publisher).NotNull().NotEmpty().WithMessage("Publisher cannot be null or empty");
            RuleFor(r => r.CreatedDateTime).NotEmpty().NotNull().GreaterThan(gt => DateTime.MinValue).LessThan(lt => DateTime.MaxValue).WithMessage("Created DateTime cannot be null or empty");
        }
    }
}
