using Farmasi.Utils;
using Farmasi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;

namespace Farmasi.Repositories
{
    public static class ShoppingRepository
    {       
        public static bool AddToCart(int productId, int quantity)
        {
            if(quantity > 0)
            {
                using (SqlContext _sqlCtx = new SqlContext())
                {
                    int? count = GetCartProductsCountByProduct(productId);
                    int? result = null;
                    KeyValuePair<int, double> rest = GeneralRepository.GetProductRestOriginal(productId, GeneralRepository._storeId, DateTime.Now);

                    if(rest.Value > 0)
                    {
                        if (count == 0)
                        {
                            if (quantity <= rest.Value)
                            {
                                result = _sqlCtx.ExecuteSql("INSERT INTO shop.CartProducts(user_id, product_id, quantity, price) VALUES(@userId, @productId, @quantity, @price)", new SqlParameter[]
                                {
                                    new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id },
                                    new SqlParameter("@productId", SqlDbType.Int) { Value = productId },
                                    new SqlParameter("@quantity", SqlDbType.Int) { Value = quantity },
                                    new SqlParameter("@price", SqlDbType.Decimal) { Value = GeneralRepository.GetProductPrice(productId) }
                                });
                            }
                        }
                        else
                        {
                            int? db_quantity = GetCartProductsQuantity(productId);
                            int _quantity = quantity + db_quantity.Value;

                            if (_quantity <= rest.Value)
                            {
                                result = _sqlCtx.ExecuteSql("UPDATE shop.CartProducts SET quantity = @quantity WHERE user_id = @userId AND product_id = @productId", new SqlParameter[]
                                {
                                    new SqlParameter("@quantity", SqlDbType.Int) { Value = _quantity },
                                    new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id },
                                    new SqlParameter("@productId", SqlDbType.Int) { Value = productId }
                                });
                            }
                        }
                    }

                    return result.HasValue && result.Value > 0;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool RemoveFromCart(int productId, int quantity)
        {
            if(quantity > 0)
            {
                using (SqlContext _sqlCtx = new SqlContext())
                {
                    int? count = GetCartProductsCountByProduct(productId);
                    int? result = null;

                    if (count != 0)
                    {
                        int? db_quantity = GetCartProductsQuantity(productId);

                        if (db_quantity == quantity)
                        {
                            result = _sqlCtx.ExecuteSql("DELETE FROM shop.CartProducts WHERE user_id = @userId AND product_id = @productId", new SqlParameter[]
                            {
                            new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id },
                            new SqlParameter("@productId", SqlDbType.Int) { Value = productId }
                            });
                        }
                        else if (quantity < db_quantity)
                        {
                            result = _sqlCtx.ExecuteSql("UPDATE shop.CartProducts SET quantity = @quantity WHERE user_id = @userId AND product_id = @productId", new SqlParameter[]
                            {
                            new SqlParameter("@quantity", SqlDbType.Int) { Value = db_quantity.Value - quantity },
                            new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id },
                            new SqlParameter("@productId", SqlDbType.Int) { Value = productId }
                            });
                        }
                    }

                    return result.HasValue && result.Value > 0;
                }
            }
            else
            {
                return false;
            }
        }

