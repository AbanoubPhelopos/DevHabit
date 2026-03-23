using FluentValidation;

namespace DevHabit.Application.DTOs.Tags;

public sealed record CreateTagDto(
    string Name,
    string? Description
);

public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3);
        
        RuleFor(x => x.Description)
            .MaximumLength(50);
    }
}
