namespace Korga.Server.Ldap.Internal
{
    internal interface ILdapSerializer<T>
    {
        T Deserialize();
        void Serialize(AttributeCollection attributes, T entry);
    }
}
