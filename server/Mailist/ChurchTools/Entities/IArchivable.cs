using System;

namespace Mailist.ChurchTools.Entities;

public interface IArchivable
{
    DateTime DeletionTime { get; set; }
}
