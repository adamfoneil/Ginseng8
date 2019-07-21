using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
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

        public static SelectList YesNo(string yesText = "Yes", string noText = "No", object selectedValue = null)
        {
            return new SelectList(YesNoItems(yesText, noText), "Value", "Text", selectedValue);
        }

        public static IEnumerable<SelectListItem> YesNoItems(string yesText = "Yes", string noText = "No", object selectedValue = null)
        {            
            return new SelectListItem[]
            {
                new SelectListItem(yesText, true.ToString(), (selectedValue?.ToString().Equals(true.ToString()) ?? false)),
                new SelectListItem(noText, false.ToString(), (selectedValue?.ToString().Equals(false.ToString()) ?? false))
            };
        }
    }
}