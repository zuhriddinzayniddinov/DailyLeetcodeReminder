using DailyLeetcodeReminder.Infrastructure.Models;
using DailyLeetcodeReminder.Infrastructure.Services;
using Quartz;
using System.Text;
using DailyLeetcodeReminder.Domain.Entities;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DailyLeetcodeReminder.Infrastructure.Jobs
{
    public class DailyProblemJob : IJob
    {
        private readonly ILeetCodeBroker leetCodeBroker;
        private readonly ITelegramBotClient telegramBotClient;
        private readonly TelegramBotSetting botSetting;

        public DailyProblemJob(
            ILeetCodeBroker leetCodeBroker,
            ITelegramBotClient telegramBotClient,
            IOptions<TelegramBotSetting> botSetting)
        {
            this.leetCodeBroker = leetCodeBroker;
            this.telegramBotClient = telegramBotClient;
            this.botSetting = botSetting.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            DailyProblem dailyProblem =
                await leetCodeBroker.GetDailyProblemAsync();

            StringBuilder builder = new StringBuilder();
            
            builder.AppendLine($"<b>Kun masalasi</b> - {dailyProblem.Date}");
            builder.AppendLine($"<b>Masala</b> - <a href=\"https://leetcode.com{dailyProblem.Link}\">{dailyProblem.Title}</a>");
            builder.AppendLine($"<b>Qiyinchilik</b> - {dailyProblem.Difficulty}");
            builder.AppendLine($"<b>Teglar</b> - {dailyProblem.Tags}");

            var message = await telegramBotClient.SendTextMessageAsync(
                chatId: botSetting.GroupId,
                text: builder.ToString(),
                parseMode: ParseMode.Html);

            await telegramBotClient.PinChatMessageAsync(botSetting.GroupId, message.MessageId);
        }
    }
}
