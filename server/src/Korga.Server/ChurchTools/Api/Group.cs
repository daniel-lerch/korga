using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Korga.Server.ChurchTools.Api;

public class Group : IIdentifiable<int>
{
	public Group(int id, Guid guid, string name, Dictionary<string, JsonElement> information)
	{
		Id = id;
		Guid = guid;
		Name = name;
		Information = information;
	}

	public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public Dictionary<string, JsonElement> Information { get; set; }
}
