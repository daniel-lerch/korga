namespace Korga.Server.Ldap;

public interface IObjectClass<T>
{
    static abstract string[] Attributes { get; }
    static abstract T Deserialize(AttributeCollection attributes);
    static abstract void Serialize(AttributeCollection attributes, T entry);
}
