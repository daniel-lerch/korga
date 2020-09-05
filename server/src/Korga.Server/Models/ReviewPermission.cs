using System;

namespace Korga.Server.Models
{
    [Flags]
    public enum ReviewPermission
    {
        /// <summary>
        /// No review permissions. Messages have to be reviewed by someone else.
        /// </summary>
        None = 0,
        /// <summary>
        /// Passive review permissions. Own messages are self-reviewed automatically.
        /// </summary>
        Passive = 1,
        /// <summary>
        /// Active review permission as long as nobody disagrees.
        /// </summary>
        Consensus = 2,
        /// <summary>
        /// Dictatorship. Approve or deny any message permanently.
        /// </summary>
        Overrule = 4
    }
}
