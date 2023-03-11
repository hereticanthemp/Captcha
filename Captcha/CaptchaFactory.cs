using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Captcha
{
    public class CaptchaFactory : ICaptchaFactory
    {
        private const string ContentType = "image/jpeg";
        private readonly string[] _symbols = { "@", "#", "$", "%", "&" };
        private readonly string[] _chineseNumbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "零" };

        private const float BoxSize = 1.5f; // For One Char, Leave 1.5 * FontPt (pixel)

        private readonly Color[] _disturbColors = { Color.Red, Color.Blue, Color.Green };
        private const int DisturbLines = 10;

        public async Task<CaptchaInfo> CreateAsync(CaptchaOption option)
        {
            var answer = GenerateAnswer(option.Type, option.CharCount);
            var size = CalcSize(option.CharCount, option.FontSizePixel);
            using var img = new Image<Rgba32>(size[0], size[1]);
            DrawAnswer(img, answer, option);
            //DrawDisturb(img);

            return new CaptchaInfo
            {
                Image = await img.ToByteArray(),
                ContentType = ContentType,
                Answer = string.Join("", answer)
            };
        }

        #region Setups

        private List<string> GenerateAnswer(CaptchaTypes type, int length)
        {
            var charset = GetCharSet(type);
            var answer = new List<string>();
            var random = new Random();
            for (var i = 0; i < length; i++)
            {
                answer.Add(charset[random.Next(charset.Length)]);
            }

            return answer;
        }

        #endregion

        #region Foundations

        private string[] GetCharSet(CaptchaTypes types)
        {
            var result = new List<string>();
            if (types.HasFlag(CaptchaTypes.Numeric))
            {
                // Skip 0 if hybrid type
                var startNum = types == CaptchaTypes.Numeric ? 0 : 1;
                var arr = Enumerable.Range(startNum, 9).Select(n => n.ToString()).ToArray();
                result.AddRange(arr);
            }

            if (types.HasFlag(CaptchaTypes.UpperCase))
            {
                for (var i = 'A'; i < 'Z'; i++)
                {
                    // Skip O if hybrid type
                    if (i == 'O' && types != CaptchaTypes.UpperCase)
                        continue;
                    result.Add(i.ToString());
                }
            }

            if (types.HasFlag(CaptchaTypes.LowerCase))
            {
                for (var i = 'a'; i < 'z'; i++)
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

        private static FontCollection GetOpenSansTcFontCollection()
        {
            var fonts = new FontCollection();
            var dllPath = Assembly.GetExecutingAssembly().Location;
            var dllFolder = Path.GetDirectoryName(dllPath);
            fonts.Add(dllFolder + "/Fonts/NotoSansTC-Regular.otf");
            return fonts;
        }

        /// <summary>
        /// Return Image Width and Height
        /// </summary>
        /// <param name="charCount"></param>
        /// <param name="fontSizePt"></param>
        /// <returns></returns>
        private static int[] CalcSize(int charCount, float fontSizePt)
        {
            var width = fontSizePt * charCount * BoxSize;
            return new[] { (int)width, (int)(fontSizePt * BoxSize) };
        }

        #endregion

        #region Image Mutations

        private static void DrawAnswer(Image<Rgba32> img, List<string> answer, CaptchaOption option)
        {
            var r = new Random();
            var openSans = GetOpenSansTcFontCollection();

            img.Mutate(ctx => ctx.BackgroundColor(Color.WhiteSmoke));

            using var textImg = img.Clone();

            var drawPosition = new PointF(0, 0); // Every Font Box Left Top Corner

            foreach (var c in answer)
            {
                var font = new Font(openSans.Families.First(), (float)option.FontSizePt,
                    Extensions.GetRandom<FontStyle>());

                var options = new TextOptions(font)
                {
                    Dpi = 72,
                    KerningMode = KerningMode.Standard
                };

                var rect = TextMeasurer.Measure(c, options);

                var rectDrawPoint = new PointF()
                {
                    X = drawPosition.X + option.FontSizePixel * BoxSize / 2 - rect.Width / 2,
                    Y = drawPosition.Y + option.FontSizePixel * BoxSize / 2 - rect.Height / 2,
                };

                // Make word shift
                var threshold = (int)(option.FontSizePixel / 3);
                rectDrawPoint.X += r.Next(-threshold, threshold);
                rectDrawPoint.Y += r.Next(-threshold, threshold);


                var rectCenter = new PointF
                {
                    X = drawPosition.X + option.FontSizePixel * BoxSize / 2,
                    Y = drawPosition.Y + option.FontSizePixel * BoxSize / 2,
                };


                textImg.Mutate(
                    ctx => ctx
                        .SetDrawingTransform(
                            Matrix3x2Extensions.CreateRotationDegrees(r.Next(-45, 45), rectCenter)) // Set Rotate
                        .DrawText(c, font, new Color(new Argb32(0.0f, 0.0f, 1.0f)), rectDrawPoint)); // Draw

                drawPosition.X += option.FontSizePixel * BoxSize; // Next Box
            }

            var textImgCopy = textImg.Clone(); // Fix Access to disposed closure
            img.Mutate(ctx => ctx.DrawImage(textImgCopy, location: new Point(0, 0), 1));
        }

        private void DrawDisturb(Image img)
        {
            var random = new Random();
            for (var i = 0; i < DisturbLines; i++)
            {
                var thickness = random.Next(5, 10) / 10f;
                var x1 = random.Next(img.Width);
                var x2 = random.Next(img.Width);
                var y1 = random.Next(img.Height);
                var y2 = random.Next(img.Height);
                img.Mutate(ctx =>
                    ctx.DrawLines(_disturbColors.GetRandom(), thickness, new PointF(x1, y1), new PointF(x2, y2)));
            }
        }

        #endregion
    }

    public static class Extensions
    {
        public static async Task<byte[]> ToByteArray(this Image img)
        {
            using var stream = new MemoryStream();
            await img.SaveAsJpegAsync(stream);
            return stream.ToArray();
        }

        public static TEnum GetRandom<TEnum>()
        {
            var values = Enum.GetValues(typeof(TEnum));
            var rnd = new Random();
            var randomEnum = (TEnum)values.GetValue(rnd.Next(values.Length))!;
            return randomEnum;
        }

        public static T GetRandom<T>(this IEnumerable<T> collection)
        {
            var rnd = new Random();
            var enumerable = collection as T[] ?? collection.ToArray();
            return enumerable.ToArray()[rnd.Next(enumerable.Length)];
        }
    }
}