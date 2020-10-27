using Korga.Server.Ldap.ObjectClasses;

namespace Korga.Server.Ldap.Internal
{
    internal class InetOrgPersonSerializer : ILdapSerializer<InetOrgPerson>
    {
        public string[] Attributes => new[] 
        {
            "cn",
            "objectClass",
            "sn",
            "displayName",
            "employeeNumber",
            "givenName",
            "mail",
            "uid",
            "userPassword"
        };

        public InetOrgPerson Deserialize(AttributeCollection attributes)
        {
            return new InetOrgPerson(attributes.GetRequiredValue("cn"), attributes.GetRequiredValue("sn"))
            {
                DisplayName = attributes.GetValue("displayName"),
                EmployeeNumber = attributes.GetValue("employeeNumber"),
                GivenName = attributes.GetValue("givenName"),
                Mail = attributes.GetValue("mail"),
                Uid = attributes.GetValue("uid"),
                UserPassword = attributes.GetValue("userPassword")
            };
        }

        public void Serialize(AttributeCollection attributes, InetOrgPerson entry)
        {
            attributes.Add("cn", entry.Cn);
            attributes.Add("objectClass", entry.ObjectClass);
            attributes.Add("sn", entry.Sn);
            attributes.AddIfNotEmpty("displayName", entry.DisplayName);
            attributes.AddIfNotEmpty("employeeNumber", entry.EmployeeNumber);
            attributes.AddIfNotEmpty("givenName", entry.GivenName);
            attributes.AddIfNotEmpty("mail", entry.Mail);
            attributes.AddIfNotEmpty("uid", entry.Uid);
            attributes.AddIfNotEmpty("userPassword", entry.UserPassword);
        }
    }
}
