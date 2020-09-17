namespace Korga.Server.Database.Entities
{
    public class Person : EntityBase
    {
        public Person(string givenName, string familyName)
        {
            GivenName = givenName;
            FamilyName = familyName;
        }

        public int Id { get; set; }

        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }
    }
}
