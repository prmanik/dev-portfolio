using FluentValidation;
using GoalTracker.Mini.DTOs;

public class CreateGoalValitor: AbstractValidator<CreateGoal>
{
    public CreateGoalValitor()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}