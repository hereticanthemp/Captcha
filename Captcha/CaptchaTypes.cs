using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captcha
{
    [Flags]
    public enum CaptchaTypes
    {
        /// <summary>
        /// Unset
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Number
        /// </summary>
        Numeric = 1,

        /// <summary>
        /// A~Z
        /// </summary>
        UpperCase = 2,

        /// <summary>
        /// a~z
        /// </summary>
        LowerCase = 4,

        /// <summary>
        /// @#$%& only
        /// </summary>
        Symbols = 8,

        /// <summary>
        /// 0~9 in chinese words
        /// </summary>
        ChineseNumeric = 16,
    }
}
