using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bms.startup.util
{
    public class HexCommon
    {

        /// <summary>
        /// 将字节数组转换为16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        /// <summary>
        /// 将字节数组转换为16进制字符串并在字符串中增加空格
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStrSpace(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    returnStr += (bytes[i].ToString("X2") + " ");
                }
            }
            return returnStr;
        }
    }
}
