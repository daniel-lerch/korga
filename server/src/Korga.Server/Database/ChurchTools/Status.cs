﻿namespace Korga.Server.Database.ChurchTools;

public class Status
{
	public Status(int id, string name)
	{
		Id = id;
		Name = name;
	}

	public int Id { get; set; }
	public string Name { get; set; }
}
