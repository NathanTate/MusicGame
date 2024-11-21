using FluentResults;

namespace Application.Errors;
public class ValidationError : Error
{
    public ValidationError(string messagw) : base(messagw)
    {
    }
}
