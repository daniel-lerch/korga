using System;
using System.Collections.Generic;

namespace ChurchTools.Model;

public class Event
{
    public Event(int id, Guid guid, string name, DateTime startDate, DateTime endDate, List<Service> eventServices)
    {
        Id = id;
        Guid = guid;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        EventServices = eventServices;
    }

    public int Id { get; }
    public Guid Guid { get; }
    public string Name { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public List<Service> EventServices { get; }

    public class Service
    {
        public Service(int id, int? personId, int serviceId, bool agreed)
        {
            Id = id;
            PersonId = personId;
            ServiceId = serviceId;
            Agreed = agreed;
        }

        public int Id { get; }
        public int? PersonId { get; }
        public int ServiceId { get; }
        public bool Agreed { get; }
    }
}
