namespace Korga.Server.Database.Entities
{
    public class Person : MutableEntityBase
    {
        public int Id { get; set; }
    }

    public class PersonSnapshot : SnapshotBase
    {
        public PersonSnapshot(string givenName, string familyName)
        {
            GivenName = givenName;
            FamilyName = familyName;
        }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }
    }
}
