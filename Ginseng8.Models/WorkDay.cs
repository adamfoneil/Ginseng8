using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Ginseng.Models
{
	public class WorkDay : AppTable
	{
		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[MaxLength(3)]
		public string Abbreviation { get; set; }

		/// <summary>
		/// SQL Server DATEPART(dw...) value
		/// </summary>
		public int Value { get; set; }

		/// <summary>
		/// Bit mask value
		/// </summary>
		public int Flag { get; set; }

		[NotMapped]
		public bool IsSelected { get; set; }

		public static IEnumerable<WorkDay> WorkDays => new WorkDay[]
		{
			new WorkDay() { Name = "Sunday", Abbreviation = "Sun", Flag = 1, Value = 1 },
			new WorkDay() { Name = "Monday", Abbreviation = "Mon", Flag = 2, Value = 2 },
			new WorkDay() { Name = "Tuesday", Abbreviation = "Tue", Flag = 4, Value = 3 },
			new WorkDay() { Name = "Wednesday", Abbreviation = "Wed", Flag = 8, Value = 4 },
			new WorkDay() { Name = "Thursday", Abbreviation = "Thr", Flag = 16, Value = 5 },
			new WorkDay() { Name = "Friday", Abbreviation = "Fri", Flag = 32, Value = 6 },
			new WorkDay() { Name = "Saturday", Abbreviation = "Sat", Flag = 64, Value = 7 }
		};

		public static DataTable GetSeedData()
		{
			return WorkDays.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
		}

		public DayOfWeek ToDayOfWeek()
		{
			return ToDayOfWeek(Value);
		}

		public static DayOfWeek ToDayOfWeek(int value)
		{
			return (DayOfWeek)value - 1;
		}
	}
}