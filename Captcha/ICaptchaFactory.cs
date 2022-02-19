namespace Captcha
{
    public interface ICaptchaFactory
    {
        /// <summary>
        /// Return Captcha Image and Answer string
        /// </summary>
        /// <param name="captchaInfo"></param>
        /// <returns></returns>
        Task<CaptchaInfo> CreateAsync(CaptchaOption captchaInfo);
    }
}