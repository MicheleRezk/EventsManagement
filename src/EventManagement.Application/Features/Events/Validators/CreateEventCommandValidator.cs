using EventManagement.Application.Features.Events.Commands;
using FluentValidation;

namespace EventManagement.Application.Features.Events.Validators;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .Must(BeInFuture).WithMessage("Start time must be in the future.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.");

        RuleFor(x => x)
            .Must(x => x.EndTime > x.StartTime)
            .WithMessage("End time must be after start time.");
    }

    private static bool BeInFuture(DateTime startTime)
    {
        return startTime > DateTime.UtcNow;
    }
}
