namespace vizsgaController.Dtos
{
    public class AdminReportDTO
    {
        public int ReportID { get; set; }
        public int PostID { get; set; }
        public string PostTitle { get; set; }
        public int ReporterUserID { get; set; }
        public string ReporterUsername { get; set; }
        public string Reason { get; set; }
        public DateTime Created_at { get; set; }
        public string ReportStatus { get; set; }
    }
}
