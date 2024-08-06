namespace Korga.Filters;

public enum Permissions
{
    Permissions_View,

    /// <summary>
    /// Inherits <see cref="Permissions_View"/>.
    /// </summary>
    Permissions_Admin,

    DistributionLists_View,

    /// <summary>
    /// Inherits <see cref="DistributionLists_View"/>.
    /// </summary>
    DistributionLists_Admin,

    ServiceHistory_View,
}
