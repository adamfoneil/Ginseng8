using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// Collection of model classes (database tables) grouped under a name
	/// </summary>
	[TrackChanges(IgnoreProperties = "DateModified,ModifiedBy")]
	public class DataModel : BaseTable, IBody
	{
		[References(typeof(Application))]
		[PrimaryKey]
		public int ApplicationId { get; set; }

		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		public bool IsActive { get; set; } = true;

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
		{
			if (action == SaveAction.Insert)
			{				
				foreach (var type in BuiltInScalarTypes(Id)) await connection.SaveAsync(type, new SystemUser() { UserName = CreatedBy, LocalTime = DateCreated });
			}
		}

		public static IEnumerable<ModelClass> BuiltInScalarTypes(int dataModelId)
		{
			return new string[]
			{
				"string", "int", "decimal", "date", "datetime", "time",
				"float", "money", "bigint", "byte", "binary", "boolean"
			}.Select(s => new ModelClass() { DataModelId = dataModelId, Name = s, IsScalarType = true });
		}
	}
}