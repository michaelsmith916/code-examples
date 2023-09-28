using CodeExamples.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PrintWebConfig.Lib {
    public class WebUtil {

        public static Dictionary<string, string> decodeParams(string data) {
            //TODO - replace with this...
            //   NameValueCollection qscoll = HttpUtility.ParseQueryString(querystring);


            //using System.Web and Add a Reference to System.Web
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            if (data != null) {

                string[] rawParams = data.Split('&');
                foreach (string param in rawParams) {
                    string[] kvPair = param.Split('=');
                    if (kvPair != null && kvPair.Length == 2) {
                        string key = kvPair[0];
                        string value = WebUtility.UrlDecode(kvPair[1]);
                        postParams.Add(key, value);
                        Logging.Log("reading posted values - " + key + " value: " + value);
                    }
                }
            }

            return postParams;
        }
    }
}
