using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;

namespace SOLTIUS_Web_API_Add_On.Services.AuditLog
{
    public class ApiLogEntry
    {
        public DateTime Ts { get; set; }
        public string Method { get; set; } = "";
        public string Path { get; set; } = "";
        public int Status { get; set; }
        public string ClientIp { get; set; } = "";
        public string Uid { get; set; } = "";
        public string Detail { get; set; } = "";
        public int DurationMs { get; set; }
    }

    public class SyncLogEntry
    {
        public DateTime Ts { get; set; }
        public string Uid { get; set; } = "";
        public string CardCode { get; set; } = "";
        public string DocType { get; set; } = "";
        public string DocEntry { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string Warehouse { get; set; } = "";
        public string Status { get; set; } = "";
        public string ErrorSource { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }
}
