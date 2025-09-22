using FluentResults;

namespace Application.Errors;
public class UnauthorizedAccessError : Error
{
    public UnauthorizedAccessError(string message) : base(message)
    {       
    }
}
