using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Printer.Formats
{
    public static class ExtensionMethods
    {
        public static string CenterText(this string stringToCenter, int totalLength)
        {
            return stringToCenter.PadLeft(((totalLength - stringToCenter.Length) / 2)
                                + stringToCenter.Length)
                       .PadRight(totalLength);
        }

        public static string FormatTextToPhone(this string text)
        {
            string result = "";
            long phone = 0;
            if (!string.IsNullOrWhiteSpace(text))
            {
                text = text.Replace("(", "").Replace(")", "").Replace("-", "");
                long.TryParse(text, out phone);
                if (phone > 0)
                {
                    if (text.Length == 10)
                    {
                        result = string.Format("{0:###-###-####}", phone);
                    }
                    else if (text.Length == 11)
                    {
                        result = string.Format("{0:####-###-####}", phone);
                    }
                    else
                        result = text;
                }
                else
                    result = text;
            }
            return result;
        }

        public static string ReplaceSymbol(this string TEXT)
        {
            if (TEXT.Contains("?"))
            {
                TEXT = TEXT.Replace("?", " ");
            }
            else if (TEXT.Contains("'"))
            {
                TEXT = TEXT.Replace("'", "");
            }
            else if (TEXT.Contains("/"))
            {
                TEXT = TEXT.Replace("/", "-");
            }

            return TEXT;
        }
    }
}
