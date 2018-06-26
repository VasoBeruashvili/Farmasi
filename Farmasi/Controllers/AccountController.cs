using System.Linq;
using System.Web.Mvc;
using Farmasi.ViewModels;
using Farmasi.Utils;
using System.Data.SqlClient;
using System.Data;
using System;
using Farmasi.Repositories;
using System.Web;
using System.Collections.Generic;

namespace Farmasi.Controllers
{
    public class AccountController : Controller
    {
        private const int _farmasiId = 971;

        private List<int?> parentIds = new List<int?>();

        List<ContragentRelationViewModel> result { get; set; }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserViewModel _user = AuthorizeUser(model.UserName, model.Password);

                if (_user != null)
                {                                     
                    Session.Add("currentUser", new UserViewModel { Id = _user.Id, Name = _user.Name, Code = _user.Code });
                    return RedirectToAction("index", "home");
                }
                else
                {
                    ViewBag.Error = Resources.Translator.WrongUserNameOrPassword;
                    ViewBag.ShowRecoveryPassword = true;
                    return View("login");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(model.MailTo))
                {
                    using(SqlContext _sqlCtx = new SqlContext())
                    {
                        RecoveryViewModel contragent = _sqlCtx.GetList<RecoveryViewModel>("SELECT id, e_mail AS email FROM book.Contragents WHERE @userName = usr_column_502 AND @mailTo = e_mail", new SqlParameter[]
                        {
                            new SqlParameter("@userName", SqlDbType.NVarChar) { Value = model.UserName },
                            new SqlParameter("@mailTo", SqlDbType.NVarChar) { Value = model.MailTo }
                        }).FirstOrDefault();

                        if (string.IsNullOrEmpty(contragent.Email))
                        {
                            ViewBag.Error = Resources.Translator.EmailNotValidForThisUser;
                            return View("login");
                        }
                        else
                        {
                            Random random = new Random();
                            const string chars = "abcdefghijklmnopqrstuvwxyz";
                            char[] pwd = (Enumerable.Repeat(chars, 6)
                              .Select(s => s[random.Next(s.Length)]).ToArray());

                            string pwdc = new string(pwd);

                            string companyInfo = GeneralRepository.GetCompanyInfo();
                            string[] infos = companyInfo.Split(';');
                            bool emailSent = GeneralRepository.SendEmail(
                                new List<dynamic>(),
                                "FARMASi",
                                infos[0],
                                Convert.ToInt32(infos[1]),
                                Convert.ToBoolean(infos[2]),
                                infos[3],
                                infos[4],
                                infos[5],
                                new List<string>() { model.MailTo },
                                string.Format("თქვენი მოთხოვნა პაროლის აღდგენაზე შესრულებულია. სისტემაში შესასვლელად გამოიყენეთ პაროლი {0}", pwdc
                            ));

                            if (emailSent)
                            {
                                _sqlCtx.ExecuteSql("UPDATE book.Contragents SET usr_column_503 = @pwd WHERE id = @id", new SqlParameter[]
                                {
                                    new SqlParameter("@pwd", SqlDbType.NVarChar) { Value = HashHelper.Calc(pwdc) },
                                    new SqlParameter("@id", SqlDbType.Int) { Value = contragent.Id }
                                });

                                ViewBag.Recovered = Resources.Translator.NewPasswordSentOnYourEmail;
                                return View("login");
                            }
                            else
                            {
                                ViewBag.Error = Resources.Translator.ErrorOnRecovery;
                                return View("login");
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.Error = Resources.Translator.UserNameAndPasswordRequired;
                    return View("login");
                }                
            }
        }

        UserViewModel AuthorizeUser(string userName, string password)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                SqlParameter[] _sqlParams = new SqlParameter[]
                {
                    new SqlParameter("@userName", SqlDbType.NVarChar) { Value = userName },
                    new SqlParameter("@password", SqlDbType.NVarChar) { Value = HashHelper.Calc(password) }
                };

                return _sqlCtx.GetList<UserViewModel>("SELECT TOP 1 c.id, c.name, c.code FROM book.Contragents AS c WHERE @userName = c.usr_column_502 AND @password = c.usr_column_503", _sqlParams).FirstOrDefault();
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Remove("currentUser");
            return RedirectToAction("login", "account");
        }

        [AllowAnonymous]
        public ActionResult Registration(string rf_id)
        {           
            if (!string.IsNullOrEmpty(rf_id))
            {
                ViewBag.rfContragentName = GeneralRepository.GetContragentNameById(Convert.ToInt32(rf_id));                
            }

            return View();
        }

