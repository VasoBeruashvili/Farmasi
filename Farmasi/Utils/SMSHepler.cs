using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Farmasi.Utils
{
    public class SMSHepler
    {
        public static string SMSServiceId { get; set; }
        public static string SMSUserName { get; set; }
        public static string SMSClientId { get; set; }
        public static string SMSPassword { get; set; }
        public static string SendSMS(string phone, string text)
        {
            Func<string, string> FormatPhoneNumber = (string number) =>
            {
                string _digitalNumber = new string(number.ToCharArray().Where(p => char.IsDigit(p)).ToArray());
                if (_digitalNumber.Length < 12 && _digitalNumber.Length != 0)
                    _digitalNumber = "995" + _digitalNumber;

                return _digitalNumber;
            };

            if (string.IsNullOrEmpty(text))
                return "ტელეფონის ნომერი ვერ მოიძებნა!";

            if (string.IsNullOrEmpty(text))
                return "შეტყობინების ტექსტი არ არის მითითებული!";

            string _url = "http://msg.ge/bi/sendsms.php?username=" + SMSUserName + @"&password=" +
                          SMSPassword + @"&client_id=" + SMSClientId + "&service_id=" + SMSServiceId +
                          "&to=" + FormatPhoneNumber(phone) + "&text=" + Uri.EscapeDataString(text);

            string _result;
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(_url);
                Request.Method = "GET";
                Request.ContentType = "application/charset=utf-8";
                HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
                

                using (StreamReader resultStream = new StreamReader(Response.GetResponseStream(), Encoding.UTF8))
                {
                    string code = resultStream.ReadToEnd();
                    if (code == null) return "შეტყობინება ვერ გაიგზავნა!";

                    code = code.Split('-').FirstOrDefault().Trim();
                    switch (code)
                    {
                        case "0000": _result = "შეტყობინება წარმატებით გაიგზავნა!"; break;
                        case "0001": _result = "შეტყობინების პარამეტრები არასწორია!"; break;
                        case "0003": _result = "არასწორი მოთხოვნა!"; break;
                        case "0005": _result = "ცარიელი შეტყობინება დაუშვებელია!"; break;
                        case "0006": _result = "არასწორი პრეფიქსი!"; break;
                        case "0007": _result = "მიმღების ნომერი არასწორია!"; break;
                        default: _result = "შეტყობინება ვერ გაიგზავნა!"; break;
                    }
                }
                return _result;
            }
            catch (Exception)
            {
                return "შეტყობინება ვერ გაიგზავნა!";
            }            
        }
      
    }
}