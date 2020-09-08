using System;

namespace Korga.Server.Database.Entities
{
    public class Message
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the time when this message was received by the Korga server
        /// </summary>
        public DateTime ReceptionTime { get; set; }

        /// <summary>
        /// Gets or sets the time when this message was delivered to its recipients.
        /// </summary>
        public DateTime DeliveryTime { get; set; }

        // TODO: Add properties for state, sender, etc.
    }
}
