using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace Korga.Server.Ldap;

public class AttributeModificationCollection
{
    private readonly List<DirectoryAttributeModification> modifications = new();

    public void AddIfChanged(string attributeName, string? newValue, string? oldValue)
    {
        if (newValue != oldValue)
        {
            DirectoryAttributeModification modification = new() { Name = attributeName };

            if (newValue == null)
            {
                modification.Operation = DirectoryAttributeOperation.Delete;
            }
            else
            {
                modification.Operation = oldValue == null ? DirectoryAttributeOperation.Add : DirectoryAttributeOperation.Replace;
                modification.Add(newValue);
            }

            modifications.Add(modification);
        }
    }

    public DirectoryAttributeModification[] ToArray()
    {
        return modifications.ToArray();
    }
}
