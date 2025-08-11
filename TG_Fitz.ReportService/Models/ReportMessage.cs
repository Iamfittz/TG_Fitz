namespace TG_Fitz.ReportService.Models {
    public class ReportMessage {
        public long ChatId { get; set; }
        public string CompanyName { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