        public static List<ProductViewModel> GetCartProducts()
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetList<ProductViewModel>(@"SELECT p.id, p.code, p.name, cp.quantity, p.path, (SELECT TOP 1 pi.img FROM book.ProductImages AS pi WHERE pi.product_id = p.id) AS image
                                                           FROM book.Products AS p
                                                           INNER JOIN shop.CartProducts AS cp ON cp.product_id = p.id AND cp.user_id = @userId", new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id });
            }
        }

        public static int? GetCartProductsCount()
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetScalar<int>("SELECT COUNT(id) AS count FROM shop.CartProducts WHERE user_id = @userId", new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id });
            }
        }

        public static int? GetCartProductsCountByProduct(int productId)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetScalar<int>("SELECT COUNT(id) AS count FROM shop.CartProducts WHERE user_id = @userId AND product_id = @productId", new SqlParameter[] {
                    new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id },
                    new SqlParameter("@productId", SqlDbType.Int) { Value = productId }
                });
            }
        }

        public static int? GetCartProductsQuantity(int productId)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetScalar<int>("SELECT quantity FROM shop.CartProducts WHERE user_id = @userId AND product_id = @productId", new SqlParameter[] {
                    new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id },
                    new SqlParameter("@productId", SqlDbType.Int) { Value = productId }
                });
            }
        }
        
        public static bool PlaceOrder()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://rates.fina.ge:8081/CurrencyService/CurrencyService.svc/GetRateByDate/currency=" + "USD" + "/date=" + DateTime.Now.ToString("yyyy-MM-dd"));
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var rateResult = JsonConvert.DeserializeObject<dynamic>(responseString);
            if (rateResult == null)
                return false;
            double currRate = rateResult.rate;            

            using (SqlContext _sqlCtx = new SqlContext())
            {
                List<CartProductViewModel> cartProducts = _sqlCtx.GetList<CartProductViewModel>("SELECT cp.id, cp.user_id AS userId, cp.product_id AS productId, cp.quantity, cp.price FROM shop.CartProducts AS cp WHERE cp.user_id = @userId", new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id });

                long? docNum = _sqlCtx.GetScalar<long>("SELECT MAX(gd.doc_num) FROM doc.GeneralDocs AS gd WHERE gd.doc_type = @docType", new SqlParameter { ParameterName = "@docType", Value = 8 }); //შეკვეთა მყიდველისგან

                int? generalId = _sqlCtx.InsertGeneralDoc
                     (
                         DateTime.Now,
                         string.Empty,
                         docNum.HasValue ? docNum.Value + 1 : 0,
                         8, //შეკვეთა მყიდველისგან
                         string.Format("შეკვეთა მყიდველისგან № {0}", docNum.HasValue ? docNum.Value + 1 : 0),
                         cartProducts.Select(cp => (double)cp.Price * cp.Quantity).Sum(),
                         1, // currency_id
                         currRate,
                         18,
                         1, // user_id
                         0,
                         GeneralRepository.GetCurrentUser().Id, // contragent
                         GeneralRepository._storeId,
                         1,
                         false,
                         1,
                         1,
                         0 // staff_id TODO?
                     );

                if (generalId.HasValue)
                {
                    var result = false;

                    foreach (var cp in cartProducts)
                    {
                        result = _sqlCtx.InsertProductsFlow
                        (
                            cp.ProductId,
                            "",
                            generalId.Value,
                            cp.Quantity,
                            (double)cp.Price,
                            GeneralRepository._storeId,
                            18,
                            (double)cp.Price / 1.18,
                            -1,
                            1,
                            0,
                            0,
                            1,
                            0,
                            1,
                            "",
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            "",
                            0
                        );

                        if (!result)
                        {
                            return false;
                        }
                    }
                    
                    string payType = "1"; //დეფაულტად უნაღდო (თუ არაფერია მითითებული)
                    
                    result = _sqlCtx.InsertCustomerOrder
                    (
                        generalId.Value,
                        0,
                        1,
                        6,
                        "3",
                        "",
                        false,
                        0,
                        payType,
                        DateTime.Now,
                        0,
                        null, // staff_id
                        DateTime.Now,
                        DateTime.Now,
                        0,
                        string.Empty,
                        _sqlCtx.GetString("SELECT c.address FROM book.Contragents AS c WHERE c.id = @userId", new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id }),
                        GeneralRepository._priceId
                    );

                    if (result)
                    {
                        _sqlCtx.ExecuteSql("DELETE FROM shop.CartProducts WHERE user_id = @userId", new SqlParameter("@userId", SqlDbType.Int) { Value = GeneralRepository.GetCurrentUser().Id });
                    }

                    return result;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}