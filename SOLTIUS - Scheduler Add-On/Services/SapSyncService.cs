using System;
using SAPbobsCOM;
using SOLTIUS_Scheduler_Add_On.Model;
using SOLTIUS_Scheduler_Add_On.UI;

namespace SOLTIUS_Scheduler_Add_On.Services
{
    public class SapSyncService : IDisposable
    {
        private Company _oCompany;

        public void ConnectToDIAPI(AppConfig config)
        {
            if (_oCompany == null) _oCompany = new Company();
            if (_oCompany.Connected) _oCompany.Disconnect();

            string serverType = (config.SAPServerType ?? "").ToUpper();
            if (serverType.Contains("2019")) _oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2019;
            else if (serverType.Contains("2017")) _oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2017;
            else if (serverType.Contains("2016")) _oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2016;
            else if (serverType.Contains("2022")) _oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2022;
            else if (serverType.Contains("HANA")) _oCompany.DbServerType = BoDataServerTypes.dst_HANADB;
            else _oCompany.DbServerType = BoDataServerTypes.dst_MSSQL2017;

            _oCompany.Server = config.SAPDBServer;
            string cleanLicense = (config.SAPLicenseServer ?? "")
                .Replace("https://", "").Replace("http://", "").Split('/')[0];
            _oCompany.LicenseServer = cleanLicense;
            _oCompany.CompanyDB = config.SAPDatabase;
            _oCompany.UserName = config.SAPUser;
            _oCompany.Password = config.SAPPass;
            _oCompany.DbUserName = config.SAPDBUser;
            _oCompany.DbPassword = config.SAPDBPass;

            int retCode = _oCompany.Connect();
            if (retCode != 0)
            {
                _oCompany.GetLastError(out int errCode, out string errMsg);
                throw new Exception($"SAP Connection Failed [{errCode}]: {errMsg}");
            }
        }

        public void ExecuteSalesOrderSync(SyncLogModel task)
        {
            Documents oOrder = null;
            Document_Lines oLines = null; // Tambahkan variabel eksplisit untuk Lines

            try
            {
                oOrder = (Documents)_oCompany.GetBusinessObject(BoObjectTypes.oOrders);
                oOrder.CardCode = task.CardCode;
                oOrder.DocDueDate = DateTime.Now.AddDays(7);
                oOrder.Comments = "Sync via SOLTIUS Scheduler";

                // Gunakan variabel eksplisit, jangan di-chain (oOrder.Lines.xxx)
                oLines = oOrder.Lines;
                oLines.ItemCode = task.ItemCode;
                oLines.Quantity = task.Quantity;
                oLines.Price = task.Price;
                oLines.WarehouseCode = task.WarehouseCode;

                int addResult = oOrder.Add();
                if (addResult != 0)
                {
                    _oCompany.GetLastError(out int errCode, out string errMsg);
                    if (errMsg.Contains("[SQL Server]") || errMsg.Contains("ODBC") ||
                        errMsg.Contains("Native Client") || errMsg.Contains("SBO_SP_TransactionNotification"))
                    {
                        task.ErrorSource = "Custom Validation (SP)";
                        int lastBracket = errMsg.LastIndexOf(']');
                        if (lastBracket >= 0 && lastBracket < errMsg.Length - 1)
                            errMsg = errMsg.Substring(lastBracket + 1).Trim();
                    }
                    else
                    {
                        task.ErrorSource = "SAP Validation";
                    }

                    throw new Exception($"[{errCode}] {errMsg}");
                }

                task.DocEntry = _oCompany.GetNewObjectKey();
                task.Status = "Success";
                task.ErrorSource = "-";
                task.ErrorMessage = "-";
            }
            catch (System.Runtime.InteropServices.COMException comEx)
            {
                task.ErrorSource = "Server / Network";
                throw new Exception(comEx.Message);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(task.ErrorSource)) task.ErrorSource = "Application Logic";
                throw new Exception(ex.Message);
            }
            finally
            {
                // Selalu rilis COM Object dari child ke parent
                if (oLines != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oLines);
                    oLines = null;
                }
                if (oOrder != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oOrder);
                    oOrder = null;
                }
            }
        }

        /// <summary>
        /// Membuat Sales Order multi-line di SAP dari data pending staging.
        /// Mengembalikan DocEntry SAP yang baru.
        /// </summary>
        public string ExecuteSalesOrderSync(PendingSalesOrder order)
        {
            Documents oOrder = null;
            Document_Lines oLines = null;

            try
            {
                oOrder = (Documents)_oCompany.GetBusinessObject(BoObjectTypes.oOrders);
                oOrder.CardCode = order.CardCode;
                oOrder.DocDate = order.DocDate;
                oOrder.DocDueDate = order.DocDueDate == DateTime.MinValue ? DateTime.Now.AddDays(7) : order.DocDueDate;
                oOrder.TaxDate = order.TaxDate;
                if (!string.IsNullOrEmpty(order.Remarks))
                    oOrder.Comments = order.Remarks;
                else
                    oOrder.Comments = "Sync via SOLTIUS Scheduler";

                foreach (var line in order.Lines)
                {
                    oLines = oOrder.Lines;
                    oLines.ItemCode = line.ItemCode;
                    oLines.Quantity = (double)line.Quantity;
                    oLines.Price = (double)line.Price;
                    if (!string.IsNullOrEmpty(line.Warehouse))
                        oLines.WarehouseCode = line.Warehouse;
                    oLines.Add();
                }

                int addResult = oOrder.Add();
                if (addResult != 0)
                {
                    _oCompany.GetLastError(out int errCode, out string errMsg);
                    throw new Exception($"[{errCode}] {errMsg}");
                }

                return _oCompany.GetNewObjectKey();
            }
            catch (System.Runtime.InteropServices.COMException comEx)
            {
                throw new Exception(comEx.Message);
            }
            finally
            {
                if (oLines != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oLines);
                    oLines = null;
                }
                if (oOrder != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oOrder);
                    oOrder = null;
                }
            }
        }

        public void Dispose()
        {
            if (_oCompany != null)
            {
                if (_oCompany.Connected) _oCompany.Disconnect();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_oCompany);
                _oCompany = null;
            }
        }
    }
}