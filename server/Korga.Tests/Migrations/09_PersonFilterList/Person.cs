﻿using System;

namespace Korga.Tests.Migrations.PersonFilterList;

public class Person
{
    public int Id { get; set; }

    public int StatusId { get; set; }
    public Status? Status { get; set; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public DateTime DeletionTime { get; set; }
}
