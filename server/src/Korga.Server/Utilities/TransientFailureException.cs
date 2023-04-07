﻿using System;

namespace Korga.Server.Utilities;

public class TransientFailureException : ApplicationException
{
    public TransientFailureException(string? message) : base(message) { }
    public TransientFailureException(string? message, Exception? innerException) : base(message, innerException) { }
}
