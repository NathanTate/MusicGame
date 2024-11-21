using FluentResults;

namespace Application.Errors;
public class ForbiddenAccessError : Error
{
    public ForbiddenAccessError(string message = "Access Denied") : base(message)
    {   
    }
}
