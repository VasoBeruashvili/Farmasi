using Farmasi.Utils;
using Farmasi.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Farmasi.Repositories
{
    public static class StructureRepository
    {
        static List<RelClient> m_Clients = new List<RelClient>();

        public static void FillTypes(bool isGenerate)
        {
            using (SqlContext _sqlCtx = new SqlContext())
            {
                if(isGenerate)
                    _sqlCtx.ExecuteSql("UPDATE book.ContragentRelations SET type=0");

                string _sql = @"SELECT r.id, r.child_id AS ChildId, r.parent_id AS ParentId, r.path, ISNULL(c.name,'') AS Name,
                        (SELECT COUNT(rr.id) FROM book.ContragentRelations AS rr 
                            WHERE rr.parent_id=r.child_id AND 
                        (SELECT COUNT(id) FROM book.ContragentRelations WHERE parent_id=rr.child_id)<5 ) AS ChildrenCount, r.type, CAST(0 AS BIT) AS IsChanged
                        FROM book.ContragentRelations AS r 
                        INNER JOIN book.Contragents AS p ON r.parent_id=p.id
                        INNER JOIN book.Contragents AS c ON r.child_id=c.id
                        WHERE (SELECT COUNT(id) FROM book.ContragentRelations WHERE parent_id=r.child_id)>=5
                        ORDER BY LEN(r.path) DESC";

                m_Clients = _sqlCtx.GetList<RelClient>(_sql);
                if (m_Clients == null || m_Clients.Count == 0)
                {
                    return;
                }

                int _count = 0;

                foreach (RelClient _c in m_Clients)
                {
                    _count = m_Clients.Where(c => c.ParentId == _c.ChildId && c.ChildrenCount >= 5).Count();
                    if (_c.ChildrenCount >= 5)
                    {
                        if (_c.Type != 1)
                        {
                            _c.Type = 1;
                            _c.IsChanged = true;
                        }
                    }
                }

                foreach (RelClient _c in m_Clients)
                {
                    _count = m_Clients.Where(c => c.ParentId == _c.ChildId && c.Type == 1).Count();
                    if (_c.ChildrenCount >= 10 && _count >= 2)
                    {
                        if (_c.Type != 2)
                        {
                            _c.Type = 2;
                            _c.IsChanged = true;
                        }
                    }

                }


                foreach (RelClient _c in m_Clients)
                {
                    if (_c.ChildrenCount < 15)
                        continue;

                    _count = m_Clients.Where(c => c.ParentId == _c.ChildId && c.Type == 2).Count();
                    if (_count >= 2)
                    {
                        if (_c.Type != 3)
                        {
                            _c.Type = 3;
                            _c.IsChanged = true;
                        }

                    }
                }

                foreach (RelClient _c in m_Clients)
                {
                    if (_c.ChildrenCount < 40)
                        continue;

                    _count = m_Clients.Where(c => c.ParentId == _c.ChildId && c.Type == 3).Count();
                    if (_count >= 2)
                    {
                        if (_c.Type != 4)
                        {
                            _c.Type = 4;
                            _c.IsChanged = true;
                        }
                    }
                }

                if (isGenerate)
                {
                    List<RelClient> _changedClients = m_Clients.Where(c => c.IsChanged).ToList();
                    foreach (RelClient _cc in _changedClients)
                    {
                        _sql = "UPDATE book.ContragentRelations SET type=" + _cc.Type + " WHERE id=" + _cc.Id;
                        _sqlCtx.ExecuteSql(_sql);
                    }
                }
            }
        }

        public static bool FillRelations(RelationTreeNodeViewModel node, int id)
        {
            FillTypes(false);

            string sqlText = "";

            using(SqlContext _sqlCtx = new SqlContext())
            {
                if (id != 0)
                {
                    sqlText = @"SELECT r.*, c.name, c.id AS cid, (COALESCE(d.debetCycle,0) - COALESCE(d.creditCycle,0)) AS amount, 
                                ISNULL(c.tel,'') AS Tel FROM book.ContragentRelations AS r 
                                INNER JOIN book.Contragents AS c ON r.child_id=c.id
                                OUTER APPLY
                                (SELECT
		                            ROUND((SELECT SUM(e.amount*e.rate) FROM doc.Entries e INNER JOIN doc.GeneralDocs g ON e.general_id = g.id WHERE e.a1=c.id AND ISNULL(g.is_deleted,0)=0 AND (e.debit_acc = c.account OR e.debit_acc =  c.account2 )),3) AS debetCycle,
		                            ROUND((SELECT SUM(e.amount*e.rate) FROM doc.Entries e INNER JOIN doc.GeneralDocs g ON e.general_id = g.id WHERE e.b1=c.id AND ISNULL(g.is_deleted,0)=0 AND (e.credit_acc = c.account OR e.credit_acc = c.account2 )),3) AS creditCycle
                                ) AS d
                                WHERE r.parent_id = " + id.ToString() + "ORDER BY r.id";
                }
                else
                {
                    sqlText = @"SELECT r.*, c.name, c.id AS cid, (COALESCE(d.debetCycle,0) - COALESCE(d.creditCycle,0)) AS amount, 
                                ISNULL(c.tel,'') AS Tel FROM book.ContragentRelations AS r 
                                INNER JOIN book.Contragents AS c ON r.child_id=c.id
                                OUTER APPLY
                                (SELECT
		                            ROUND((SELECT SUM(e.amount*e.rate) FROM doc.Entries e INNER JOIN doc.GeneralDocs g ON e.general_id = g.id WHERE e.a1=c.id AND ISNULL(g.is_deleted,0)=0 AND (e.debit_acc = c.account OR e.debit_acc =  c.account2 )),3) AS debetCycle,
		                            ROUND((SELECT SUM(e.amount*e.rate) FROM doc.Entries e INNER JOIN doc.GeneralDocs g ON e.general_id = g.id WHERE e.b1=c.id AND ISNULL(g.is_deleted,0)=0 AND (e.credit_acc = c.account OR e.credit_acc = c.account2 )),3) AS creditCycle
                                ) AS d
                                WHERE r.parent_id = 0 ORDER BY r.id";

                    DataTable tableinvNodes = _sqlCtx.GetTableData(sqlText);

                    if (tableinvNodes == null)
                        return false;

                    if (tableinvNodes.Rows.Count == 0)
                        return false;

                    int idd;
                    foreach (DataRow row in tableinvNodes.Rows)
                    {

                        idd = int.Parse(row["child_id"].ToString());
                        RelationTreeNodeViewModel chNode = new RelationTreeNodeViewModel();
                        chNode.Text = row["name"].ToString();
                        chNode.Amount = Convert.ToDouble(row["amount"]);
                        chNode.Cid = Convert.ToInt32(row["cid"]);
                        if (node.Nodes == null)
                        {
                            node.Nodes = new List<RelationTreeNodeViewModel>();
                        }

                        RelClient _cc = m_Clients.Where(p => p.ChildId == idd).FirstOrDefault();
                        if (_cc != null)
                        {
                            switch (_cc.Type)
                            {
                                case 1: chNode.Text += " (კოორდინატორი)"; break;
                                case 2: chNode.Text += " (უფროსი კოორდინატორი)"; break;
                                case 3: chNode.Text += " (მენეჯერი)"; break;
                                case 4: chNode.Text += " (დირექტორი)"; break;
                            }
                        }

                        node.Nodes.Add(chNode);

                        FillRelations(chNode, idd);
                    }

                    return true;
                }

                DataTable tb = _sqlCtx.GetTableData(sqlText);

                if (tb == null)
                    return false;
                if (tb.Rows.Count == 0)
                    return false;

                foreach (DataRow row in tb.Rows)
                {
                    int idd = -1;
                    idd = int.Parse(row["child_id"].ToString());
                    RelationTreeNodeViewModel chNode = new RelationTreeNodeViewModel();
                    chNode.Text = row["name"].ToString();
                    chNode.Amount = Convert.ToDouble(row["amount"]);
                    chNode.Cid = Convert.ToInt32(row["cid"]);
                    if (node.Nodes == null)
                    {
                        node.Nodes = new List<RelationTreeNodeViewModel>();
                    }

                    RelClient _cc = m_Clients.Where(p => p.ChildId == idd).FirstOrDefault();
                    if (_cc != null)
                    {
                        switch (_cc.Type)
                        {
                            case 1: chNode.Text += " (კოორდინატორი)"; break;
                            case 2: chNode.Text += " (უფროსი კოორდინატორი)"; break;
                            case 3: chNode.Text += " (მენეჯერი)"; break;
                            case 4: chNode.Text += " (დირექტორი)"; break;
                        }
                    }

                    node.Nodes.Add(chNode);
                    if (idd != id)
                        FillRelations(chNode, idd);
                }
            }

            return true;
        }

        public static double? GetCurrentContragentRelationAmount()
        {
            using(SqlContext _sqlCtx = new SqlContext())
            {
                return _sqlCtx.GetScalar<double>(@"SELECT (COALESCE(d.debetCycle,0) - COALESCE(d.creditCycle,0)) AS amount
                                                   FROM book.ContragentRelations AS r
                                                   INNER JOIN book.Contragents AS c ON r.child_id = c.id
                                                   OUTER APPLY
                                                   (SELECT

                                                       ROUND((SELECT SUM(e.amount * e.rate) FROM doc.Entries e INNER JOIN doc.GeneralDocs g ON e.general_id = g.id WHERE e.a1 = c.id AND ISNULL(g.is_deleted, 0) = 0 AND(e.debit_acc = c.account OR e.debit_acc = c.account2)), 3) AS debetCycle,
                                                       ROUND((SELECT SUM(e.amount * e.rate) FROM doc.Entries e INNER JOIN doc.GeneralDocs g ON e.general_id = g.id WHERE e.b1 = c.id AND ISNULL(g.is_deleted, 0) = 0 AND(e.credit_acc = c.account OR e.credit_acc = c.account2)), 3) AS creditCycle
                                                   ) AS d
                                                   WHERE r.child_id = " + GeneralRepository.GetCurrentUser().Id + "");
            }
        }



        public class RelClient
        {
            public int Id { get; set; }
            public int ChildId { get; set; }
            public int ParentId { get; set; }
            public string Path { get; set; }
            public string Name { get; set; }
            public int ChildrenCount { get; set; }
            public byte Type { get; set; }
            public bool IsChanged { get; set; }
        }
    }
}