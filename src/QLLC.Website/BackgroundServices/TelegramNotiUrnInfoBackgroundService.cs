using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Services;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.ViewModels;
using System.Text.Json;
using Telegram.Bot.Types;

namespace Tasin.Website.BackgroundServices
{
    public class TelegramNotiUrnInfoBackgroundService : BackgroundService
    {
        private readonly ILogger<TelegramNotiUrnInfoBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TelegramService _telegramService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public TelegramNotiUrnInfoBackgroundService(ILogger<TelegramNotiUrnInfoBackgroundService> logger,
          IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, TelegramService telegramService, IMapper mapper
          )
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _telegramService = telegramService;
            _mapper = mapper;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timeRunHour = Int32.Parse(_configuration.GetSection("TimeRunTelegramNotiHour")?.Value);
            var timeRunMinute = Int32.Parse(_configuration.GetSection("TimeRunTelegramNotiMinute")?.Value);
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                //var nextRun = now.AddSeconds(10);

                var nextRun = now.Date.AddHours(timeRunHour).AddMinutes(timeRunMinute); // 8 AM today
                if (now > nextRun)
                {
                    // If it's past 8 AM today, schedule for 8 AM tomorrow
                    nextRun = nextRun.AddDays(1);
                }

                var delay = nextRun - now;
                if (delay.TotalMilliseconds <= 0)
                {
                    delay = TimeSpan.Zero;
                }
                _logger.LogInformation("TelegramNotiUrnInfoBackgroundService running at: {time}", DateTimeOffset.Now);

                //Wait until the next run time
                await Task.Delay(delay, stoppingToken);
                //Excute your daily task
                await SendTelegramNoti(stoppingToken);
            }
        }
        private List<int> GetInternalStaffRole()
        {
            return new List<int>()
                    {
                        (int)ERoleType.Admin,
                        //(int)ERoleType.Reporter,
                    };
        }
        private async Task SendTelegramNoti(CancellationToken stoppingToken)
        {
            // Your logic to run daily
            _logger.LogInformation("SendTelegramNoti running at: {time}", DateTimeOffset.Now);
            try
            {
                using (var serviceScope = _serviceScopeFactory.CreateScope())
                {
                   
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error send telegram noti to {ex.Message}");
            }
        }
    }
}
