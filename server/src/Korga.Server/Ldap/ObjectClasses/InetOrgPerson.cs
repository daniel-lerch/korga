namespace Korga.Server.Ldap.ObjectClasses;

public class InetOrgPerson
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
}
