namespace Korga.Server.Ldap;

public interface IObjectClass<T>
{
    static abstract string[] Attributes { get; }
    static abstract T Deserialize(AttributeCollection attributes);
    void Serialize(AttributeCollection attributes);
    void SerializeChanges(AttributeModificationCollection modifications);
    void AcceptNewValues();
}
