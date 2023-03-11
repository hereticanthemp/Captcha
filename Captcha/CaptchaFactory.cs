﻿using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private const int MarginX = 0;
        private const int MarginY = 0;
        private readonly Color[] _disturbColors = { Color.Red, Color.Blue, Color.Green };
        private const int DisturbLines = 10;

        public CaptchaFactory()
        {
        }

        public async Task<CaptchaInfo> CreateAsync(CaptchaOption option)
        {
            var answer = GenerateAnswer(option.Type, option.CharCount);
            var size = CalcSize(option.CharCount, option.FontSize);
            using var img = new Image<Rgba32>(size[0], size[1]);
            DrawAnswer(img, answer, option);
            DrawDisturb(img);

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
        /// <param name="fontSize"></param>
        /// <returns></returns>
        private static int[] CalcSize(int charCount, int fontSize)
        {
            var width = fontSize * charCount * 2 + MarginX * 2;
            var height = fontSize * 2 + MarginY * 2;
            return new int[] { width, height };
        }

        #endregion

        #region Image Mutations

        private static void DrawAnswer(Image img, List<string> answer, CaptchaOption option)
        {
            var openSans = GetOpenSansTcFontCollection();

            //var fontFamilies = GetFontFamilies();
            var random = new Random();
            img.Mutate(ctx => ctx.BackgroundColor(Color.WhiteSmoke));

            // Write Text into new img, rotate it, then write rotated text img to result img
            using var textImg = new Image<Rgba32>(img.Width, img.Height);
            var position = option.FontSize / 2;
            var y = option.FontSize / 2;
            foreach (var c in answer)
            {
                var font = new Font(openSans.Families.First(), (float)option.FontSize,
                    Extensions.GetRandom<FontStyle>());
                var location = new PointF(MarginX + position, y);
                textImg.Mutate(ctx => ctx.DrawText(c, font, new Color(new Argb32(0.0f, 0.0f, 1.0f)), location));
                position += option.FontSize * 2;
            }

            var r = new Random();
            textImg.Mutate(ctx => ctx.Transform(new AffineTransformBuilder().AppendMatrix(
                new System.Numerics.Matrix3x2
                {
                    M11 = 0.9f,
                    M12 = r.Next(-5, 5) / 100f,
                    M21 = r.Next(-1, 1) / 10f,
                    M22 = 0.9f,
                    M31 = 8.0f,
                    M32 = 45.0f
                })));
            
            var textImgCopy = textImg.Clone(); // Fix Access to disposed closure
            img.Mutate(ctx => ctx.DrawImage(textImgCopy, location: new Point(0, -(textImgCopy.Height / 2)), 1));
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