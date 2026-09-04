using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SOLTIUS_Scheduler_Add_On.Model;

namespace SOLTIUS_Scheduler_Add_On.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly int _maxRetryCount;

        public DatabaseService(string connectionString, int maxRetryCount = 5)
        {
            _connectionString = connectionString;
            _maxRetryCount = maxRetryCount;
        }

        public void InitializeTables()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TBL_SYNC_HISTORY' and xtype='U')
                    CREATE TABLE TBL_SYNC_HISTORY (
                        LogID INT IDENTITY(1,1) PRIMARY KEY,
                        DocType VARCHAR(50),
                        DocEntry VARCHAR(50),
                        CardCode VARCHAR(100),
                        ItemCode VARCHAR(100),
                        Quantity INT,
                        Price DECIMAL(18,2),
                        WarehouseCode VARCHAR(50),
                        Status VARCHAR(50),
                        ErrorSource VARCHAR(100),
                        ErrorMessage VARCHAR(MAX),
                        CreatedAt DATETIME
                    );

                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TBL_SYNC_ERROR' and xtype='U')
                    CREATE TABLE TBL_SYNC_ERROR (
                        ErrorID INT IDENTITY(1,1) PRIMARY KEY,
                        DocType VARCHAR(50),
                        DocEntry VARCHAR(50),
                        CardCode VARCHAR(100),
                        ItemCode VARCHAR(100),
                        Quantity INT,
                        Price DECIMAL(18,2),
                        WarehouseCode VARCHAR(50),
                        ErrorMessage VARCHAR(MAX),
                        IsResolved BIT DEFAULT 0,
                        CreatedAt DATETIME
                    );";

                using (var command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveLogToDatabase(SyncLogModel log)
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // 1. Insert to TBL_SYNC_HISTORY (all logs)
                    string queryHistory = @"INSERT INTO TBL_SYNC_HISTORY
                                          (DocType, DocEntry, CardCode, ItemCode, Quantity, Price, WarehouseCode, Status, ErrorSource, ErrorMessage, CreatedAt)
                                          VALUES (@DocType, @DocEntry, @CardCode, @ItemCode, @Qty, @Price, @Whs, @Status, @ErrSrc, @ErrMsg, @CreatedAt)";

                    using (SqlCommand cmd = new SqlCommand(queryHistory, conn))
                    {
                        cmd.Parameters.AddWithValue("@DocType", log.DocType ?? "");
                        cmd.Parameters.AddWithValue("@DocEntry", log.DocEntry ?? "");
                        cmd.Parameters.AddWithValue("@CardCode", log.CardCode ?? "");
                        cmd.Parameters.AddWithValue("@ItemCode", log.ItemCode ?? "");
                        cmd.Parameters.AddWithValue("@Qty", log.Quantity);
                        cmd.Parameters.AddWithValue("@Price", log.Price);
                        cmd.Parameters.AddWithValue("@Whs", log.WarehouseCode ?? "");
                        cmd.Parameters.AddWithValue("@Status", log.Status ?? "");
                        cmd.Parameters.AddWithValue("@ErrSrc", log.ErrorSource ?? "");
                        cmd.Parameters.AddWithValue("@ErrMsg", log.ErrorMessage ?? "");
                        cmd.Parameters.AddWithValue("@CreatedAt", log.CreatedAt == DateTime.MinValue ? DateTime.Now : log.CreatedAt);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. If Failed, also insert to TBL_SYNC_ERROR
                    if (log.Status == "Failed")
                    {
                        string queryError = @"INSERT INTO TBL_SYNC_ERROR
                                              (DocType, DocEntry, CardCode, ItemCode, Quantity, Price, WarehouseCode, ErrorMessage, IsResolved, CreatedAt)
                                              VALUES (@DocType, @DocEntry, @CardCode, @ItemCode, @Qty, @Price, @Whs, @ErrMsg, 0, @CreatedAt)";

                        using (SqlCommand cmdErr = new SqlCommand(queryError, conn))
                        {
                            cmdErr.Parameters.AddWithValue("@DocType", log.DocType ?? "");
                            cmdErr.Parameters.AddWithValue("@DocEntry", log.DocEntry ?? "");
                            cmdErr.Parameters.AddWithValue("@CardCode", log.CardCode ?? "");
                            cmdErr.Parameters.AddWithValue("@ItemCode", log.ItemCode ?? "");
                            cmdErr.Parameters.AddWithValue("@Qty", log.Quantity);
                            cmdErr.Parameters.AddWithValue("@Price", log.Price);
                            cmdErr.Parameters.AddWithValue("@Whs", log.WarehouseCode ?? "");
                            cmdErr.Parameters.AddWithValue("@ErrMsg", log.ErrorMessage ?? "");
                            cmdErr.Parameters.AddWithValue("@CreatedAt", log.CreatedAt == DateTime.MinValue ? DateTime.Now : log.CreatedAt);
                            cmdErr.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Insert Error: " + ex.Message);
            }
        }

        public void MarkErrorsAsResolved(string cardCode, string itemCode)
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string queryUpdate = @"UPDATE TBL_SYNC_ERROR
                                           SET IsResolved = 1
                                           WHERE CardCode = @CardCode
                                           AND ItemCode = @ItemCode
                                           AND IsResolved = 0";

                    using (SqlCommand cmd = new SqlCommand(queryUpdate, conn))
                    {
                        cmd.Parameters.AddWithValue("@CardCode", cardCode);
                        cmd.Parameters.AddWithValue("@ItemCode", itemCode);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Update Error: " + ex.Message);
            }
        }

        public List<SyncLogModel> LoadLogHistory()
        {
            var loadedList = new List<SyncLogModel>();
            if (string.IsNullOrEmpty(_connectionString)) return loadedList;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT TOP 500 DocType, DocEntry, CardCode, ItemCode, Quantity, Price,
                                            WarehouseCode, Status, ErrorSource, ErrorMessage, CreatedAt
                                     FROM TBL_SYNC_HISTORY ORDER BY LogID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            loadedList.Add(new SyncLogModel
                            {
                                DocType = reader["DocType"]?.ToString() ?? "",
                                DocEntry = reader["DocEntry"]?.ToString() ?? "",
                                CardCode = reader["CardCode"]?.ToString() ?? "",
                                ItemCode = reader["ItemCode"]?.ToString() ?? "",
                                Quantity = Convert.IsDBNull(reader["Quantity"]) ? 0 : Convert.ToInt32(reader["Quantity"]),
                                Price = Convert.IsDBNull(reader["Price"]) ? 0.0 : Convert.ToDouble(reader["Price"]),
                                WarehouseCode = reader["WarehouseCode"]?.ToString() ?? "",
                                Status = reader["Status"]?.ToString() ?? "",
                                ErrorSource = reader["ErrorSource"]?.ToString() ?? "",
                                ErrorMessage = reader["ErrorMessage"]?.ToString() ?? "",
                                CreatedAt = Convert.IsDBNull(reader["CreatedAt"]) ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadLog Error: " + ex.Message);
            }
            return loadedList;
        }

        /// <summary>
        /// Mengambil Sales Order pending dari tabel staging (process_status = 0),
        /// termasuk detail barisnya, untuk disinkronkan ke SAP.
        /// </summary>
        public List<PendingSalesOrder> LoadPendingSalesOrders()
        {
            var result = new List<PendingSalesOrder>();
            if (string.IsNullOrEmpty(_connectionString)) return result;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT h.id AS HeaderId, h.cardcode, h.cardname, h.docdate, h.docduedate, h.taxdate, h.remarks,
                               d.linenum, d.itemcode, d.itemname, d.warehouse, d.quantity, d.price
                        FROM sales_order_header h
                        INNER JOIN sales_order_detail d ON d.header_id = h.id
                        WHERE h.process_status = 0
                        ORDER BY h.id, d.linenum";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        PendingSalesOrder current = null;
                        long currentHeaderId = -1;

                        while (reader.Read())
                        {
                            long headerId = Convert.ToInt64(reader["HeaderId"]);

                            if (current == null || headerId != currentHeaderId)
                            {
                                current = new PendingSalesOrder
                                {
                                    HeaderId = headerId,
                                    CardCode = reader["cardcode"]?.ToString() ?? "",
                                    CardName = reader["cardname"]?.ToString() ?? "",
                                    DocDate = Convert.IsDBNull(reader["docdate"]) ? DateTime.Now : Convert.ToDateTime(reader["docdate"]),
                                    DocDueDate = Convert.IsDBNull(reader["docduedate"]) ? DateTime.Now.AddDays(7) : Convert.ToDateTime(reader["docduedate"]),
                                    TaxDate = Convert.IsDBNull(reader["taxdate"]) ? DateTime.Now : Convert.ToDateTime(reader["taxdate"]),
                                    Remarks = reader["remarks"]?.ToString() ?? ""
                                };
                                result.Add(current);
                                currentHeaderId = headerId;
                            }

                            current.Lines.Add(new PendingSalesOrderLine
                            {
                                LineNum = Convert.IsDBNull(reader["linenum"]) ? 0 : Convert.ToInt32(reader["linenum"]),
                                ItemCode = reader["itemcode"]?.ToString() ?? "",
                                ItemName = reader["itemname"]?.ToString() ?? "",
                                Warehouse = reader["warehouse"]?.ToString() ?? "",
                                Quantity = Convert.IsDBNull(reader["quantity"]) ? 0 : Convert.ToDecimal(reader["quantity"]),
                                Price = Convert.IsDBNull(reader["price"]) ? 0 : Convert.ToDecimal(reader["price"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gagal memuat Sales Order pending dari staging.", ex);
            }

            return result;
        }

        /// <summary>
        /// Update status SO pending di tabel staging setelah proses sync ke SAP.
        /// process_status: 0 = pending, 1 = sukses, 2 = gagal.
        /// retrycount hanya bertambah saat gagal; di-reset saat sukses.
        /// </summary>
        public void UpdateSalesOrderStatus(long headerId, int processStatus, string errorMessage = null)
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        UPDATE sales_order_header
                        SET process_status = @Status,
                            errormessage = @ErrMsg,
                            retrycount = CASE WHEN @Status = 1 THEN 0 ELSE retrycount + 1 END,
                            processed_at = GETDATE(),
                            updated_at = GETDATE()
                        WHERE id = @HeaderId;

                        UPDATE sales_order_detail
                        SET process_status = @Status,
                            updated_at = GETDATE()
                        WHERE header_id = @HeaderId;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HeaderId", headerId);
                        cmd.Parameters.AddWithValue("@Status", processStatus);
                        cmd.Parameters.AddWithValue("@ErrMsg", (object)errorMessage ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Update Status Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Returns true if the SO header has exceeded the max retry limit.
        /// </summary>
        public bool IsRetryLimitExceeded(long headerId)
        {
            if (string.IsNullOrEmpty(_connectionString)) return false;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT retrycount FROM sales_order_header WHERE id = @HeaderId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HeaderId", headerId);
                        object result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return false;
                        int retryCount = Convert.ToInt32(result);
                        return retryCount >= _maxRetryCount;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mark SO as dead-letter (status = 3) when retry limit exceeded.
        /// </summary>
        public void MarkAsExceededRetryLimit(long headerId)
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE sales_order_header
                        SET process_status = 3,
                            errormessage = 'Max retry limit exceeded',
                            updated_at = GETDATE()
                        WHERE id = @HeaderId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HeaderId", headerId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB MarkExceeded Error: " + ex.Message);
            }
        }

        public List<SyncLogModel> GetLatestUnresolvedErrors()
        {
            var latestErrors = new List<SyncLogModel>();
            if (string.IsNullOrEmpty(_connectionString)) return latestErrors;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT ErrorID, DocType, DocEntry, CardCode, ItemCode, Quantity, Price, WarehouseCode, ErrorMessage, CreatedAt
                        FROM TBL_SYNC_ERROR e1
                        WHERE IsResolved = 0
                        AND ErrorID = (
                            SELECT MAX(ErrorID)
                            FROM TBL_SYNC_ERROR e2
                            WHERE e2.CardCode = e1.CardCode
                              AND e2.ItemCode = e1.ItemCode
                              AND e2.IsResolved = 0
                        )";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            latestErrors.Add(new SyncLogModel
                            {
                                ErrorId = Convert.ToInt32(reader["ErrorID"]),
                                DocType = reader["DocType"]?.ToString() ?? "",
                                DocEntry = reader["DocEntry"]?.ToString() ?? "",
                                CardCode = reader["CardCode"]?.ToString() ?? "",
                                ItemCode = reader["ItemCode"]?.ToString() ?? "",
                                Quantity = Convert.IsDBNull(reader["Quantity"]) ? 0 : Convert.ToInt32(reader["Quantity"]),
                                Price = Convert.IsDBNull(reader["Price"]) ? 0.0 : Convert.ToDouble(reader["Price"]),
                                WarehouseCode = reader["WarehouseCode"]?.ToString() ?? "",
                                ErrorMessage = reader["ErrorMessage"]?.ToString() ?? "",
                                CreatedAt = Convert.IsDBNull(reader["CreatedAt"]) ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gagal memuat data error dari database.", ex);
            }
            return latestErrors;
        }
    }
}
