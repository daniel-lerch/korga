using Korga.Server.Ldap.ObjectClasses;
using System;

namespace Korga.Server.Ldap.Internal
{
    internal class OrganizationalUnitSerializer : ILdapSerializer<OrganizationalUnit>
    {
        public OrganizationalUnit Deserialize()
        {
            throw new NotImplementedException();
        }

        public void Serialize(AttributeCollection attributes, OrganizationalUnit entry)
        {
            attributes.Add("objectClass", entry.ObjectClass);
            attributes.Add("ou", entry.Ou);
            attributes.AddIfNotEmpty("description", entry.Description);
        }
    }
}
