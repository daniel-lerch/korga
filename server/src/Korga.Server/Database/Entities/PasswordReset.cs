using System;

namespace Korga.Server.Database.Entities
{
    public class PasswordReset
    {
        public PasswordReset(string uid)
        {
            Uid = uid;
        }

        public Guid Token { get; set; }
        public string Uid { get; set; }
        public DateTime Expiry { get; set; }
    }
}
