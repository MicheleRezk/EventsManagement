using EventManagement.Application.Features.Events.Commands;
using FluentValidation;

namespace EventManagement.Application.Features.Events.Validators;

public class RegisterForEventCommandValidator : AbstractValidator<RegisterForEventCommand>
{
    public RegisterForEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");
    }
}
