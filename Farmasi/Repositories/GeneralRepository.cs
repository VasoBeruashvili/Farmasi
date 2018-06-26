using Farmasi.Utils;
using Farmasi.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace Farmasi.Repositories
{
    public static class GeneralRepository
    {
        public const int _storeId = 3; // რომელ საწყობს უყუროს (ამ შემთხვევაში ოფისის საწყობს)
        public const int _priceId = 3; // რომელ ფასის ტიპს უყუროს (ამ შემთხვევაში საცალო)

        public static UserViewModel GetCurrentUser()
        {
            return (HttpContext.Current.Session["currentUser"] as UserViewModel);
        }

        public static string GetLanguageCookieValue()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["Language"];
            return cookie != null && !string.IsNullOrEmpty(cookie.Value) ? cookie.Value : "ka-GE";
        }

        public static double? GetProductPrice(int productId)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                SqlParameter[] sqlParams = new SqlParameter[]
                {
                    new SqlParameter { ParameterName = "@productId", Value = productId },
                    new SqlParameter { ParameterName = "@priceId", Value = _priceId }
                };

                return _sqlCtx.GetScalar<double>(@"SELECT pp.manual_val AS price
                                               FROM book.Products AS p
                                               INNER JOIN book.ProductPrices AS pp
                                               ON pp.product_id = p.id AND pp.price_id = @priceId
                                               WHERE p.id = @productId", sqlParams);
            }
        }

        public static KeyValuePair<int, double> GetProductRestOriginal(int product_id, int store_id, DateTime toDate)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                string id_string = product_id.ToString();
                string sql = @"SELECT p.id, (SELECT ISNULL(SUM(a.amount*a.coeff),0) 
                             FROM doc.ProductsFlow a 
                             INNER JOIN doc.GeneralDocs g ON g.id=a.general_id
                             WHERE a.is_order = 0 AND ISNULL(g.is_deleted,0) = 0 AND a.is_expense=0 AND g.tdate<='" + toDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + @"' AND a.product_id = p.id
                             AND a.store_id = CASE " + store_id + @" WHEN 0 THEN a.store_id ELSE " + store_id + @" END) as rest
                             FROM book.Products AS p WHERE p.id = " + id_string + ";";

                return _sqlCtx.GetDictionary<int, double>(sql).FirstOrDefault();
            }
        }

        public static string GetContragentNameById(int contragentId)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetString(@"SELECT c.name FROM book.Contragents AS c WHERE c.id = @contragentId", new SqlParameter("@contragentId", SqlDbType.Int) { Value = contragentId });
            }
        }

        public static int? GetContragentIdByUserName(string userName)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetScalar<int>(@"SELECT id FROM book.Contragents WHERE usr_column_502 = @userName", new SqlParameter("@userName", SqlDbType.NVarChar) { Value = userName });
            }
        }

        public static string GetCompanyCode()
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetString(@"SELECT TOP 1 code FROM book.Companies");
            }
        }

        public static string GetCompanyInfo()
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetString(@"SELECT TOP 1 info FROM book.Companies");
            }
        }

        public static int? GetFarmasiContragentId(string contragentCode)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetScalar<int>(@"SELECT c.id FROM book.Contragents AS c WHERE c.code = @contragentCode", new SqlParameter("@contragentCode", SqlDbType.NVarChar) { Value = contragentCode });
            }
        }

        public static string GenerateRfId()
        {
            return GetCurrentUser().Id.ToString();
        }

        public static UserViewModel GetCurrentContragent()
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                UserViewModel model = _sqlCtx.GetList<UserViewModel>(@"SELECT c.code, c.name, c.tel, c.address, c.e_mail AS email, c.usr_column_502 AS userName FROM book.Contragents AS c WHERE c.id = @id", new SqlParameter("@id", SqlDbType.Int) { Value = GetCurrentUser().Id }).FirstOrDefault();
                model.RfLink = string.Format("{0}://{1}/account/registration?rf_id={2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, GenerateRfId());
                return model;
            }
        }

        public static bool SendEmail(List<dynamic> file, string Subject, string SmtpHost, int SmtpPort, bool EnableSsl, string SenderAddress, string login, string password, List<string> BCC, string MessageText)
        {
            using (MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(SenderAddress),
                Priority = MailPriority.High,
                Body = MessageText,
                Subject = Subject
            })
            {
                try
                {
                    SmtpClient smtpClient = new SmtpClient()
                    {
                        Host = SmtpHost,
                        Port = SmtpPort,
                        Credentials = new NetworkCredential(login, password),
                        EnableSsl = EnableSsl
                    };

                    BCC.ForEach(bcc => { mailMessage.Bcc.Add(bcc); });

                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    throw ex;
                    //return false;
                }

                return true;
            }
        }        

        public static List<UserViewModel> GetRegisteredStaffs(DateTime fromDate, DateTime toDate)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                if (GetCompanyCode() == GetCurrentContragent().Code)
                {
                    return _sqlCtx.GetList<UserViewModel>(@"SELECT c.id, c.code, c.name, c.tel, c.e_mail AS email, cr.create_date AS createDate, c.address FROM book.ContragentRelations AS cr INNER JOIN book.Contragents AS c ON c.id = cr.child_id WHERE CAST(cr.create_date AS DATE) BETWEEN @fromDate AND @toDate", new SqlParameter[]
                    {
                        new SqlParameter("@fromDate", SqlDbType.DateTime) { Value = fromDate },
                        new SqlParameter("@toDate", SqlDbType.DateTime) { Value = toDate }
                    }).OrderByDescending(x => x.Id).ToList();
                }
                else
                {
                    return _sqlCtx.GetList<UserViewModel>(@"SELECT c.id, c.code, c.name, c.tel, c.e_mail AS email, cr.create_date AS createDate, c.address FROM book.ContragentRelations AS cr INNER JOIN book.Contragents AS c ON c.id = cr.child_id WHERE cr.parent_id = @id AND CAST(cr.create_date AS DATE) BETWEEN @fromDate AND @toDate", new SqlParameter[]
                    {
                        new SqlParameter("@id", SqlDbType.Int) { Value = GetCurrentUser().Id },
                        new SqlParameter("@fromDate", SqlDbType.DateTime) { Value = fromDate },
                        new SqlParameter("@toDate", SqlDbType.DateTime) { Value = toDate }
                    }).OrderByDescending(x => x.Id).ToList();
                }
            }
        }        
    }
}