using ChurchTools.Model;

namespace Mailist.ChurchTools.Entities;

public class DepartmentMember : IIdentifiable<long>
{
    public int PersonId { get; set; }
    public Person? Person { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    long IIdentifiable<long>.Id => (((long)PersonId) << 32) | (long)DepartmentId;
}
