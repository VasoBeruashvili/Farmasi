using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Configuration;

namespace Farmasi.Utils
{
    public  class SqlContext : IDisposable
    {
        private bool _disposed = false;

        public string ConnectionString { get; set; }
        public int Timeout { get; set; }
        public string ErrorEx { get; private set; }


        public SqlContext(string connection_string)
        {
            ConnectionString = connection_string;
            Timeout = 1200;
        }
        public SqlContext()
            : this(ConfigurationManager.ConnectionStrings["db_connection"].ConnectionString)
        { }

        public bool IsConnectionValid()
        {
            ErrorEx = null;
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    _connection.Open();
                    _connection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorEx = ex.Message;
                return false;
            }
        }

        public int? ExecuteSql(string sql, params SqlParameter[] sql_parameters)
        {
            ErrorEx = null;
            int? _result = null;
            sql_parameters.ToList().ForEach(a => a.Value = a.Value ?? DBNull.Value);
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand _command = new SqlCommand(sql, _connection))
                    {
                        _command.CommandTimeout = Timeout;
                        _command.Parameters.AddRange(sql_parameters);
                        _connection.Open();
                        _result = _command.ExecuteNonQuery();
                        _connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _result = null;
                ErrorEx = ex.Message;
            }
            return _result;
        }

        public List<T> GetList<T>(string sql, params SqlParameter[] sql_parameters)
        {
            ErrorEx = string.Empty;
            List<T> _result = new List<T>();
            sql_parameters.ToList().ForEach(a => a.Value = a.Value ?? DBNull.Value);
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand _command = new SqlCommand(sql, _connection))
                    {
                        _command.CommandTimeout = Timeout;
                        _command.Parameters.AddRange(sql_parameters);
                        _connection.Open();
                        using (SqlDataReader _reader = _command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            if (typeof(T).IsValueType || typeof(T) == typeof(string))
                            {
                                while (_reader.Read())
                                    _result.Add((T)_reader[0]);
                            }
                            else
                            {
                                T obj = default(T);
                                var _columns = _reader.GetSchemaTable().AsEnumerable().Select(r => r.Field<string>("ColumnName")).ToList();
                                var _existing = Activator.CreateInstance<T>().GetType().GetProperties().Where(a => _columns.Contains(a.Name, StringComparer.InvariantCultureIgnoreCase));
                                while (_reader.Read())
                                {
                                    obj = Activator.CreateInstance<T>();
                                    foreach (PropertyInfo prop in _existing)
                                    {
                                        if (!Equals(_reader[prop.Name], DBNull.Value))
                                            prop.SetValue(obj, _reader[prop.Name], null);
                                    }
                                    _result.Add(obj);
                                }
                            }
                        }
                        _connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _result = null;
                ErrorEx = ex.Message;
            }
            return _result;
        }

        public List<Dictionary<string, object>> GetTableDictionary(string sql, params SqlParameter[] sql_parameters)
        {
            ErrorEx = string.Empty;
            List<Dictionary<string, object>> _result = new List<Dictionary<string, object>>();
            sql_parameters.ToList().ForEach(a => a.Value = a.Value ?? DBNull.Value);
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand _command = new SqlCommand(sql, _connection))
                    {
                        _command.CommandTimeout = Timeout;
                        _command.Parameters.AddRange(sql_parameters);
                        _connection.Open();
                        using (SqlDataReader _reader = _command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            var cols = _reader.GetSchemaTable().AsEnumerable().Select(r => r["ColumnName"]);
                            while (_reader.Read())
                            {
                                Dictionary<string, object> item = new Dictionary<string, object>();
                                foreach (var _c in cols)
                                    item.Add(_c.ToString(), _reader[_c.ToString()]);
                                _result.Add(item);
                            }
                        }
                        _connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _result = null;
                ErrorEx = ex.Message;
            }
            return _result;
        }

        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string sql, params SqlParameter[] sql_parameters)
        {
            ErrorEx = string.Empty;
            Dictionary<TKey, TValue> _result = new Dictionary<TKey, TValue>();
            sql_parameters.ToList().ForEach(a => a.Value = a.Value ?? DBNull.Value);
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand _command = new SqlCommand(sql, _connection))
                    {
                        _command.CommandTimeout = Timeout;
                        _command.Parameters.AddRange(sql_parameters);
                        _connection.Open();
                        using (SqlDataReader _reader = _command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (_reader.Read())
                            {
                                if (!_result.ContainsKey((TKey)_reader[0]))
                                    _result.Add((TKey)_reader[0], (TValue)_reader[1]);
                            }
                        }
                        _connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _result = null;
                ErrorEx = ex.Message;
            }
            return _result;
        }

        public KeyValuePair<TKey, TValue>? GetKeyValuePair<TKey, TValue>(string sql, params SqlParameter[] sql_parameters)
        {
            ErrorEx = string.Empty;
            KeyValuePair<TKey, TValue>? _result = new KeyValuePair<TKey, TValue>?();
            sql_parameters.ToList().ForEach(a => a.Value = a.Value ?? DBNull.Value);
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand _command = new SqlCommand(sql, _connection))
                    {
                        _command.CommandTimeout = Timeout;
                        _command.Parameters.AddRange(sql_parameters);
                        _connection.Open();
                        using (SqlDataReader _reader = _command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (_reader.Read())
                            {
                                _result = new KeyValuePair<TKey, TValue>((TKey)_reader[0], (TValue)_reader[1]);
                                break;
                            }
                        }
                        _connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _result = null;
                ErrorEx = ex.Message;
            }
            return _result;
        }

        public T? GetScalar<T>(string sql, params SqlParameter[] sql_parameters) where T : struct
        {
            ErrorEx = null;
            T? _result = null;
            sql_parameters.ToList().ForEach(a => a.Value = a.Value ?? DBNull.Value);
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand _command = new SqlCommand(sql, _connection))
                    {
                        _command.CommandTimeout = Timeout;
                        _command.Parameters.AddRange(sql_parameters);
                        _connection.Open();
                        object _res = _command.ExecuteScalar();
                        _connection.Close();
                        if (_res != null)
                            _result = (T)Convert.ChangeType(_res, typeof(T));
                    }
                }
            }
            catch (Exception ex)
            {
                _result = null;
                ErrorEx = ex.Message;
            }
            return _result;
        }

        public string GetString(string sql, params SqlParameter[] sql_parameters)
        {
            ErrorEx = string.Empty;
            string _result = null;
            sql_parameters.ToList().ForEach(a => a.Value = a.Value ?? DBNull.Value);
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand _command = new SqlCommand(sql, _connection))
                    {
                        _command.CommandTimeout = Timeout;
                        _command.Parameters.AddRange(sql_parameters);
                        _connection.Open();
                        object _res = _command.ExecuteScalar();
                        _connection.Close();
                        if (_res != null)
                            _result = Convert.ToString(_res);
                    }
                }
            }
            catch (Exception ex)
            {
                _result = null;
                ErrorEx = ex.Message;
            }
            return _result;
        }

        public DataTable GetTableData(string sql, params SqlParameter[] sql_parameters)
        {
            ErrorEx = string.Empty;
            DataTable _result = new DataTable("temp");
            sql_parameters.ToList().ForEach(a => a.Value = a.Value ?? DBNull.Value);
            try
            {
                using (SqlConnection _connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand _command = new SqlCommand(sql, _connection))
                    {
                        _command.CommandTimeout = Timeout;
                        _command.Parameters.AddRange(sql_parameters);
                        _connection.Open();
                        using (SqlDataReader _reader = _command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            _result.Load(_reader);
                        }
                        _connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _result = null;
                ErrorEx = ex.Message;
            }
            return _result;
        }



        #region FINA Logic
        public int? InsertGeneralDoc(DateTime tDate, string docNumPrefix, long docNum, int? docType, string purpose, double? amount, int? currencyId, double? rate, double? vat, int? userId, int? refId, int? paramId1, int? paramId2, int? statusId, bool makeEntry, int projectId, int houseId, int staffId)
        {
            if (amount > 100000000)
                amount = 0;

            string sql = @"INSERT INTO doc.GeneralDocs
                           (tdate, doc_num_prefix ,doc_num,  doc_type, purpose ,amount ,currency_id ,rate, vat, user_id, ref_id, param_id1, param_id2, status_id ,make_entry, project_id, house_id, staff_id, uid)
                           VALUES 
                           (@tDate, @docNumPrefix, @docNum, @docType, @purpose, @amount, @currencyId, @rate, @vat, @userId, @refId, @paramId1, @paramId2, @statusId, @makeEntry, @projectId, @houseId, @staffId, @uId)
                           SELECT SCOPE_IDENTITY()";

            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter { ParameterName = "@tDate", Value = tDate.ToString("yyyy-MM-dd HH:mm:ss.fff") },
                new SqlParameter { ParameterName = "@docNumPrefix", Value = docNumPrefix },
                new SqlParameter { ParameterName = "@docNum", Value = GetObjectSqlString(docNum) },
                new SqlParameter { ParameterName = "@docType", Value = GetObjectSqlString(docType) },
                new SqlParameter { ParameterName = "@purpose", Value = purpose },
                new SqlParameter { ParameterName = "@amount", Value = amount.ToString().Replace(',', '.') },
                new SqlParameter { ParameterName = "@currencyId", Value = GetObjectSqlString(currencyId) },
                new SqlParameter("@rate", SqlDbType.Float) { Value = rate },
                new SqlParameter { ParameterName = "@vat", Value = GetObjectSqlString(vat) },
                new SqlParameter { ParameterName = "@userId", Value = userId },
                new SqlParameter { ParameterName = "@refId", Value = GetObjectSqlString(refId) },
                new SqlParameter { ParameterName = "@paramId1", Value = GetObjectSqlString(paramId1) },
                new SqlParameter { ParameterName = "@paramId2", Value = GetObjectSqlString(paramId2) },
                new SqlParameter { ParameterName = "@statusId", Value = GetObjectSqlString(statusId) },
                new SqlParameter { ParameterName = "@makeEntry", Value = GetObjectSqlString(Convert.ToInt32(makeEntry)) },
                new SqlParameter { ParameterName = "@projectId", Value = projectId },
                new SqlParameter { ParameterName = "@houseId", Value = houseId },
                new SqlParameter { ParameterName = "@staffId", Value = staffId },
                new SqlParameter { ParameterName = "@uId", Value =  GetGuid() }
            };

            return GetScalar<int>(sql, sqlParams);
        }

        string GetObjectSqlString(object obj)
        {
            if (obj == null)
                return "NULL";

            if (obj is string)
                return "'" + obj.ToString() + "'";

            if (obj is DateTime? || obj is DateTime)
                return "'" + Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";

            return obj.ToString();
        }

        string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public bool InsertProductsFlow(int productId, string productTreePath, int generalId, double? amount, double? price, int? storeId, decimal? vatPercent, double? selfCost, int? coeff, byte? isOrder, byte? isExpense, byte? isMove, byte? visible, int? refId, int? unitId, string comment, double? discountPercent, int? inId, int? vendorId, int? outId, int? subId, double? discountValue, double? originalPrice, string refUid, double excise)
        {
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter { ParameterName = "@productId", Value = productId },
                new SqlParameter { ParameterName = "@productTreePath", Value =  GetObjectSqlString(productTreePath) },
                new SqlParameter { ParameterName = "@generalId", Value = generalId },
                new SqlParameter { ParameterName = "@amount", Value = amount.ToString().Replace(',', '.') },
                new SqlParameter { ParameterName = "@price", Value = price.ToString().Replace(',', '.') },
                new SqlParameter { ParameterName = "@storeId", Value = GetObjectSqlString(storeId) },
                new SqlParameter { ParameterName = "@vatPercent", Value = GetObjectSqlString(vatPercent) },
                new SqlParameter { ParameterName = "@selfCost", Value = selfCost.ToString().Replace(',', '.') },
                new SqlParameter { ParameterName = "@coeff", Value = GetObjectSqlString(coeff) },
                new SqlParameter { ParameterName = "@isOrder", Value = GetObjectSqlString(isOrder) },
                new SqlParameter { ParameterName = "@isExpense", Value = GetObjectSqlString(isExpense) },
                new SqlParameter { ParameterName = "@isMove", Value = GetObjectSqlString(isMove) },
                new SqlParameter { ParameterName = "@visible", Value = GetObjectSqlString(visible) },
                new SqlParameter { ParameterName = "@refId", Value = GetObjectSqlString(refId) },
                new SqlParameter { ParameterName = "@unitId", Value = GetObjectSqlString(unitId) },
                new SqlParameter { ParameterName = "@comment", Value = GetObjectSqlString(comment) },
                new SqlParameter { ParameterName = "@discountPercent", Value = discountPercent.ToString().Replace(',', '.') },
                new SqlParameter { ParameterName = "@inId", Value = GetObjectSqlString(inId) },
                new SqlParameter { ParameterName = "@vendorId", Value =  GetObjectSqlString(vendorId) },
                new SqlParameter { ParameterName = "@outId", Value =  GetObjectSqlString(outId) },
                new SqlParameter { ParameterName = "@subId", Value =  GetObjectSqlString(subId) },
                new SqlParameter { ParameterName = "@discountValue", Value =  discountValue.ToString().Replace(',', '.') },
                new SqlParameter { ParameterName = "@originalPrice", Value =  originalPrice.ToString().Replace(',', '.') },
                new SqlParameter { ParameterName = "@uid", Value = GetGuid() },
                new SqlParameter { ParameterName = "@refUid", Value =  GetObjectSqlString(refUid) },
                new SqlParameter { ParameterName = "@excise", Value =  excise.ToString().Replace(',', '.') }
            };

            int? result = ExecuteSql(@"INSERT INTO doc.ProductsFlow (product_id,  product_tree_path, general_id, amount, price, store_id, self_cost, vat_percent, unit_id, coeff, is_order, 
                                       is_expense, is_move,visible, ref_id, ref_uid, discount_percent, discount_value, original_price, comment, in_id, vendor_id, out_id, sub_id, uid, excise)
                                       VALUES
                                       (@productId,  @productTreePath, @generalId, @amount, @price, @storeId, @selfCost, @vatPercent, @unitId, @coeff, @isOrder, 
                                       @isExpense, @isMove, @visible, @refId, @refUid, @discountPercent, @discountValue, @originalPrice, @comment, @inId, @vendorId, @outId, @subId, @uid, @excise)", sqlParams);

            return result.HasValue ? result.Value > 0 : false;
        }

        public bool InsertCustomerOrder(int generalId, double? discountPercent, byte? makeReserve, int? invoiceBankId, string invoiceTerm, string invoiceNum, bool? discountCheck, int? liveId, string payType, DateTime? payDate, int? agreementId, int? staffId, DateTime? deliveryDate, DateTime? reserveDate, int? chekStatus, string transpStartPlace, string transpEndPlace, int? priceType)
        {
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter { ParameterName = "@generalId", Value = generalId },
                new SqlParameter { ParameterName = "@discountPercent", Value =  discountPercent.ToString().Replace(',', '.') },
                new SqlParameter { ParameterName = "@makeReserve", Value = makeReserve },
                new SqlParameter { ParameterName = "@invoiceBankId", Value = invoiceBankId },
                new SqlParameter { ParameterName = "@invoiceTerm", Value = invoiceTerm },
                new SqlParameter { ParameterName = "@invoiceNum", Value = invoiceNum },
                new SqlParameter { ParameterName = "@discountCheck", Value = discountCheck },
                new SqlParameter { ParameterName = "@liveId", Value = liveId },
                new SqlParameter { ParameterName = "@payType", Value = payType },
                new SqlParameter { ParameterName = "@payDate", Value = payDate },
                new SqlParameter { ParameterName = "@agreementId", Value = agreementId },
                new SqlParameter { ParameterName = "@staffId", Value = staffId },
                new SqlParameter { ParameterName = "@deliveryDate", Value = deliveryDate },
                new SqlParameter { ParameterName = "@reserveDate", Value = reserveDate },
                new SqlParameter { ParameterName = "@chekStatus", Value = chekStatus },
                new SqlParameter { ParameterName = "@transpStartPlace", Value = transpStartPlace },
                new SqlParameter { ParameterName = "@transpEndPlace", Value = transpEndPlace },
                new SqlParameter { ParameterName = "@priceType", Value = priceType }
            };

            int? result = ExecuteSql(@"INSERT INTO doc.CustomerOrders (general_id,discount_percent,make_reserve, invoice_bank_id,invoice_term,invoice_num,discount_check,live_id, pay_type, pay_date, agreement_id, staff_id, delivery_date, reserve_date, chek_status, transp_start_place, transp_end_place, price_type)
                                       VALUES(@generalId, @discountPercent, @makeReserve, @invoiceBankId, @invoiceTerm, @invoiceNum, @discountCheck, @liveId, @payType, @payDate, @agreementId, @staffId, @deliveryDate, @reserveDate, @chekStatus, @transpStartPlace, @transpEndPlace, @priceType)", sqlParams);

            return result.HasValue ? result.Value > 0 : false;
        }
        #endregion



        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                ConnectionString = null;
                SqlConnection.ClearAllPools();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SqlContext()
        {
            Dispose(false);
        }
    }
}