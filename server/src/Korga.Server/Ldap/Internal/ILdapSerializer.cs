namespace Korga.Server.Ldap.Internal;

internal interface ILdapSerializer<T>
{
    string[] Attributes { get; }
    T Deserialize(AttributeCollection attributes);
    void Serialize(AttributeCollection attributes, T entry);
}
