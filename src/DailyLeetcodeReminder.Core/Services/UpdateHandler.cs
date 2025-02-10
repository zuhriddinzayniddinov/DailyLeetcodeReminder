﻿using DailyLeetcodeReminder.Application.Services;
using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Domain.Enums;
using DailyLeetcodeReminder.Domain.Exceptions;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DailyLeetcodeReminder.Core.Services;

public class UpdateHandler
{
    private readonly IChallengerService challengerService;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly ILogger<UpdateHandler> logger;
    private readonly TelegramBotSetting botSetting;
    private static int pageSize = 10;

    public UpdateHandler(
        IChallengerService challengerService,
        ITelegramBotClient telegramBotClient,
        ILogger<UpdateHandler> logger,
        IOptions<TelegramBotSetting> botSetting)
    {
        this.challengerService = challengerService;
        this.telegramBotClient = telegramBotClient;
        this.logger = logger;
        this.botSetting = botSetting.Value;
    }

    public async Task UpdateHandlerAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => HandleCommandAsync(update.Message),
            UpdateType.CallbackQuery => OnCallbackQueryReceivedAsync(update.CallbackQuery),
            _ => HandleNotAvailableCommandAsync(update.Message)
        };

        await handler;
    }

    private async Task OnCallbackQueryReceivedAsync(CallbackQuery callbackQuery)
    {
        var challengers = await challengerService.RetrieveChallengers();

        int pageCount = challengers.Count / pageSize +
            (challengers.Count % pageSize > 0 ? 1 : 0);

        string[] callQueries = callbackQuery.Data.Split(' ');

        int page = int.Parse(callQueries[1]);

        if (callQueries[0] == "next")
        {
            page += (page < pageCount) ? 1 : 0;
        }
        else
        {
            page -= (page > 1) ? 1 : 0;
        }

        var sortedChallengers = challengers.OrderByDescending(ch => ch.TotalSolvedProblems)
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();

        try
        {
            await telegramBotClient.EditMessageTextAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: $"<b>{ServiceHelper.TableBuilder(sortedChallengers)}</b>",
                replyMarkup: ServiceHelper.GenerateButtons(page),
                parseMode: ParseMode.Html);
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception.Message);

            await telegramBotClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Sahifa topilmadi");
        }
    }

    public async Task HandleCommandAsync(Message message)
    {
        if(message is null || message.Text is null)
        {
            return;
        }

        if (message.Text.StartsWith("/") is false)
        {
            return;
        }

        var command = message.Text.Split(' ').First()[1..];
        
        if(command.IndexOf('@') > -1)
            command = command.Split('@')[0];

        try
        {
            var task = command switch
            {
                "start" => HandleStartCommandAsync(message),
                "register" => HandleRegisterCommandAsync(message),
                "rank" => HandleRankCommandAsync(message),
                "statistics" => HandleStatisticsCommandAsync(message),
                "weekly_report" => HandleWeeklyReportCommandAsync(message),
                _ => HandleNotAvailableCommandAsync(message)
            }; ;

            await task;
        }
        catch (AlreadyExistsException exception)
        {
            this.logger.LogError(exception.Message);

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Siz allaqachon ro'yxatdan o'tgansiz");

            return;
        }
        catch (NotFoundException exception)
        {
            this.logger.LogError(exception.Message);

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Kechirasiz usernameni tekshirib qayta urining, username topilmadi");

            return;
        }
        catch (DuplicateException exception)
        {
            this.logger.LogError(exception.Message);

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Sizning telegram yoki leetcode profilingiz ro'yxatdan o'tgan");

            return;
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception.Message);

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Sizning so'rovingizda xatolik yuz berdi. Qayta urinib ko'ring");

            return;
        }
    }

    private async Task HandleWeeklyReportCommandAsync(Message message)
    {
        if (message.Chat.Type != ChatType.Private)
            return;

        var challengers = await this.challengerService
            .WeeklyUserAttempts(message.Chat.Id);

        string status = challengers.Status == UserStatus.Active ? "Faol" : "Nofaol";
        string week = string.Join("\n\n", challengers.DailyAttempts
                    .Select(da =>
                       "Sana: " + $"<b>{da.Date.ToString("dd-MM-yyyy")}</b>" + "\n" +
                       "Ishlangan misollar: " + $"<b>{da.SolvedProblems}</b>"));

        if (week.Length == 0)
            week = "Ma'lumot mavjud emas.";

        string sendText = $"<b>{challengers.FirstName}</b>" + "\n"
            + "Sizda qolgan imkoniyatlar: " + $"<b>{challengers.Heart}</b>" + "\n"
            + "Sizning ishlagan misollaringiz: " + $"<b>{challengers.TotalSolvedProblems}</b>" + "\n"
            + "Sizning holatingiz: " + $"<b>{status}</b>" + "\n\n"
            + "Haftalik hisobotlar\n\n"
            + week;
        

        await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: sendText,
            parseMode: ParseMode.Html);
    }

    private async Task HandleStartCommandAsync(Message message)
    {
        await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Daily leetcode botiga xush kelibsiz. " +
                "Kunlik challenge'da qatnashish uchun, " +
                "leetcode username'ni /register komandasidan keyin yuboring. " +
                "Misol uchun: /register username");
    }

    private async Task HandleNotAvailableCommandAsync(Message message)
    {
        await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Mavjud bo'lmagan komanda kiritildi. " +
                "Tekshirib ko'ring.");
    }

    private async Task HandleRegisterCommandAsync(Message message)
    {
        try
        {
            if (message.Chat.Type != ChatType.Private)
            {
                return;
            }
            
            var leetCodeUsername = message.Text?.Split(' ').Skip(1).FirstOrDefault();

            var groupLink = botSetting.GroupLink;

            if (string.IsNullOrWhiteSpace(leetCodeUsername))
            {
                await this.telegramBotClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Iltimos username ni ham kiriting.\n Misol uchun: /register myusername");

                return;
            }

            var challenger = new Challenger
            {
                TelegramId = message.Chat.Id,
                LeetcodeUserName = leetCodeUsername,
                FirstName = message.From.FirstName,
                LastName = message.From.LastName,
                Status = UserStatus.Active,
            };

            Challenger insertedChallenger = await this.challengerService
                .AddUserAsync(challenger);

            var button = InlineKeyboardButton.WithUrl("Guruhga qo'shilish", groupLink);
            var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: insertedChallenger.TelegramId,
                text: "Siz muvaffaqiyatli ro'yxatdan o'tdingiz",
                replyMarkup: keyboard);
        }
        catch(NotFoundException)
        {
            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Ushbu username leetcode platformasida mavjud emas");
        }
    }

    private async Task HandleRankCommandAsync(Message message)
    {
        var challengers = await challengerService.RetrieveChallengers();

        var sortedChallengers = challengers
            .OrderByDescending(ch => ch.TotalSolvedProblems)
            .Skip(0)
            .Take(10)
            .ToList();

        await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"<b>{ServiceHelper.TableBuilder(sortedChallengers)}</b>",
            replyMarkup: ServiceHelper.GenerateButtons(challengers.Count / 10),
            parseMode: ParseMode.Html);
    }

    private async Task HandleStatisticsCommandAsync(Message message)
    {
        if (message.Chat.Type != ChatType.Private)
            return;

        var chellenger = await challengerService
            .RetrieveChallengerByTelegramIdAsync(message.Chat.Id);

        if (chellenger == null)
        {
            await telegramBotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Kechirasiz, oldin ro'yxatdan o'ting");

            return;
        }

        int totalProblemSolved = await challengerService
            .CurrentSolvedProblemsAsync(chellenger.LeetcodeUserName);
        
        string status = chellenger.Status == UserStatus.Active ? "Faol" : "Nofaol";
        
        string sendText =
            "Sizning ishlagan misollaringiz: "
            + $"<b>{totalProblemSolved}</b>" + "\n"
            + "Sizning holatingiz: "
            + $"<b>{status}</b>"
            + "\n"
            + "Sizda qolgan imkoniyatlar: "
            + $"<b>{chellenger.Heart}</b>";

        await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: sendText,
            parseMode: ParseMode.Html);
    }
}
