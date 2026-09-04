using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SOLTIUS_Web_API_Add_On.Models.Transaction
{
    public class SalesOrderDetail
    {
        [Required(ErrorMessage = "ItemCode is required.")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "ItemCode must be 1-30 characters.")]
        [JsonPropertyName("itemCode")]
        public string ItemCode { get; set; } = "";

        [StringLength(200, ErrorMessage = "ItemDescription max 200 characters.")]
        [JsonPropertyName("itemDescription")]
        public string ItemDescription { get; set; } = "";

        [StringLength(20, ErrorMessage = "WarehouseCode max 20 characters.")]
        [JsonPropertyName("warehouseCode")]
        public string WarehouseCode { get; set; } = "";

        [Range(0.001, 9999999, ErrorMessage = "Quantity must be between 0.001 and 9999999.")]
        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        [Range(0, 999999999, ErrorMessage = "Price must be between 0 and 999999999.")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}
