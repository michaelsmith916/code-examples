using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeExamples.Lib {
    public class EnumUtil {
        public static T ParseEnum<T>(string value,T defaultValue) {
            if (string.IsNullOrEmpty(value)) {
                return defaultValue;
            }

            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IEnumerable<T> GetValues<T>() {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
