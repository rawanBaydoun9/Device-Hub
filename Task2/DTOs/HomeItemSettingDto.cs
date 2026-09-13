namespace Task2.DTOs
{
    public class HomeItemSettingDto
    {
        public int Id { get; set; }

        public string Key { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconClass { get; set; } = string.Empty;

        public string ControllerName { get; set; } = string.Empty;

        public string ActionName { get; set; } = "Index";

        public bool IsVisible { get; set; }

        public int DisplayOrder { get; set; }
    }
}