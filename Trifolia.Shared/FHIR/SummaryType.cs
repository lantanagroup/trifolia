using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Trifolia.Shared.FHIR
{
    /// <summary>
    /// Enumeration that indicates what type of summary to respond with
    /// </summary>
    public enum SummaryType
    {
        /// <summary>
        /// true
        /// </summary>
        [EnumMember(Value = "true")]
        True,

        /// <summary>
        /// false
        /// </summary>
        [EnumMember(Value = "false")]
        False,

        /// <summary>
        /// text
        /// </summary>
        [EnumMember(Value = "text")]
        Text,

        /// <summary>
        /// data
        /// </summary>
        [EnumMember(Value = "data")]
        Data
    }
}
