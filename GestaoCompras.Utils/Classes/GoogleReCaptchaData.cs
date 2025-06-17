using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.Utils.Classes
{
    public class GoogleReCaptchaData
    {
        public string secret { get; set; }
        public string response { get; set; }

        public GoogleReCaptchaData() { }

        public GoogleReCaptchaData(string secret, string response)
        {
            this.secret = secret;
            this.response = response;
        }

    }
}
