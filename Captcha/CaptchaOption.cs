using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captcha
{
    public class CaptchaOption
    {
        public CaptchaTypes Type { get; set; }
        public int CharCount { get; set; }
        public float FontSizePt { get; set; }

        public float FontSizePixel => FontSizePt * 4 / 3;
    }
}