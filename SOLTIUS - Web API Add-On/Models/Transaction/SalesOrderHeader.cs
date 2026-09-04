using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SOLTIUS_Web_API_Add_On.Models.Transaction
{
    public class SalesOrderHeader
    {
        [Required(ErrorMessage = "CardCode is required.")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "CardCode must be 1-30 characters.")]
        [JsonPropertyName("cardCode")]
        public string CardCode { get; set; } = "";

        [StringLength(200, ErrorMessage = "CardName max 200 characters.")]
        [JsonPropertyName("cardName")]
        public string CardName { get; set; } = "";

        [JsonPropertyName("docDate")]
        public DateTime DocDate { get; set; } = DateTime.MinValue;

        [JsonPropertyName("docDueDate")]
        public DateTime DocDueDate { get; set; } = DateTime.MinValue;

        [JsonPropertyName("taxDate")]
        public DateTime TaxDate { get; set; } = DateTime.MinValue;

        [StringLength(254, ErrorMessage = "Remarks max 254 characters.")]
        [JsonPropertyName("remarks")]
        public string Remarks { get; set; } = "";

        [JsonPropertyName("documentLines")]
        [MinLength(1, ErrorMessage = "At least one DocumentLine is required.")]
        [MaxLength(100, ErrorMessage = "Maximum 100 DocumentLines per order.")]
        public List<SalesOrderDetail> DocumentLines { get; set; } = new();
    }
}
