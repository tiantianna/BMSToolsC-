using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bms.startup.util
{
    class PasswordBuilder
    {
        
        public static string calPwdByTimes(String seed, int times)
        {
            if (times == 0)
            {
                Console.WriteLine(times + ":" + seed);
                return seed;
            }
            else
            {
                String sss = calPwdByTimes(seed, --times);
                byte[] array = System.Text.Encoding.Default.GetBytes(sss);
                String str = "";
                for (int i = 0; i < array.Length; i++)
                {
                    str += array[i].ToString();
                }
                string[] strArray = strToStrArray(str);
                int length = strArray.Length;
                for (int i = 0; i < length; i++)
                {
                    switch (i % 3)
                    {
                        case 0:
                            strArray[i] = (1.2 * Math.Round(Convert.ToDouble(strArray[i]), 1)).ToString();
                            break;
                        case 1:
                            strArray[i] = (Math.Round(Convert.ToDouble(strArray[i]), 1) + 0.21 * times % 30 - 3.2).ToString();
                            break;
                        case 2:
                            strArray[i] = (Math.Round(Convert.ToDouble(strArray[i]), 1) - Math.Round(Convert.ToDouble(strArray[i - 1]), 1) + (9.8 - times * 0.12)).ToString();
                            break;
                    }
                }
                string s = strArrayToStr(strArray);
                s = s.Replace(".", "");
                s = s.Replace("-", "");
                int len = s.Length;
                if (len > 10)
                {
                    int more = len - 10;
                    int temp = len / more;
                    s = s.Substring(more / 2);
                    s = s.Substring(0, s.Length - more + (more / 2));
                }
                return s;
            }
        }

        //字符串转字符数组
        private static string[] strToStrArray(string s)
        {
            string[] sarray = new string[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                sarray[i] = s.Substring(i, i + 1);
            }
            return sarray;
        }
        //字符数组转字符串
        private static string strArrayToStr(string[] sarray)
        {
            return sarray.ToString();
        }

    }
}
