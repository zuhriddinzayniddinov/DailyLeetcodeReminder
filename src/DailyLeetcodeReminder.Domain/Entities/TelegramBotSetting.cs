namespace DailyLeetcodeReminder.Domain.Entities;

public class TelegramBotSetting
{
    public string ApiKey { get; set; }
    public string BaseAddress { get; set; }
    public string GroupLink { get; set; }
    public long GroupId { get; set; }
}