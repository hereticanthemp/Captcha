using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captcha
{
    public class CaptchaFactory : ICaptchaFactory
    {
        private readonly string[] _symbols = { "@", "#", "$", "%", "&" };
        private readonly string[] _chineseNumbers = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "零" };

        public CaptchaFactory()
        {

        }

        public Task<CaptchaInfo> CreateAsync(CaptchaOption option)
        {
            var answ = GenerateAnswer(option.Type,option.CharCount);
            throw new NotImplementedException();
        }

        #region Setups
        private string GenerateAnswer(CaptchaTypes type, int length)
        {
            var charset = GetCharSet(type);
            return "";
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
        #endregion
    }
}
