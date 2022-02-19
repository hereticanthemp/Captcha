using Captcha;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCaptcha(this IServiceCollection services)
        {
            return services.AddScoped<ICaptchaFactory, CaptchaFactory>();
        }
    }
}
