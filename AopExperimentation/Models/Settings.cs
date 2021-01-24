namespace AopExperimentation.Models
{
    public class Settings : ISettings
    {
        public int CountdownDelay { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}