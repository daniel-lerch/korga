﻿using System;

namespace Korga.Server.Database.Entities
{
    public class Person
    {
        public int Id { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime DeletionTime { get; set; }
    }
}
