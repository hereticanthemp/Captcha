using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captcha
{
    public class CaptchaInfo
    {
        /// <summary>
        /// Image bytes
        /// </summary>
        public byte[]? Image { get; set; }

        /// <summary>
        /// Image Content Type
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Captcha Answer string
        /// </summary>
        public string? Answer { get; set; }
    }
}
