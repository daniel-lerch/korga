namespace Korga.Server.Ldap.ObjectClasses;

public class OrganizationalUnit : IObjectClass<OrganizationalUnit>
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



    #region Serialization
    public static string[] Attributes => new[]
    {
        "objectClass",
        "ou",
        "description"
    };

    public static OrganizationalUnit Deserialize(AttributeCollection attributes)
    {
        return new OrganizationalUnit(attributes.GetRequiredValue("ou"))
        {
            Description = attributes.GetValue("description")
        };
    }

    public static void Serialize(AttributeCollection attributes, OrganizationalUnit entry)
    {
        attributes.Add("objectClass", entry.ObjectClass);
        attributes.Add("ou", entry.Ou);
        attributes.AddIfNotEmpty("description", entry.Description);
    }
    #endregion
}
