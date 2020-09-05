using System;

namespace Korga.Server.Models
{
    [Flags]
    public enum ReviewOpinion
    {
        Reject = 0,
        Approve = 1,
        Overrule = 2
    }
}
