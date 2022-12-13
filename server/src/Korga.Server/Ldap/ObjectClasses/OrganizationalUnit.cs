namespace Korga.Server.Ldap.ObjectClasses;

public class OrganizationalUnit : IObjectClass<OrganizationalUnit>
{
    private const string objectClass = "organizationalUnit";

    private string? _ou;
    private string? _description;

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
        OrganizationalUnit result = new(attributes.GetRequiredValue("ou"))
        {
            Description = attributes.GetValue("description")
        };
        ((IObjectClass<OrganizationalUnit>)result).AcceptNewValues();
        return result;
    }

    void IObjectClass<OrganizationalUnit>.Serialize(AttributeCollection attributes)
    {
        attributes.Add("objectClass", ObjectClass);
        attributes.Add("ou", Ou);
        attributes.AddIfNotEmpty("description", Description);
    }

    void IObjectClass<OrganizationalUnit>.SerializeChanges(AttributeModificationCollection modifications)
    {
        modifications.AddIfChanged("ou", Ou, _ou);
        modifications.AddIfChanged("description", Description, _description);
    }

    void IObjectClass<OrganizationalUnit>.AcceptNewValues()
    {
        _ou = Ou;
        _description = Description;
    }
    #endregion
}
