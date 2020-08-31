namespace Korga.Server.Database.Entities
{
    public class Account
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }
    }
}
