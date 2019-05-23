using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Ginseng.Mvc.Extensions
{
    public static class SelectListHelper
    {
        public static SelectList FromEnum<TEnum>(object selectedValue = null)
        {
            var names = Enum.GetNames(typeof(TEnum));
            var values = Enum.GetValues(typeof(TEnum));
            var items = names.Select((name, index) => new SelectListItem() { Value = values.GetValue(index).ToString(), Text = name });
            return new SelectList(items, "Value", "Text", selectedValue);
        }
    }
}