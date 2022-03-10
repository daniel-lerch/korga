using System;

namespace Korga.Server.Models;

[Flags]
public enum RegistrationPeriod
{
    Independent = 0,
    Synchronize = 1,
    OpenWithFirst = 2,
    CloseWithLast = 4
}
