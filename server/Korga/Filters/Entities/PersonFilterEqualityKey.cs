namespace Korga.Filters.Entities;

public struct PersonFilterEqualityKey(
    string Discriminator,
    int GroupId = 0,
    int? GroupRoleId = null,
    int GroupTypeId = 0,
    int PersonId = 0,
    int StatusId = 0)
{
    public override readonly string ToString()
    {
        return $"{Discriminator}{GroupId:X8}{GroupRoleId.GetValueOrDefault():X8}{GroupTypeId:X8}{PersonId:X8}{StatusId:X8}";
    }
}
