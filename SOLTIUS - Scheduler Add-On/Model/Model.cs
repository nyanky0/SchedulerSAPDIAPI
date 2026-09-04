using System;

namespace SOLTIUS_Scheduler_Add_On.Model
{
    public class SyncLogModel
    {
        public int ErrorId { get; set; }
        public string DocType { get; set; }
        public string DocEntry { get; set; }
        public string CardCode { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string WarehouseCode { get; set; }
        public string Status { get; set; }
        public string ErrorSource { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
