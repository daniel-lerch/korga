using Korga.Server.Ldap.ObjectClasses;
using System;

namespace Korga.Server.Ldap.Internal
{
    internal class InetOrgPersonSerializer : ILdapSerializer<InetOrgPerson>
    {
        public InetOrgPerson Deserialize()
        {
            throw new NotImplementedException();
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
