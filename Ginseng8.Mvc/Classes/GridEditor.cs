using Ginseng.Models.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Encodings.Web;

namespace Ginseng.Mvc.Classes
{
	public class GridEditor<TRecord> where TRecord : BaseTable
	{
		private List<string> _propertyNames = new List<string>();
		private List<string> _rowHiddenFields = new List<string>();
		private string _tableRowId = null;				
		private Dictionary<string, SelectList> _selectLists = new Dictionary<string, SelectList>();		
		private TRecord _record = default(TRecord);
		private int _id = 0;
		private string _newRowContext = null;

		private readonly string _namePrefix = null;
		private readonly object _defaults = null;
		private readonly PageBase _pageBase = null;
		private readonly ViewContext _viewContext = null;
		private readonly TextWriter _writer = null;
		private readonly HtmlEncoder _encoder = null;

		public string OnEditCallback { get; set; }
		public string OnSaveCallback { get; set; }
		public string OnActionCompleteCallback { get; set; }
		public string[] HiddenFields { get; set; }
		public string InsertFunction { get; set; } = "DataGridInsertRow";
		public string UpdateFunction { get; set; } = "DataGridSaveRow";
		public string DeleteFunction { get; set; } = "DataGridDeleteRow";

		public GridEditor(PageBase pageBase, string namePrefix = null, object defaults = null)
		{
			_pageBase = pageBase;
			_viewContext = pageBase.ViewContext;
			_writer = _viewContext.Writer;
			_encoder = _pageBase.HtmlEncoder;
			_namePrefix = namePrefix ?? string.Empty;
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

		public object Controls(bool allowEdit = true, bool allowDelete = true, string validationFunction = null)
		{
			string editFieldIDs = "[" + string.Join(",", _propertyNames.Where(s => !_rowHiddenFields.Contains(s)).Select(s => "'" + ControlId(s) + "'")) + "]";
			string editFieldNames = "[" + string.Join(",", _propertyNames.Where(s => !_rowHiddenFields.Contains(s)).Select(s => "'" + s + "'")) + "]";

			string saveFieldIDs = (HiddenFields != null) ?
				"[" + string.Join(",", _propertyNames.Concat(HiddenFields).Select(s => "'" + ControlId(s) + "'")) + "]" :
				"[" + string.Join(",", _propertyNames.Select(s => "'" + ControlId(s) + "'")) + "]";

			string saveFieldNames = (HiddenFields != null) ?
				"[" + string.Join(",", _propertyNames.Concat(HiddenFields).Select(s => "'" + s + "'")) + "]" :
				"[" + string.Join(",", _propertyNames.Select(s => "'" + s + "'")) + "]";

			string jsArgsEdit = "'" + _tableRowId + "', " + editFieldIDs + ", " + _id.ToString();
			jsArgsEdit += ", '" + SaveFormId() + "', " + (!string.IsNullOrEmpty(OnEditCallback) ? OnEditCallback : "null");

			string jsArgsSave = saveFieldIDs + ", " + saveFieldNames;
			jsArgsSave += ", " + (!string.IsNullOrEmpty(validationFunction) ? validationFunction : "null") + ", " + (!string.IsNullOrEmpty(OnActionCompleteCallback) ? OnActionCompleteCallback : "null");

			string jsArgsInsert = jsArgsSave;
			Dictionary<string, object> hiddenDefaults;

			if (IsSavedRow())
			{
				// edit, delete links
				TagBuilder spanClean = new TagBuilder("span");
				spanClean.MergeAttribute("id", _tableRowId + "-clean");

				if (allowEdit)
				{
					TagBuilder aEdit = new TagBuilder("a");
					aEdit.MergeAttribute("id", $"aGridEditorEdit{_namePrefix}_{_id}");
					aEdit.MergeAttribute("href", "javascript:DataGridEditRow(" + jsArgsEdit + ")");
					aEdit.InnerHtml.Append("edit");
					spanClean.InnerHtml.AppendHtml(aEdit);
				}

				if (allowDelete)
				{
					if (allowEdit) spanClean.InnerHtml.AppendHtml("&nbsp;|&nbsp;");

					TagBuilder aDelete = new TagBuilder("a");
					aDelete.MergeAttribute("id", $"aGridEditorDelete{_namePrefix}-{_id}");
					aDelete.InnerHtml.Append("delete");
					aDelete.MergeAttribute("href", "javascript:" + DeleteFunction + "('" + DeleteFormId() + "', " + _id.ToString() + ")");
					spanClean.InnerHtml.AppendHtml(aDelete);
				}

				spanClean.WriteTo(_writer, _encoder);

				// save, cancel links
				TagBuilder spanDirty = new TagBuilder("span");
				spanDirty.MergeAttribute("id", _tableRowId + "-dirty");
				spanDirty.MergeAttribute("style", "display:none");

				TagBuilder aSave = new TagBuilder("a");
				aSave.MergeAttribute("id", $"aGridEditorSave{_namePrefix}_{_id}");
				aSave.MergeAttribute("href", "javascript:" + UpdateFunction + "(" + jsArgsSave + ")");
				aSave.InnerHtml.Append("save");
				spanDirty.InnerHtml.AppendHtml(aSave);

				spanDirty.InnerHtml.AppendHtml("&nbsp;|&nbsp;");

				TagBuilder aCancel = new TagBuilder("a");
				aCancel.MergeAttribute("href", "javascript:DataGridCancelEdit(" + (!string.IsNullOrEmpty(OnActionCompleteCallback) ? OnActionCompleteCallback : "null") + ")");
				aCancel.InnerHtml.Append("cancel");
				spanDirty.InnerHtml.AppendHtml(aCancel);
				spanDirty.WriteTo(_writer, _encoder);
			}
			else
			{
				TagBuilder aInsert = new TagBuilder("a");
				aInsert.MergeAttribute("id", $"aGridEditorInsert{_namePrefix}");
				aInsert.MergeAttribute("href", "javascript:" + InsertFunction + "('" + SaveFormId() + "', " + jsArgsSave + ")");
				aInsert.InnerHtml.Append("add record");
				aInsert.WriteTo(_writer, _encoder);
			}

			return null;
		}

		#region Input rendering

		public object TextBox<TValue>(Expression<Func<TRecord, TValue>> expression, object htmlAttributes = null)
		{
			string propertyName = PropertyNameFromLambda(expression);
			if (!_propertyNames.Contains(propertyName)) _propertyNames.Add(propertyName);

			TagBuilder textBox = new TagBuilder("input");
			textBox.MergeAttribute("type", "text");
			textBox.MergeAttribute("name", propertyName);
			textBox.MergeAttribute("id", ControlId(propertyName));
			if (htmlAttributes != null) textBox.MergeAttributes(new RouteValueDictionary(htmlAttributes));
			string displayValue = string.Empty;

			if (IsSavedRow())
			{
				var function = expression.Compile();
				object rawValue = function.Invoke(_record);

				if (rawValue != null)
				{
					displayValue = rawValue.ToString();

					PropertyInfo pi = _record.GetType().GetProperty(propertyName);
					object[] attr = pi.GetCustomAttributes(typeof(DisplayFormatAttribute), false);
					if (attr.Length == 1)
					{
						DisplayFormatAttribute format = (DisplayFormatAttribute)attr[0];
						displayValue = string.Format(format.DataFormatString, rawValue);
					}
				}

				textBox.MergeAttribute("value", displayValue);
				WriteSpans(propertyName, textBox, displayValue);				
			}
			else
			{
				object defaultText;
				if (DefaultValueExists(propertyName, out defaultText)) textBox.MergeAttribute("value", defaultText.ToString());
				textBox.WriteTo(_writer, _encoder);				
			}

			return null;
		}

		#endregion

		public object ActionForms(string saveHandler, string deleteHandler, string returnUrl = null)
		{
			TagBuilder formSpan = new TagBuilder("span");
			formSpan.MergeAttribute("style", "display:none");

			var url = new UrlHelper(_viewContext);
			if (string.IsNullOrEmpty(returnUrl)) returnUrl = UriHelper.GetEncodedUrl(_pageBase.Request);

			// save form
			TagBuilder saveForm = new TagBuilder("form");
			saveForm.MergeAttribute("action", HandlerAction(saveHandler));
			saveForm.MergeAttribute("method", "post");
			saveForm.MergeAttribute("id", SaveFormId());
			if (!string.IsNullOrEmpty(returnUrl)) AddReturnUrlField(saveForm, returnUrl);
			foreach (var prop in _propertyNames)
			{
				TagBuilder hidden = new TagBuilder("input");
				hidden.MergeAttribute("type", "hidden");
				hidden.MergeAttribute("name", prop);
				saveForm.InnerHtml.AppendHtml(hidden);
			}

			if (HiddenFields != null)
			{
				foreach (string fieldName in HiddenFields)
				{
					TagBuilder hidden = new TagBuilder("input");
					hidden.MergeAttribute("type", "hidden");
					hidden.MergeAttribute("name", fieldName);
					saveForm.InnerHtml.AppendHtml(hidden);
				}
			}

			if (_defaults != null)
			{
				PropertyInfo[] props = _defaults.GetType().GetProperties();
				foreach (PropertyInfo pi in props)
				{
					TagBuilder defaultValue = new TagBuilder("input");
					defaultValue.MergeAttribute("type", "hidden");
					defaultValue.MergeAttribute("name", pi.Name);
					defaultValue.MergeAttribute("value", pi.GetValue(_defaults, null).ToString());
					saveForm.InnerHtml.AppendHtml(defaultValue);
				}
			}

			TagBuilder hiddenRowId = new TagBuilder("input");
			hiddenRowId.MergeAttribute("type", "hidden");
			hiddenRowId.MergeAttribute("name", "ID");
			saveForm.InnerHtml.AppendHtml(hiddenRowId);

			formSpan.InnerHtml.AppendHtml(saveForm);

			// delete form
			TagBuilder deleteForm = new TagBuilder("form");
			deleteForm.MergeAttribute("action", HandlerAction(deleteHandler));
			deleteForm.MergeAttribute("method", "post");
			deleteForm.MergeAttribute("id", DeleteFormId());
			if (!string.IsNullOrEmpty(returnUrl)) AddReturnUrlField(deleteForm, returnUrl);
			deleteForm.InnerHtml.AppendHtml(hiddenRowId);

			formSpan.InnerHtml.AppendHtml(deleteForm);

			formSpan.WriteTo(_writer, _encoder);

			return null;
		}

		private string HandlerAction(string handlerName)
		{
			return _pageBase.Request.Path + $"?handler={handlerName}";
		}

		private void AddReturnUrlField(TagBuilder formTag, string returnUrl)
		{
			TagBuilder hidden = new TagBuilder("input");
			hidden.MergeAttribute("type", "hidden");
			hidden.MergeAttribute("name", "returnUrl");
			hidden.MergeAttribute("value", returnUrl);
			formTag.InnerHtml.AppendHtml(hidden);
		}

		private bool IsSavedRow()
		{
			return (_id != 0);
		}

		private string SaveFormId()
		{
			return "saveForm-" + _namePrefix;
		}

		private string DeleteFormId()
		{
			return "deleteForm-" + _namePrefix;
		}		

		private string PropertyNameFromLambda(Expression expression)
		{
			// thanks to http://odetocode.com/blogs/scott/archive/2012/11/26/why-all-the-lambdas.aspx
			// thanks to http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression

			LambdaExpression le = expression as LambdaExpression;
			if (le == null) throw new ArgumentException("expression");

			MemberExpression me = null;
			if (le.Body.NodeType == ExpressionType.Convert)
			{
				me = ((UnaryExpression)le.Body).Operand as MemberExpression;
			}
			else if (le.Body.NodeType == ExpressionType.MemberAccess)
			{
				me = le.Body as MemberExpression;
			}

			if (me == null) throw new ArgumentException("expression");

			return me.Member.Name;
		}

		private bool DefaultValueExists(string propertyName, out object value)
		{
			value = null;
			if (_defaults == null) return false;

			PropertyInfo[] props = _defaults.GetType().GetProperties();
			PropertyInfo pi = props.SingleOrDefault(p => p.Name.Equals(propertyName));
			if (pi != null)
			{
				value = pi.GetValue(_defaults, null);
				return true;
			}

			return false;
		}

		private void WriteSpans(string propertyName, TagBuilder editMarkup, string displayMarkup)
		{
			// helpful discussion at https://stackoverflow.com/questions/32416425/tagbuilder-innerhtml-in-asp-net-5-mvc-6

			TagBuilder spanEdit = new TagBuilder("span");
			spanEdit.MergeAttribute("id", "edit-" + ControlId(propertyName));
			spanEdit.MergeAttribute("style", "display:none");						
			spanEdit.InnerHtml.AppendHtml(editMarkup);
			spanEdit.WriteTo(_writer, _encoder);

			TagBuilder spanDisplay = new TagBuilder("span");
			spanDisplay.MergeAttribute("id", "display-" + ControlId(propertyName));
			spanDisplay.InnerHtml.AppendHtml(displayMarkup);
			spanDisplay.WriteTo(_writer, _encoder);			
		}		
	}
}