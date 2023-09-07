using System;

namespace Korga.ChurchTools.Entities;

public interface IArchivable
{
    DateTime DeletionTime { get; set; }
}
