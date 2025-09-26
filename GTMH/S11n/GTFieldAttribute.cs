using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

// not in the reflection namespace
namespace GTMH
{
	[AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
	public class GTFieldAttribute : System.Attribute
	{
		//public bool Required = false;
		public string EditorBrief = "";
		/// <summary>
		/// Indicates the field/property is reflect loadable. If this is marked true
		/// then the declaring class should have a field/property of type interface
		/// which is named Name+"Instance". If setup this way GTFields.Init can magically
		/// load shit
		/// </summary>
		//public bool TInstance = false;

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
		/// The item may have been named, this was it's old name
		/// </summary>
		public string ? AKA = null;
    /// <summary>
    /// For custome parse, serialise
    /// </summary>
    public string ? Parse = null;
    public string ? DeParse = null;
	}
}
