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
        /// Active review permission for messages from other users.
        /// </summary>
        Active = 2,
        /// <summary>
        /// Change the distribution lists which this message is assigned to.
        /// </summary>
        Reassign = 4
    }
}
