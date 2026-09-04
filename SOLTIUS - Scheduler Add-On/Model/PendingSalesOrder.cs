using System;
using System.Collections.Generic;

namespace SOLTIUS_Scheduler_Add_On.Model
{
    /// <summary>
    /// Sales Order pending dari tabel staging (sales_order_header + sales_order_detail)
    /// yang belum diproses (process_status = 0).
    /// </summary>
    public class PendingSalesOrder
    {
        public long HeaderId { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public DateTime TaxDate { get; set; }
        public string Remarks { get; set; }
        public List<PendingSalesOrderLine> Lines { get; set; } = new List<PendingSalesOrderLine>();
    }

    public class PendingSalesOrderLine
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Warehouse { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
