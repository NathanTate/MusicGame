﻿using FluentResults;

namespace Application.Errors;
public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message)
    {
    }
}
