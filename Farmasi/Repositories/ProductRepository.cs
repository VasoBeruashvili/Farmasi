using Farmasi.Utils;
using Farmasi.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Farmasi.Repositories
{
    public static class ProductRepository
    {
        static List<ContragentRelationViewModel> contragentRelations { get; set; }
        static Dictionary<decimal, decimal> SalePercentages = new Dictionary<decimal, decimal>()
        {
            { 16000, 22 },
            { 12000, 18 },
            { 9000, 15 },
            { 6000, 12 },
            { 3000, 9 },
            { 1200, 6 },
            { 300, 3 }
        };


        public static List<ProductViewModel> GetNewlyAddedProducts()
        {
            using(SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetList<ProductViewModel>(@"SELECT TOP 10 p.id, p.name, (SELECT TOP 1 pi.img FROM book.ProductImages AS pi WHERE pi.product_id = p.id) AS image
                                                                     FROM book.Products AS p ORDER BY p.id DESC");
            }
        }

        public static List<GroupProductViewModel> GetGroupProducts()
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                List<GroupProductViewModel> rootGroupProducts = _sqlCtx.GetList<GroupProductViewModel>("SELECT id, parentid, path, name FROM book.GroupProducts WHERE parentid = @parentId ORDER BY order_id", new SqlParameter() { ParameterName = "@parentId", Value = "11" });
                if (rootGroupProducts != null)
                {
                    rootGroupProducts.ForEach(rgp =>
                    {
                        rgp.Children = FillGroupProductChildren(rgp.Id);
                        rgp.ProductQuantity = CalculateGroupProductQuantity(rgp.Path);
                    });
                }

                return rootGroupProducts;
            }
        }

        static List<GroupProductViewModel> FillGroupProductChildren(int id)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                List<GroupProductViewModel> groupProducts = _sqlCtx.GetList<GroupProductViewModel>("SELECT id, parentid, path, name FROM book.GroupProducts WHERE parentId = @parentId ORDER BY order_id", new SqlParameter() { ParameterName = "@parentId", Value = id });

                if (groupProducts != null)
                {
                    groupProducts.ForEach(gp =>
                    {
                        gp.Children = FillGroupProductChildren(gp.Id);
                        gp.ProductQuantity = CalculateGroupProductQuantity(gp.Path);
                    });
                }

                return groupProducts;
            }
        }

        static string CalculateGroupProductQuantity(string path)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetString("SELECT COUNT(p.id) AS productQuantity FROM book.Products AS p INNER JOIN book.GroupProducts AS gp ON gp.id = p.group_id WHERE gp.path LIKE '" + path + "%'");
            }
        }

        public static List<ProductViewModel> GetProducts(int groupProductId)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                string path = _sqlCtx.GetString("SELECT path FROM book.GroupProducts WHERE id = @groupProductId", new SqlParameter("@groupProductId", SqlDbType.Int) { Value = groupProductId });
                return _sqlCtx.GetList<ProductViewModel>("SELECT p.id, p.code, p.name, p.path, pp.manual_val AS price, (SELECT TOP 1 pi.img FROM book.ProductImages AS pi WHERE pi.product_id = p.id) AS image FROM book.Products AS p INNER JOIN book.ProductPrices AS pp ON pp.product_id = p.id AND pp.price_id = " + GeneralRepository._priceId + " WHERE p.path LIKE '" + path + "%'");

                //return _sqlCtx.GetList<ProductViewModel>(@"DECLARE @store_id INT=" + GeneralRepository._storeId + @"; WITH rest_data(product_id, rest) AS
                //                                        (
                //                                            SELECT fl.product_id, ISNULL(SUM(fl.amount * CASE fl.is_order WHEN 1 THEN -1 WHEN 0 THEN fl.coeff END),0)  AS rest
                //                                            FROM book.Products AS p
                //                                            INNER JOIN doc.ProductsFlow AS fl ON fl.product_id=p.id
                //                                            INNER JOIN doc.GeneralDocs AS g ON g.id=fl.general_id
                //                                            INNER JOIN book.GroupProducts AS gp ON gp.id = p.group_id
                //                                            WHERE g.tdate <= GETDATE() AND ISNULL(g.is_deleted,0)=0  AND fl.is_order IN(0,1)  AND fl.is_expense=0 AND @store_id=fl.store_id AND p.path LIKE '0#1#10%'
                //                                            GROUP BY fl.product_id
                //                                        )
                //                                        SELECT t1.id, t1.group_id AS groupId, t1.code, t1.name, t1.comment, u.full_name AS unitFullName, (SELECT TOP 1 pi.img FROM book.ProductImages AS pi WHERE pi.product_id = t1.id) AS image, t0.rest, CAST(ROUND(pp.manual_val, 2) AS FLOAT) AS price, c.code AS currency FROM rest_data AS t0 INNER JOIN book.Products AS t1 ON t1.id=t0.product_id
                //                                        INNER JOIN book.ProductPrices AS pp
                //                                        ON pp.product_id = t1.id AND pp.price_id = @priceId
                //                                        INNER JOIN book.GroupProducts AS gp ON gp.id = t1.group_id
                //                                        INNER JOIN book.Currencies AS c
                //                                        ON pp.manual_currency_id = c.id
                //                                        INNER JOIN book.Units AS u
                //                                        ON u.id = t1.unit_id
                //                                        WHERE t1.path LIKE '" + path + "%'", new SqlParameter("@priceId", SqlDbType.Int) { Value = GeneralRepository._priceId });
            }
        }
                
        public static ProductViewModel GetProductDetails(int productId)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                SqlParameter[] _sqlParams = new SqlParameter[]
                {
                    new SqlParameter("@productId", SqlDbType.Int) { Value = productId },
                    new SqlParameter("@priceId", SqlDbType.Int) { Value = GeneralRepository._priceId },
                    new SqlParameter("@store_id", SqlDbType.Int) { Value = GeneralRepository._storeId }
                };

                ProductViewModel product = _sqlCtx.GetList<ProductViewModel>(@";WITH rest_data(product_id, rest) AS
                                                                            (
                                                                                SELECT fl.product_id, ISNULL(SUM(fl.amount * CASE fl.is_order WHEN 1 THEN -1 WHEN 0 THEN fl.coeff END),0)  AS rest
                                                                                FROM book.Products AS p
                                                                                INNER JOIN doc.ProductsFlow AS fl ON fl.product_id=p.id
                                                                                INNER JOIN doc.GeneralDocs AS g ON g.id=fl.general_id
                                                                                INNER JOIN book.GroupProducts AS gp ON gp.id = p.group_id
                                                                                WHERE g.tdate <= GETDATE() AND ISNULL(g.is_deleted,0)=0  AND fl.is_order IN(0,1)  AND fl.is_expense=0 AND @store_id=fl.store_id AND p.path LIKE '0#1#10%'
                                                                                GROUP BY fl.product_id
                                                                            )
                                                                            SELECT t1.id, t1.group_id AS groupId, t1.code, t1.name, t1.usr_column_504 AS colorGroupNumber, t1.comment, u.full_name AS unitFullName, (SELECT TOP 1 pi.img FROM book.ProductImages AS pi WHERE pi.product_id = t1.id) AS image, t0.rest, CAST(ROUND(pp.manual_val, 2) AS FLOAT) AS price, c.code AS currency FROM rest_data AS t0 INNER JOIN book.Products AS t1 ON t1.id=t0.product_id
                                                                            INNER JOIN book.ProductPrices AS pp
                                                                            ON pp.product_id = t1.id AND pp.price_id = @priceId
                                                                            INNER JOIN book.GroupProducts AS gp ON gp.id = t1.group_id
                                                                            INNER JOIN book.Currencies AS c
                                                                            ON pp.manual_currency_id = c.id
                                                                            INNER JOIN book.Units AS u
                                                                            ON u.id = t1.unit_id
                                                                            WHERE t1.id = @productId", _sqlParams).FirstOrDefault();

                if (product != null)
                {
                    product.Images = _sqlCtx.GetList<ProductImageViewModel>("SELECT pi.img AS image FROM book.ProductImages AS pi WHERE pi.product_id = @productId", new SqlParameter("@productId", SqlDbType.Int) { Value = productId });
                    if (!string.IsNullOrEmpty(product.ColorGroupNumber))
                    {
                        product.Colors = _sqlCtx.GetList<ProductColorViewModel>("SELECT p.id AS productId, p.usr_column_504 AS colorGroupNumber, p.usr_column_505 AS color FROM book.Products AS p WHERE p.usr_column_504 = @cgn AND p.id != @productId", new SqlParameter[] { new SqlParameter("@cgn", SqlDbType.NVarChar) { Value = product.ColorGroupNumber }, new SqlParameter("@productId", SqlDbType.NVarChar) { Value = product.Id } });
                    }
                }

                return product;
            }
        }

        public static List<ProductOutViewModel> GetProductOuts(DateTime fromDate, DateTime toDate, int? cid)
        {
            using(SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetList<ProductOutViewModel>(@"SELECT gd.id, gd.tdate, gd.doc_num AS num, gd.purpose, gd.amount, c.code AS currency
                                                            FROM doc.GeneralDocs AS gd
                                                            INNER JOIN book.Currencies AS c ON c.id = gd.currency_id
                                                            WHERE gd.param_id1 = @id AND CAST(gd.tdate AS DATE) BETWEEN @fromDate AND @toDate", new SqlParameter[]
                                                            {
                                                                new SqlParameter("@id", SqlDbType.Int) { Value = cid.HasValue ? cid.Value : GeneralRepository.GetCurrentUser().Id },
                                                                new SqlParameter("@fromDate", SqlDbType.DateTime) { Value = fromDate },
                                                                new SqlParameter("@toDate", SqlDbType.DateTime) { Value = toDate }
                                                            });
            }
        }



        static decimal GetChildrenSales(int parentId)
        {
            if (contragentRelations == null || contragentRelations.Count == 0)
                return 0;

            List<ContragentRelationViewModel> _children = new List<ContragentRelationViewModel>(contragentRelations.Where(p => p.ParentId == parentId && p.Id != p.ParentId));
            if (_children == null || _children.Count == 0)
                return 0;

            decimal _amount = _children.Select(p => p.Sales).DefaultIfEmpty().Sum();
            foreach (ContragentRelationViewModel _item in _children)
            {
                _amount += GetChildrenSales(_item.Id);
            }
            return _amount;
        }

        static decimal GetPercent(decimal personalSales, decimal groupSales)
        {
            if (personalSales <= 50)
                return 0;

            decimal _percent = SalePercentages.Where(p => groupSales >= p.Key).Select(p => p.Value).FirstOrDefault();
            return _percent;
        }

        static decimal GetChildrenBonus(int parentId)
        {
            if (contragentRelations == null || contragentRelations.Count == 0)
                return 0;

            List<ContragentRelationViewModel> _children = new List<ContragentRelationViewModel>(contragentRelations.Where(p => p.ParentId == parentId));
            if (_children == null || _children.Count == 0)
                return 0;

            decimal _amount = 0;
            KeyValuePair<decimal, decimal> _val = new KeyValuePair<decimal, decimal>();
            foreach (ContragentRelationViewModel _item in _children)
            {
                _val = GetBonus(_item.Id, _item.Sales, _item.GroupSales);
                _amount += _val.Key + _val.Value;
            }
            return _amount;
        }

        static KeyValuePair<decimal, decimal> GetBonus(int parentId, decimal personalSales, decimal groupSales)
        {
            if (contragentRelations == null || contragentRelations.Count == 0)
                return new KeyValuePair<decimal, decimal>(0, 0);

            decimal _amount = 0;
            decimal _bonus = 0;
            decimal _percent = SalePercentages.Where(p => groupSales >= p.Key).Select(p => p.Value).FirstOrDefault();
            List<ContragentRelationViewModel> _children = new List<ContragentRelationViewModel>(contragentRelations.Where(p => p.ParentId == parentId));
            if (_children == null)
                return new KeyValuePair<decimal, decimal>(0, 0);

            if (_children.Count == 0 && personalSales > 50 && _percent > 0)
            {
                _bonus = (groupSales / (decimal)1.18 * _percent / 100) * (decimal)0.8;
                return new KeyValuePair<decimal, decimal>(_amount, _bonus);
            }


            KeyValuePair<decimal, decimal> _val = new KeyValuePair<decimal, decimal>();
            foreach (ContragentRelationViewModel _item in _children)
            {
                _val = GetBonus(_item.Id, _item.Sales, _item.GroupSales);
                _amount += _val.Key + _val.Value;

            }
            if (_percent > 0 && personalSales > 50)
                return new KeyValuePair<decimal, decimal>(_amount, (groupSales / (decimal)1.18 * _percent / 100 - _amount) * (decimal)0.8);
            else
                return new KeyValuePair<decimal, decimal>(_amount, 0);

        }

        public static ContragentRelationViewModel GetContragentRelation(DateTime fromDate, DateTime toDate)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                string _sql = @"SELECT r.child_id AS Id, r.parent_id AS ParentId, ISNULL(c.code,'') AS Code,
				CASE r.type WHEN 0 THEN ISNULL(c.name,'') 
				    WHEN 1 THEN ISNULL(c.name,'') +N' (კოორდინატორი)'      
				    WHEN 2 THEN ISNULL(c.name,'') +N' (უფროსი კოორდინატორი)'    
				    WHEN 3 THEN ISNULL(c.name,'') +N' (მენეჯერი)'              
				    WHEN 4 THEN ISNULL(c.name,'') +N' (დირექტორი)'            
				END AS Name, ISNULL(c.tel,'') AS Tel,                     
				(SELECT CONVERT(DECIMAL(18,2), ISNULL(SUM(CASE WHEN po.discount_percent<100 THEN g.amount*100/(100-po.discount_percent) ELSE 0 END),0))
				FROM doc.GeneralDocs AS g INNER JOIN doc.ProductOut AS po ON po.general_id=g.id 
				WHERE g.doc_type=21 AND g.param_id1=c.id
				AND g.tdate BETWEEN '" + fromDate.ToString("yyyy-MM-dd HH:mm") + @"' AND '" + toDate.ToString("yyyy-MM-dd HH:mm") + @"') AS Sales,
				CAST(0 AS DECIMAL(18,2)) GroupSales,
				CAST(0 AS DECIMAL(18,2)) PersonalBonus,
				CAST(0 AS DECIMAL(18,2)) GroupBonus,   
				CAST(0 AS DECIMAL(18,2)) CoordBonus,      
				CAST(0 AS DECIMAL(18,2)) SeniorCoordBonus, 
				CAST(0 AS DECIMAL(18,2)) ManagerBonus, CAST(0 AS BIT) AS Succeeded,
				r.type    
				FROM book.ContragentRelations AS r    
				INNER JOIN book.Contragents AS p ON r.parent_id=p.id   
				INNER JOIN book.Contragents AS c ON r.child_id=c.id    
				ORDER BY r.parent_id";

                List<ContragentBonusItem> m_BonusItems = _sqlCtx.GetList<ContragentBonusItem>(_sql);
                if (m_BonusItems == null || m_BonusItems.Count == 0)
                    return null;

                // decimal _discountPercent = 0;
                List<ContragentBonusItem> _coordinators = m_BonusItems.Where(p => p.Type == 1 && p.Sales >= 50).ToList();
                List<ContragentBonusItem> _seniorCoordinators = m_BonusItems.Where(p => p.Type == 2 && p.Sales >= 50).ToList();
                List<ContragentBonusItem> _managers = m_BonusItems.Where(p => p.Type == 3 && p.Sales >= 50).ToList();
                List<ContragentBonusItem> _directors = m_BonusItems.Where(p => p.Type == 4 && p.Sales >= 50).ToList();
                List<ContragentBonusItem> _consults;
                decimal _amount = 0;

                //კოორდინატორი
                foreach (ContragentBonusItem _coordinator in _coordinators)
                {
                    decimal _discountPercent = 0;

                    // ბონუსი პირადი გაყიდვებიდან
                    _discountPercent = GetDiscountPercent(_coordinator.Sales);
                    _coordinator.PersonalBonus = _coordinator.Sales * _discountPercent / 100;

                    _consults = m_BonusItems.Where(p => p.ParentId == _coordinator.Id && p.Type == 0).ToList();
                    if (_consults.Where(p => p.Sales >= 50).Count() < 5)
                        continue;

                    _coordinator.GroupSales = _consults.Select(p => p.Sales).Sum();
                    _coordinator.GroupSales += _coordinator.Sales;
                    if (_coordinator.GroupSales < 1620)
                        continue;

                    // ბონუსი პირადი ჯგუფიდან
                    _coordinator.GroupBonus = (((_coordinator.GroupSales - _coordinator.GroupSales * 33 / 100) * m_PercentFromGroup / 100) / (decimal)1.18) * (decimal)0.8;
                    _coordinator.Succeeded = true;
                }

                //უფროსი კოორდინატორი
                foreach (ContragentBonusItem _sCoordinator in _seniorCoordinators)
                {
                    decimal _discountPercent = 0;

                    // ბონუსი პირადი გაყიდვებიდან
                    _discountPercent = GetDiscountPercent(_sCoordinator.Sales);
                    _sCoordinator.PersonalBonus = _sCoordinator.Sales * _discountPercent / 100;

                    //ბონუსი პირადი ჯგუფიდან
                    _consults = m_BonusItems.Where(p => p.ParentId == _sCoordinator.Id && p.Type == 0).ToList();
                    if (_consults.Where(p => p.Sales >= 50).Count() < 10)
                        continue;

                    _sCoordinator.GroupSales = _consults.Select(p => p.Sales).Sum();
                    _sCoordinator.GroupSales += _sCoordinator.Sales;
                    if (_sCoordinator.GroupSales < 3520)
                        continue;

                    _sCoordinator.GroupBonus = (((_sCoordinator.GroupSales - _sCoordinator.GroupSales * 33 / 100) * m_PercentFromGroup / 100) / (decimal)1.18) * (decimal)0.8;

                    //ბონუსი პირადი კოორდინატორებისგან
                    if (_coordinators.Where(p => p.ParentId == _sCoordinator.Id && !p.Succeeded).Any())
                        continue;
                    _amount = _coordinators.Where(p => p.ParentId == _sCoordinator.Id).Select(p => p.GroupSales).Sum();
                    _sCoordinator.CoordBonus = (((_amount - _amount * 33 / 100) * m_PercentFromCoords / 100) / (decimal)1.18) * (decimal)0.8;
                    _sCoordinator.Succeeded = true;
                }


                //მენეჯერი
                foreach (ContragentBonusItem _manager in _managers)
                {
                    decimal _discountPercent = 0;

                    // ბონუსი პირადი გაყიდვებიდან
                    _discountPercent = GetDiscountPercent(_manager.Sales);
                    _manager.PersonalBonus = _manager.Sales * _discountPercent / 100;

                    //ბონუსი პირადი ჯგუფიდან
                    _consults = m_BonusItems.Where(p => p.ParentId == _manager.Id && p.Type == 0).ToList();
                    if (_consults.Where(p => p.Sales >= 50).Count() < 15)
                        continue;


                    _manager.GroupSales = _consults.Select(p => p.Sales).Sum();
                    _manager.GroupSales += _manager.Sales;
                    if (_manager.GroupSales > 5120)
                        continue;
                    _manager.GroupBonus = (((_manager.GroupSales - _manager.GroupSales * 33 / 100) * m_PercentFromGroup / 100) / (decimal)1.18) * (decimal)0.8;

                    //ბონუსი უფროსი კოორდინატორებისგან
                    if (_seniorCoordinators.Where(p => p.ParentId == _manager.Id && !p.Succeeded).Any())
                        continue;

                    _amount = _seniorCoordinators.Where(p => p.ParentId == _manager.Id).Select(p => p.GroupSales).Sum();
                    _manager.SeniorCoordBonus = (((_amount - _amount * 33 / 100) * m_PercentFromSeniorCoords / 100) / (decimal)1.18) * (decimal)0.8;

                    //პირადი კოორდინატორები
                    if (_coordinators.Where(p => p.ParentId == _manager.Id && !p.Succeeded).Any())
                        continue;

                    _amount = _coordinators.Where(p => p.ParentId == _manager.Id).Select(p => p.GroupSales).Sum();

                    //უფროსი კოორდინატორების კოორდინატორები
                    List<int> _seniorCoordIds = _seniorCoordinators.Where(p => p.ParentId == _manager.Id).Select(p => p.Id).ToList();
                    if (_coordinators.Where(p => _seniorCoordIds.Contains(p.ParentId) && !p.Succeeded).Any())
                        continue;

                    _amount += _coordinators.Where(p => _seniorCoordIds.Contains(p.ParentId)).Select(p => p.GroupSales).Sum();
                    _manager.CoordBonus = (((_amount - _amount * 33 / 100) * m_PercentFromCoords / 100) / (decimal)1.18) * (decimal)0.8;
                    _manager.Succeeded = true;
                }

                //დირექტორი
                foreach (ContragentBonusItem _director in _directors)
                {
                    decimal _discountPercent = 0;

                    // ბონუსი პირადი გაყიდვებიდან
                    _discountPercent = GetDiscountPercent(_director.Sales);
                    _director.PersonalBonus = _director.Sales * _discountPercent / 100;

                    //ბონუსი პირადი ჯგუფიდან
                    _consults = m_BonusItems.Where(p => p.ParentId == _director.Id && p.Type == 0).ToList();
                    if (_consults.Where(p => p.Sales > 50).Count() < 40)
                        continue;


                    _director.GroupSales = _consults.Select(p => p.Sales).Sum();
                    _director.GroupSales += _director.Sales;
                    if (_director.GroupSales < 12300)
                        continue;

                    _director.GroupBonus = (((_director.GroupSales - _director.GroupSales * 33 / 100) * m_PercentFromGroup / 100) / (decimal)1.18) * (decimal)0.8;


                    //ბონუსი მენეჯერებისგან
                    if (_managers.Where(p => p.ParentId == _director.Id && !p.Succeeded).Any())
                        continue;

                    _amount = _managers.Where(p => p.ParentId == _director.Id).Select(p => p.GroupSales).Sum();
                    _director.ManagerBonus = (((_amount - _amount * 33 / 100) * m_PercentFromManagers / 100) / (decimal)1.18) * (decimal)0.8;


                    //პირადი უფროსი კოორდინატორები
                    List<ContragentBonusItem> _seniors = _seniorCoordinators.Where(p => p.ParentId == _director.Id).ToList();
                    if (_seniors.Where(p => !p.Succeeded).Any())
                        continue;

                    //მენეჯერების უფროსი კოორდინატორები
                    List<int> _mIds = _managers.Where(p => p.ParentId == _director.Id).Select(p => p.Id).ToList();
                    List<ContragentBonusItem> _managerSeniors = _seniorCoordinators.Where(p => _mIds.Contains(p.ParentId)).ToList();
                    if (_managerSeniors.Where(p => !p.Succeeded).Any())
                        continue;

                    _amount = _seniors.Select(p => p.GroupSales).Sum() + _managerSeniors.Select(p => p.GroupSales).Sum();
                    _director.SeniorCoordBonus = (((_amount - _amount * 33 / 100) * m_PercentFromSeniorCoords / 100) / (decimal)1.18) * (decimal)0.8;

                    //პირადი კოორდინატორები
                    List<ContragentBonusItem> _coors = _coordinators.Where(p => p.ParentId == _director.Id).ToList();
                    if (_coors.Where(p => !p.Succeeded).Any())
                        continue;

                    //პირადი მენეჯერების კოორდინატორები
                    List<ContragentBonusItem> _managerCoords = _coordinators.Where(p => _mIds.Contains(p.ParentId)).ToList();
                    if (_managerCoords.Where(p => !p.Succeeded).Any())
                        continue;

                    //პირადი უფროსი კოორდინატორების კოორდინატორები
                    List<int> _sIds = _seniors.Select(p => p.Id).ToList();
                    List<ContragentBonusItem> _seniorCoordCoords = _coordinators.Where(p => _sIds.Contains(p.ParentId)).ToList();
                    if (_seniorCoordCoords.Where(p => !p.Succeeded).Any())
                        continue;

                    //პირადი მენეჯერების უფროსი კოორდინატორების კოორდინატორები
                    _sIds = _managerSeniors.Select(p => p.Id).ToList();
                    List<ContragentBonusItem> _managerSeniorCoordCoords = _coordinators.Where(p => _sIds.Contains(p.ParentId)).ToList();
                    if (_managerSeniorCoordCoords.Where(p => !p.Succeeded).Any())
                        continue;

                    _amount = _coors.Select(p => p.GroupSales).Sum() + _managerCoords.Select(p => p.GroupSales).Sum() + _seniorCoordCoords.Select(p => p.GroupSales).Sum() + _managerSeniorCoordCoords.Select(p => p.GroupSales).Sum();

                    _director.CoordBonus = (((_amount - _amount * 33 / 100) * m_PercentFromCoords / 100) / (decimal)1.18) * (decimal)0.8;
                    _director.Succeeded = true;
                }

                ContragentBonusItem g = m_BonusItems.FirstOrDefault(cr => cr.Id == GeneralRepository.GetCurrentUser().Id);

                return new ContragentRelationViewModel
                {
                    Sales = Math.Round(g.Sales, 2),
                    GroupSales = Math.Round(g.GroupSales, 2),
                    GroupBonus = Math.Round(g.GroupBonus, 2),
                    ParentId = g.ParentId,
                    Bonus = Math.Round(GetBonus(g.Id, g.Sales, g.GroupSales).Value, 2),
                    GroupBonusWithoutVat = Math.Round(g.GroupSales / (decimal)1.18, 2),
                    GroupPayBonus = Math.Round(GetChildrenBonus(g.Id), 2),
                    Percent = GetPercent(g.Sales, g.GroupSales)
                };

            }
        }


        private static decimal GetDiscountPercent(decimal amount)
        {
            if (amount >= 0 && amount <= 49)
                return 0;
            if (amount >= 50 && amount <= 74)
                return 10;
            if (amount >= 75 && amount <= 149)
                return 20;
            if (amount >= 150 && amount <= 269)
                return 28;
            if (amount >= 270 && amount <= 499)
                return 33;
            if (amount >= 500)
                return 37;
            return 0;
        }



        private static decimal m_PercentFromGroup = 10;
        private static decimal m_PercentFromCoords = 12;
        private static decimal m_PercentFromSeniorCoords = 11;
        private static decimal m_PercentFromManagers = 10;

        public class ContragentBonusItem
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Tel { get; set; }
            public decimal Sales { get; set; }
            public decimal GroupSales { get; set; }
            public decimal PersonalBonus { get; set; }
            public decimal GroupBonus { get; set; }
            public decimal CoordBonus { get; set; }
            public decimal SeniorCoordBonus { get; set; }
            public decimal ManagerBonus { get; set; }
            public bool Succeeded { get; set; }
            public byte Type { get; set; }
        }



        public static List<CatalogProductViewModel> SearchCatalogProducts(string code)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                string path = _sqlCtx.GetString("SELECT path FROM book.GroupProducts WHERE id = @groupProductId", new SqlParameter("@groupProductId", SqlDbType.NVarChar) { Value = code });
                return _sqlCtx.GetList<CatalogProductViewModel>("SELECT p.id, p.code, p.name, p.path, (SELECT TOP 1 pi.img FROM book.ProductImages AS pi WHERE pi.product_id = p.id) AS image FROM book.Products AS p WHERE p.code LIKE '" + code + "%'");
            }
        }
    }
}