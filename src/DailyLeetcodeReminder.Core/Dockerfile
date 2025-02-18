FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DailyLeetcodeReminder.Core/DailyLeetcodeReminder.Core.csproj", "src/DailyLeetcodeReminder.Core/"]
COPY ["src/DailyLeetcodeReminder.Application/DailyLeetcodeReminder.Application.csproj", "src/DailyLeetcodeReminder.Application/"]
COPY ["src/DailyLeetcodeReminder.Infrastructure/DailyLeetcodeReminder.Infrastructure.csproj", "src/DailyLeetcodeReminder.Infrastructure/"]
COPY ["src/DailyLeetcodeReminder.Domain/DailyLeetcodeReminder.Domain.csproj", "src/DailyLeetcodeReminder.Domain/"]
RUN dotnet restore "src/DailyLeetcodeReminder.Core/DailyLeetcodeReminder.Core.csproj"
COPY . .
WORKDIR "/src/src/DailyLeetcodeReminder.Core"
RUN dotnet build "DailyLeetcodeReminder.Core.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DailyLeetcodeReminder.Core.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DailyLeetcodeReminder.Core.dll"]
