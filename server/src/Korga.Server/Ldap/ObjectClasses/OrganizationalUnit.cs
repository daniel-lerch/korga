namespace Korga.Server.Ldap.ObjectClasses
{
    public class OrganizationalUnit
    {
        private const string objectClass = "organizationalUnit";

        public OrganizationalUnit(string ou)
        {
            Ou = ou;
        }

        public string ObjectClass => objectClass;

        /// <summary>
        /// Gets or sets the organizational Unit name for this entry
        /// </summary>
        public string Ou { get; set; }

        public string? Description { get; set; }
    }
}
