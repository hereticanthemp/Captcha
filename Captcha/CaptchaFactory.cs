using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captcha
{
    public class CaptchaFactory : ICaptchaFactory
    {
        private const string ContentType = "image/jpeg";
        private readonly string[] _symbols = { "@", "#", "$", "%", "&" };
        private readonly string[] _chineseNumbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "零" };
        private const int MarginX = 0;
        private const int MarginY = 0;
        public CaptchaFactory()
        {

        }

        public async Task<CaptchaInfo> CreateAsync(CaptchaOption option)
        {
            var answer = GenerateAnswer(option.Type, option.CharCount);
            var size = CalcSize(option.CharCount, option.FontSize);
            var width = size[0];
            var height = size[1];
            using (var img = new Image<Rgba32>(width, height))
            {
                var fontFamilies = GetFontFamilies();
                Random random = new Random();
                img.Mutate(ctx => ctx.BackgroundColor(Color.WhiteSmoke));
                var position = option.FontSize/2;
                var y = option.FontSize / 2;
                foreach (string c in answer)
                {
                    var font = SystemFonts.CreateFont(fontFamilies[random.Next(0, fontFamilies.Length)], option.FontSize, FontStyle.Regular);
                    var location = new PointF(MarginX + position, y);
                    img.Mutate(ctx => ctx.DrawText(c, font, new Color(new Argb32(0.0f, 0.0f, 1.0f)), location));
                    position += option.FontSize*2;
                }

                using (var memstream = new MemoryStream())
                {
                    await img.SaveAsJpegAsync(memstream);

                    return new CaptchaInfo
                    {
                        Image = memstream.ToArray(),
                        ContentType = ContentType,
                        Answer = string.Join("", answer)
                    };
                }
            }
        }

        #region Setups
        private List<string> GenerateAnswer(CaptchaTypes type, int length)
        {
            var charset = GetCharSet(type);
            List<string> answer = new List<string>();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                answer.Add(charset[random.Next(charset.Length)]);
            }
            return answer;
        }


        #endregion

        #region Foundations
        private string[] GetCharSet(CaptchaTypes types)
        {
            List<string> result = new List<string>();
            if (types.HasFlag(CaptchaTypes.Numeric))
            {
                // Skip 0 if hybrid type
                int startNum = types == CaptchaTypes.Numeric ? 0 : 1;
                string[] arr = Enumerable.Range(startNum, 9).Select(n => n.ToString()).ToArray();
                result.AddRange(arr);
            }

            if (types.HasFlag(CaptchaTypes.UpperCase))
            {
                for (char i = 'A'; i < 'Z'; i++)
                {
                    // Skip O if hybrid type
                    if (i == 'O' && types != CaptchaTypes.UpperCase)
                        continue;
                    result.Add(i.ToString());
                }
            }

            if (types.HasFlag(CaptchaTypes.LowerCase))
            {
                for (char i = 'a'; i < 'z'; i++)
                {
                    // Skip O if hybrid type
                    if (i == 'o' && types != CaptchaTypes.UpperCase)
                        continue;
                    result.Add(i.ToString());
                }
            }

            if (types.HasFlag(CaptchaTypes.Symbols))
                result.AddRange(_symbols);

            if (types.HasFlag(CaptchaTypes.ChineseNumeric))
                result.AddRange(_chineseNumbers);

            return result.ToArray();
        }

        private string[] GetFontFamilies()
        {
            var targets = new string[] { "Arial", "Verdana", "Times New Roman" };
            var systemProvided = SystemFonts.Families.Where(f => targets.Contains(f.Name)).Select(f => f.Name).ToArray();
            return systemProvided;
        }

        /// <summary>
        /// Return Image Width and Height
        /// </summary>
        /// <param name="charCount"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        private int[] CalcSize(int charCount, int fontSize)
        {
            var width = fontSize * charCount * 2 + MarginX * 2;
            var height = fontSize * 2 + MarginY * 2;
            return new int[] { width, height };
        }
        #endregion
    }
}
