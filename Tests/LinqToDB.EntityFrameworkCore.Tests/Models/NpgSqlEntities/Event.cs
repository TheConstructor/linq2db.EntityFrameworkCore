﻿using System;
using NpgsqlTypes;

namespace LinqToDB.EntityFrameworkCore.Tests.Models.NpgSqlEntities
{
	public class Event
	{
	    public int Id { get; set; }
	    public string Name { get; set; }
	    public NpgsqlRange<DateTime> Duration { get; set; }
	}
}
