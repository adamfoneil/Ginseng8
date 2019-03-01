using Ginseng.Models.Conventions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;

namespace Ginseng.Mvc.Classes
{
	public class GridEditor<TRecord> where TRecord : BaseTable
	{
		private List<string> _propertyNames = new List<string>();
		private List<string> _rowHiddenFields = new List<string>();
		private string _tableRowId = null;
		private object _defaults = null;
		private string _namePrefix = null;
		private Dictionary<string, SelectList> _selectLists = new Dictionary<string, SelectList>();
		private string _newRowContext = null;
		private TRecord _record = default(TRecord);
		private int _id = 0;
		private TextWriter _writer = null;
		private HtmlEncoder _encoder = null;

		public GridEditor(TextWriter writer, string namePrefix = null, object defaults = null)
		{
			_writer = writer;
			_encoder = HtmlEncoder.Default;
			_namePrefix = namePrefix;
			_defaults = defaults;
		}

		public string RowId(TRecord record)
		{
			_newRowContext = null;
			_record = record;
			_id = record.Id;
			_tableRowId = ControlId("row");
			return _tableRowId;
		}

		public string NewRowId(string context = null)
		{
			_newRowContext = context;
			_record = default(TRecord);
			_id = 0;
			_tableRowId = ControlId("row");
			return _tableRowId;
		}

		public string ControlId(string propertyName)
		{			
			string result = ((_namePrefix.Length > 0) ? _namePrefix + "-" : "") + propertyName + "-" + _id.ToString();

			if (!string.IsNullOrEmpty(_newRowContext)) result += _newRowContext;

			return result;
		}
	}
}