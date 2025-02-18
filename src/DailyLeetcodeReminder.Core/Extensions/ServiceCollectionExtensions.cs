using DailyLeetcodeReminder.Application.Services;
using DailyLeetcodeReminder.Core.Services;
using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Infrastructure.Jobs;
using DailyLeetcodeReminder.Infrastructure.Contexts;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Telegram.Bot;
using Npgsql;
using Telegram.Bot.Types.Enums;

namespace DailyLeetcodeReminder.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBotClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TelegramBotSetting>(configuration
            .GetSection("TelegramBot"));

        string botApiKey = configuration["TelegramBot:ApiKey"];//Environment.GetEnvironmentVariable("BOT_API_KEY");

        services.AddSingleton<ITelegramBotClient, TelegramBotClient>(x => new TelegramBotClient(botApiKey));

        return services;
    }

    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IChallengerService, ChallengerService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("PostgresqlConnectionString");
            options.UseNpgsql(connectionString);
        });


        services.AddScoped<IChallengerRepository, ChallengerRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();

        services.AddTransient<ILeetCodeBroker, LeetCodeBroker>();

        return services;
    }

    public static IServiceCollection AddUpdateHandler(
        this IServiceCollection services)
    {
        services.AddTransient<UpdateHandler>();

        return services;
    }

    public static IServiceCollection AddSwagger(
        this IServiceCollection services)
    {
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddControllerMappers(
        this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddNewtonsoftJson();

        services.AddEndpointsApiExplorer();

        return services;
    }

    public static IServiceCollection AddHttpClientServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient("leetcode", config =>
        {
            var baseAddress = configuration["Leetcode:BaseAddress"];

            config.BaseAddress = new Uri(baseAddress);
        });

        return services;
    }

    public static IServiceCollection AddJobs(
        this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionScopedJobFactory();

            var dailyReminderJobKey = new JobKey(nameof(DailyReminderJob));

            q.AddJob<DailyReminderJob>(opts =>
            {
                opts.WithIdentity(dailyReminderJobKey);
            });

            q.AddTrigger(opts => opts
                .ForJob(dailyReminderJobKey)
                .WithIdentity($"{dailyReminderJobKey.Name}-trigger")
                .WithCronSchedule("0 0 9,14,23 * * ?")
            );

            var dailyReportJobKey = new JobKey(nameof(DailyReportJob));
            
            q.AddJob<DailyReportJob>(opts =>
            {
                opts.WithIdentity(dailyReportJobKey);
            });

            q.AddTrigger(opts => opts
                .ForJob(dailyReportJobKey)
                .WithIdentity($"{dailyReportJobKey.Name}-trigger")
                .WithCronSchedule("0 0 0 * * ?")
            );

            var dailyProblemJobKey = new JobKey(nameof(DailyProblemJob));

            q.AddJob<DailyProblemJob>(opts =>
            {
                opts.WithIdentity(dailyProblemJobKey);
            });

            q.AddTrigger(opts => opts
                .ForJob(dailyProblemJobKey)
                .WithIdentity($"{dailyProblemJobKey.Name}-trigger")
                .WithCronSchedule("0 0 13 * * ?")
            );
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
    public static async Task UseTelegramBotAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        botClient.StartReceiving(
            async (bot, update, token) =>
            {
                using var scopeRep = serviceProvider.CreateScope();
                var updateHandler = scopeRep.ServiceProvider.GetRequiredService<UpdateHandler>();
                await updateHandler.UpdateHandlerAsync(update);
            },
            async (bot, exception, token) => await HandleErrorAsync(exception),
            new Telegram.Bot.Polling.ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                ThrowPendingUpdates = true
            },
            cancellationToken: CancellationToken.None
        );

    }
    private static Task HandleErrorAsync(Exception exception)
    {
        return Task.CompletedTask;
    }
}