using Korga.Server.Ldap.ObjectClasses;
using System;

namespace Korga.Server.Ldap.Internal;

internal class OrganizationalUnitSerializer : ILdapSerializer<OrganizationalUnit>
{
    public string[] Attributes => new[]
    {
        "objectClass",
        "ou",
        "description"
    };

    public OrganizationalUnit Deserialize(AttributeCollection attributes)
    {
        return new OrganizationalUnit(attributes.GetRequiredValue("ou"))
        {
            Description = attributes.GetValue("description")
        };
    }

    public void Serialize(AttributeCollection attributes, OrganizationalUnit entry)
    {
        attributes.Add("objectClass", entry.ObjectClass);
        attributes.Add("ou", entry.Ou);
        attributes.AddIfNotEmpty("description", entry.Description);
    }
}
