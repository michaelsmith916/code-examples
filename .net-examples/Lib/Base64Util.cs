using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeExamples.Lib {
    public class Base64Util {

        public static string convertBase64toString(string base64Data) {
            string corrected = base64Data.Replace(' ', '+').Replace('_', '/');
            byte[] data = Convert.FromBase64String(corrected);
            return Encoding.UTF8.GetString(data);
        }
        public static string convertStringtoBase64(string unencodedData) {
            byte[] textAsBytes = Encoding.UTF8.GetBytes(unencodedData);
            string base64Data = System.Convert.ToBase64String(textAsBytes);

            base64Data = base64Data.Replace('+', ' ').Replace('/', '_');
            return base64Data;
        }

        public static byte[] convertBase64toByteArray(string base64Data) {
            string corrected = base64Data.Replace(' ', '+').Replace('_', '/');
            byte[] data = Convert.FromBase64String(corrected);
            return data;
        }

        public static string convertByteArraytoBase64(byte[] data) {
            string base64Data = System.Convert.ToBase64String(data);
            base64Data = base64Data.Replace('+', ' ').Replace('/', '_');
            return base64Data;
        }
    }
}
