using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace GTMH.S11n
{
	[AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
	public class GTS11nAttribute : System.Attribute
	{
    /// <summary>
    /// This only applies to TInstances which may be null
    /// </summary>
		public bool Required = false;

		public string EditorBrief = "";
		/// <summary>
		/// Indicates the field/property can be polymorphically loaded
		/// load shit
		/// </summary>
		public bool Instance = false;

		/// <summary>
		/// Not visible in the editor but configured via GTFields.Init
		/// </summary>
		//public bool Visible = true;

		/// <summary>
		/// Neither visible in the editor nor configured via GTFields.Init
		/// </summary>
		//public bool NotConfigurable = false;

		/// <summary>
		/// Don't init the field, the declaring type is in charge of manually init
		/// </summary>
		//public bool DelayedInit = false;

		/// <summary>
		/// The item may have been renamed, this was it's old name
		/// </summary>
		public string AKA = null;
    /// <summary>
    /// For custome parse - turn string type representation to an r-value.
    /// For pod the Parse
    /// </summary>
    public string Parse = null;
    public string DeParse = null;
	}
}
