using System;
using System.Collections.Generic;
using System.Text;

namespace TaboolaRasa.Models.Attributes
{
    public class EnumDisplay : Attribute
    {
        /// <summary>
        /// The display text
        /// </summary>
        public string Display { get; set; }

        /// <summary>
        /// Initialising the display value
        /// </summary>
        /// <param name="display"></param>
        public EnumDisplay(string display)
        {
            this.Display = display;
        }
    }
}
