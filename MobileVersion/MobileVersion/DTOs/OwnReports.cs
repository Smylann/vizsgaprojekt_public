using System;

namespace MobileVersion.Dtos
{
    public class OwnReports
    {
        public int UserID { get; set; }
        public int PostID { get; set; }
        public string PostTitle { get; set; }
        public string Reason { get; set; }
        public DateTime Created_at { get; set; }
        public string ReportStatus { get; set; }
    }
}
