namespace Korga.Server.Ldap.ObjectClasses;

public class InetOrgPerson : IObjectClass<InetOrgPerson>
{
    private const string objectClass = "inetOrgPerson";

    public InetOrgPerson(string cn, string sn)
    {
        Cn = cn;
        Sn = sn;
    }

    /// <summary>
    /// Gets or sets the common name.
    /// </summary>
    public string Cn { get; set; }

    /// <summary>
    /// Gets or sets the objectClass. You must use "inetOrgPerson" for full feature set.
    /// </summary>
    public string ObjectClass => objectClass;

    /// <summary>
    /// Gets or sets the surname.
    /// </summary>
    public string Sn { get; set; }

    public string? DisplayName { get; set; }

    public string? GivenName { get; set; }

    public string? EmployeeNumber { get; set; }

    /// <summary>
    /// Gets or sets the mail address for use in connected applications.
    /// </summary>
    public string? Mail { get; set; }

    /// <summary>
    /// Gets or sets the login username for connected applications.
    /// </summary>
    public string? Uid { get; set; }

    /// <summary>
    /// Gets or sets the password hash for connected applications.
    /// </summary>
    public string? UserPassword { get; set; }



    #region Serialization
    public static string[] Attributes => new[]
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

    public static InetOrgPerson Deserialize(AttributeCollection attributes)
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

    public static void Serialize(AttributeCollection attributes, InetOrgPerson entry)
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
    #endregion
}
