namespace Korga.Server.Models
{
    public enum ReviewOpinion
    {
        /// <summary>
        /// Reject a message. It will immediately be marked as deleted but can still be recovered.
        /// </summary>
        Reject = 0,
        /// <summary>
        /// Defer a message for an in-depth review. Anyone with review permissions will still be able to approve the message.
        /// </summary>
        Defer = 1,
        /// <summary>
        /// Approve a message. Delivery will start immediately and cannot be aborted.
        /// </summary>
        Approve = 2
    }
}
