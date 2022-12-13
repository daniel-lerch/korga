namespace Korga.Server.Ldap.ObjectClasses;

public class InetOrgPerson : IObjectClass<InetOrgPerson>
{
    private const string objectClass = "inetOrgPerson";

    private string? _cn;
    private string? _sn;
    private string? _displayName;
    private string? _givenName;
    private string? _employeeNumber;
    private string? _mail;
    private string? _uid;
    private string? _userPassword;

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
        InetOrgPerson result = new(attributes.GetRequiredValue("cn"), attributes.GetRequiredValue("sn"))
        {
            DisplayName = attributes.GetValue("displayName"),
            EmployeeNumber = attributes.GetValue("employeeNumber"),
            GivenName = attributes.GetValue("givenName"),
            Mail = attributes.GetValue("mail"),
            Uid = attributes.GetValue("uid"),
            UserPassword = attributes.GetValue("userPassword")
        };
        ((IObjectClass<InetOrgPerson>)result).AcceptNewValues();
        return result;
    }

    void IObjectClass<InetOrgPerson>.Serialize(AttributeCollection attributes)
    {
        attributes.Add("cn", Cn);
        attributes.Add("objectClass", ObjectClass);
        attributes.Add("sn", Sn);
        attributes.AddIfNotEmpty("displayName", DisplayName);
        attributes.AddIfNotEmpty("employeeNumber", EmployeeNumber);
        attributes.AddIfNotEmpty("givenName", GivenName);
        attributes.AddIfNotEmpty("mail", Mail);
        attributes.AddIfNotEmpty("uid", Uid);
        attributes.AddIfNotEmpty("userPassword", UserPassword);
    }

    void IObjectClass<InetOrgPerson>.SerializeChanges(AttributeModificationCollection modifications)
    {
        modifications.AddIfChanged("cn", Cn, _cn);
        modifications.AddIfChanged("sn", Sn, _sn);
        modifications.AddIfChanged("displayName", DisplayName, _displayName);
        modifications.AddIfChanged("employeeNumber", EmployeeNumber, _employeeNumber);
        modifications.AddIfChanged("givenName", GivenName, _givenName);
        modifications.AddIfChanged("mail", Mail, _mail);
        modifications.AddIfChanged("uid", Uid,_uid);
        modifications.AddIfChanged("userPassword", UserPassword, _userPassword);
    }

    void IObjectClass<InetOrgPerson>.AcceptNewValues()
    {
        _cn = Cn;
        _sn = Sn;
        _displayName = DisplayName;
        _employeeNumber = EmployeeNumber;
        _givenName = GivenName;
        _mail = Mail;
        _uid = Uid;
        _userPassword = UserPassword;
    }
    #endregion
}