        void GetContragentRelationParentIdByChildId(int contragentId)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                int? parentId = _sqlCtx.GetScalar<int>(@"SELECT parent_id FROM book.ContragentRelations WHERE child_id = @contragentId",
                    new SqlParameter("@contragentId", SqlDbType.Int) { Value = contragentId }
                );

                if (parentId != 0)
                {
                    parentIds.Add(parentId);
                    GetContragentRelationParentIdByChildId(parentId.Value);
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(ContragentViewModel model)
        {
            string rf_id = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["rf_id"];

            using (SqlContext _sqlCtx = new SqlContext())
            {
                int rfId = Convert.ToInt32(rf_id);

                if (rf_id != null && rfId > 0)
                {
                    int? rfContragentExists = _sqlCtx.GetScalar<int>("SELECT id FROM book.ContragentRelations WHERE child_id = @rfId", new SqlParameter("@rfId", SqlDbType.Int) { Value = rfId });
                    if (!rfContragentExists.HasValue)
                    {
                        ViewBag.Error = Resources.Translator.RfUserDoesNotExists;
                        return View();
                    }
                }

                if (ModelState.IsValid)
                {
                    if (model.Password == model.RepeatPassword)
                    {
                        int? cntrId = GeneralRepository.GetContragentIdByUserName(model.UserName);

                        if (cntrId == null)
                        {
                            int? contrMaxId = _sqlCtx.GetScalar<int>("SELECT MAX(id) FROM book.Contragents") + 1;                            

                            SqlParameter[] _sqlParams = new SqlParameter[]
                            {
                                new SqlParameter("@id", SqlDbType.Int) { Value = contrMaxId.Value },
                                new SqlParameter("@group_id", SqlDbType.Int) { Value = 5 }, // მყიდველები
                                new SqlParameter("@path", SqlDbType.NVarChar) { Value = "0#2#5" }, // მყიდველები
                                new SqlParameter("@userName", SqlDbType.NVarChar) { Value = model.UserName },
                                new SqlParameter("@password", SqlDbType.NVarChar) { Value = HashHelper.Calc(model.Password) },
                                new SqlParameter("@fullName", SqlDbType.NVarChar) { Value = model.ContragentFullName },
                                new SqlParameter("@phone", SqlDbType.NVarChar) { Value = model.ContragentPhone },
                                new SqlParameter("@email", SqlDbType.NVarChar) { Value = model.Email },
                                new SqlParameter("@address", SqlDbType.NVarChar) { Value = model.Address },
                                new SqlParameter("@birth_date", SqlDbType.DateTime) { Value = DateTime.Now },
                                new SqlParameter("@client_id", SqlDbType.Int) { Value = 0 },
                                new SqlParameter("@cons_period", SqlDbType.Int) { Value = 30 },
                                new SqlParameter("@create_user_id", SqlDbType.Int) { Value = 1 },
                                new SqlParameter("@create_date", SqlDbType.DateTime) { Value = DateTime.Now },
                                new SqlParameter("@limit_type", SqlDbType.Int) { Value = 0 },
                                new SqlParameter("@limit_val", SqlDbType.Decimal) { Value = 0 },
                                new SqlParameter("@limit_term", SqlDbType.SmallInt) { Value = -1 },
                                new SqlParameter("@acc_use", SqlDbType.Int) { Value = 6 },
                                new SqlParameter("@type", SqlDbType.Int) { Value = 0 },
                                new SqlParameter("@person", SqlDbType.NVarChar) { Value = string.Empty },
                                new SqlParameter("@driver_num", SqlDbType.NVarChar) { Value = string.Empty },
                                new SqlParameter("@car_model", SqlDbType.NVarChar) { Value = string.Empty },
                                new SqlParameter("@car_num", SqlDbType.NVarChar) { Value = string.Empty },
                                new SqlParameter("@vat_type", SqlDbType.Int) { Value = 0 },
                                new SqlParameter("@is_resident", SqlDbType.TinyInt) { Value = 0 },
                                new SqlParameter("@account", SqlDbType.NVarChar) { Value = "1410" }, // ხ-ზ
                                new SqlParameter("@account2", SqlDbType.NVarChar) { Value = "3120" }, // ხ-ზ
                                new SqlParameter("@tax", SqlDbType.Float) { Value = 20 },
                                new SqlParameter("@min_tax", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@code", SqlDbType.NVarChar) { Value = model.UserName },
                                new SqlParameter("@short_name", SqlDbType.NVarChar) { Value = model.ContragentFullName },
                                new SqlParameter("@vat", SqlDbType.Bit) { Value = false },
                                new SqlParameter("@country_id", SqlDbType.Int) { Value = 25 }, // საქართველო
                                new SqlParameter("@web", SqlDbType.NVarChar) { Value = "yes" }
                            };

                            var res = _sqlCtx.ExecuteSql(@"INSERT INTO book.Contragents (id, group_id, path, usr_column_502, usr_column_503, name, tel, e_mail, address, birth_date, client_id, cons_period, create_user_id, create_date, limit_type, limit_val, limit_term, acc_use, type, person, driver_num, car_model, car_num, vat_type, is_resident, account, account2, tax, min_tax, code, short_name, country_id, usr_column_506) VALUES(@id, @group_id, @path, @userName, @password, @fullName, @phone, @email, @address, @birth_date, @client_id, @cons_period, @create_user_id, @create_date, @limit_type, @limit_val, @limit_term, @acc_use, @type, @person, @driver_num, @car_model, @car_num, @vat_type, @is_resident, @account, @account2, @tax, @min_tax, @code, @short_name, @country_id, @web)", _sqlParams);

                            if (res.HasValue && res.Value > 0)
                            {
                                string rf_path = _sqlCtx.GetString("SELECT path FROM book.ContragentRelations WHERE child_id = @rfId", new SqlParameter("@rfId", SqlDbType.Int) { Value = rfId == 0 ? _farmasiId : rfId });
                                string rf_path_res = string.Format("{0}#{1}", rf_path, contrMaxId.Value);

                                var finalRes = _sqlCtx.ExecuteSql("INSERT INTO book.ContragentRelations (parent_id, child_id, path, create_date, user_id) VALUES(@parent_id, @child_id, @path, @create_date, @user_id)", new SqlParameter[] {
                                    new SqlParameter("@parent_id", SqlDbType.Int) { Value = rfId == 0 ? _farmasiId : rfId },
                                    new SqlParameter("@child_id", SqlDbType.Int) { Value = contrMaxId },
                                    new SqlParameter("@path", SqlDbType.NVarChar) { Value = rf_path_res },
                                    new SqlParameter("@create_date", SqlDbType.DateTime) { Value = DateTime.Now },
                                    new SqlParameter("@user_id", SqlDbType.Int) { Value = 0 }
                                });

                                if (finalRes.HasValue && finalRes.Value > 0)
                                {
                                    Dictionary<string, string> smsParams = _sqlCtx.GetDictionary<string, string>("SELECT * FROM config.Params WHERE name IN('SMS_ServiceId', 'SMS_UserName', 'SMS_ClientId', 'SMS_Password')");

                                    SMSHepler.SMSServiceId = smsParams.First(s => s.Key == "SMS_ServiceId").Value;
                                    SMSHepler.SMSUserName = smsParams.First(s => s.Key == "SMS_UserName").Value;
                                    SMSHepler.SMSClientId = smsParams.First(s => s.Key == "SMS_ClientId").Value;
                                    SMSHepler.SMSPassword = smsParams.First(s => s.Key == "SMS_Password").Value;

                                    string smsSendResult = SMSHepler.SendSMS(model.ContragentPhone, "tqven warmatebit gaiaret registracia");

                                    if (smsSendResult == "შეტყობინება წარმატებით გაიგზავნა!")
                                    {
                                        Dictionary<int, string> farmasiPhones = _sqlCtx.GetDictionary<int, string>("SELECT id, tel FROM book.ContragentContacts WHERE contragent_id = @contragentId",
                                            new SqlParameter("@contragentId", SqlDbType.Int) { Value = _farmasiId }
                                        );

                                        GetContragentRelationParentIdByChildId(contrMaxId.Value);

                                        string _parentIds = string.Join(",", parentIds.Select(pi => pi.ToString()).ToArray());

                                        Dictionary<int, string> parentPhones = _sqlCtx.GetDictionary<int, string>("SELECT id, tel FROM book.Contragents WHERE id IN(" + _parentIds + ")");

                                        parentPhones.ToList().ForEach(pp =>
                                        {
                                            if (!string.IsNullOrEmpty(pp.Value))
                                                SMSHepler.SendSMS(pp.Value, "momxmarebeli telefonis nomrit: " + model.ContragentPhone + " gawevrianda tqven jgufshi");
                                        });

                                        farmasiPhones.ToList().ForEach(fp =>
                                        {
                                            SMSHepler.SendSMS(fp.Value, "momxmarebeli telefonis nomrit: " + model.ContragentPhone + " gawevrianda kompaniashi FARMASi");
                                        });

                                        return View("login");
                                    }
                                    else
                                    {
                                        ViewBag.Error = "SMS: " + smsSendResult;
                                        return View("login");
                                    }
                                }
                                else
                                {
                                    _sqlCtx.ExecuteSql("DELETE FROM book.Contragents WHERE id = @contrId", new SqlParameter("@contrId", SqlDbType.Int) { Value = contrMaxId });
                                    return Redirect(Request.UrlReferrer.ToString());
                                }
                            }
                            else
                            {
                                return Redirect(Request.UrlReferrer.ToString());
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(rf_id))
                            {
                                ViewBag.Error = Resources.Translator.UserGivenUserNameAlreadyExists;
                                return View();
                            }
                            else
                            {
                                return Redirect("/account/registration?rf_id=" + rf_id);
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(rf_id))
                        {
                            ViewBag.Error = Resources.Translator.RepeatPasswordAndPasswordAreNotTheSame;
                            return View();
                        }
                        else
                        {
                            return Redirect("/account/registration?rf_id=" + rf_id);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(rf_id))
                    {
                        ViewBag.Error = Resources.Translator.AllStarFieldsRequired;
                        return View();
                    }
                    else
                    {
                        return Redirect("/account/registration?rf_id=" + rf_id);
                    }
                }
            }
        }        

        [ValidateUserFilter]
        public ActionResult Profile(ChangePasswordViewModel model)
        {
            int cid = GeneralRepository.GetCurrentUser().Id;
            RelationTreeNodeViewModel mainNode = new RelationTreeNodeViewModel();
            string companyCode = GeneralRepository.GetCompanyCode();
            string contragentCode = GeneralRepository.GetCurrentContragent().Code;
            bool fillRes = StructureRepository.FillRelations(mainNode, companyCode == contragentCode ? 0 : cid);

            RecursiveCountAndSum(mainNode.Nodes);

            ViewBag.contragents = mainNode.Nodes;

            if (ModelState.IsValid)
            {
                if (model.NewPassword == model.RepeatPassword)
                {
                    if (model.CurrentPassword == model.NewPassword)
                    {
                        ViewBag.Error = Resources.Translator.CurrentPasswordAndNewPasswordAreTheSame;
                        return View();
                    }
                    else
                    {
                        using (SqlContext _sqlCtx = new SqlContext())
                        {
                            int? isCurrentPasswordValid = _sqlCtx.GetScalar<int>("SELECT id FROM book.Contragents WHERE id = @id AND @currentPassword = usr_column_503", new SqlParameter[]
                            {
                                new SqlParameter("@currentPassword", SqlDbType.NVarChar) { Value = HashHelper.Calc(model.CurrentPassword) },
                                new SqlParameter("@id", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id }
                            });

                            if (isCurrentPasswordValid.HasValue)
                            {
                                int? passwordChangeResult = _sqlCtx.ExecuteSql("UPDATE book.Contragents SET usr_column_503 = @newPassword WHERE id = @id", new SqlParameter[]
                                {
                                    new SqlParameter("@newPassword", SqlDbType.NVarChar) { Value = HashHelper.Calc(model.NewPassword) },
                                    new SqlParameter("@id", SqlDbType.Int) { Value = isCurrentPasswordValid.Value }
                                });

                                if (passwordChangeResult.HasValue)
                                {
                                    Session.Remove("currentUser");
                                    return View("login");
                                }
                                else
                                {
                                    ViewBag.Error = Resources.Translator.ErrorOnSave;
                                    return View();
                                }
                            }
                            else
                            {
                                ViewBag.Error = Resources.Translator.CurrentPasswordIsNotValid;
                                return View();
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.Error = Resources.Translator.RepeatPasswordAndNewPasswordAreNotTheSame;
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        void RecursiveCountAndSum(List<RelationTreeNodeViewModel> nodes)
        {
            nodes.ForEach(n =>
            {
                n.Text = string.Format("{0} ({1}) [{2}]", n.Text, n.SubNodeCountSum - 1, n.Amount);

                if(n.Nodes != null)
                {
                    RecursiveCountAndSum(n.Nodes);
                }
            });
        }

        [ValidateUserFilter]
        public JsonResult Generate()
        {
            StructureRepository.FillTypes(true);

            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }
    }
}