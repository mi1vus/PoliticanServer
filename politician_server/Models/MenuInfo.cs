namespace politician_server.Models
{
    public class MenuInfo
    {
        public string Text { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public MenuInfo[] SubItems { get; set; }
    }
}