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
                await SendTelegramNotiOnGeneralDay(stoppingToken);
            }
        }
        private List<int> GetInternalStaffRole()
        {
            return new List<int>()
                    {
                        (int)ERoleType.Admin,
                        (int)ERoleType.Reporter,
                    };
        }
        private async Task SendTelegramNotiOnGeneralDay(CancellationToken stoppingToken)
        {
            // Your logic to run daily
            _logger.LogInformation("SendTelegramNotiOnGeneralDay running at: {time}", DateTimeOffset.Now);
            try
            {
                using (var serviceScope = _serviceScopeFactory.CreateScope())
                {
                    var numberDays = Int32.Parse(_configuration.GetSection("NumberOfDaysNotiGeneralDay")?.Value);

                    var _context = serviceScope.ServiceProvider.GetRequiredService<SampleDBContext>();
                    var now = DateTime.Now;
                    var configDBList = await _context.Configs.Where(i=> i.MonthGeneralNotification != 0 && i.DayGeneralNotification != 0).ToListAsync();
                    configDBList = configDBList.Where(i =>
                    {
                        var tmp = LunarCalendarHelper.CalculateNearestLunarDate(i.DayGeneralNotification, i.MonthGeneralNotification);
                        return tmp.TotalDays == numberDays;
                    }).ToList();

                    if (configDBList.Count() > 0) {
                        var tenantIdList = configDBList.Select(i=> i.TenantId).Distinct().ToList();
                        var userWithTelegramChat = await (from u in _context.Users.Where(i => i.TenantId.HasValue && tenantIdList.Contains(i.TenantId.Value))
                                                    join c in _context.TelegramChats on u.Id equals c.UserId
                                                    select new { c, u }).Distinct().ToListAsync();
                        List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();
                        var join = from tmp in userWithTelegramChat
                                   join config in configDBList on tmp.u.TenantId equals config.TenantId
                                   group new { config, tmp } by config into grouped
                                   select new
                                   {
                                       Config = grouped.Key,
                                       UserWithChatList = grouped.Select(g => g.tmp.c.TelegramChatId)
                                   };

                        //Thêm noti cho nhân viên nhà chùa
                        var internalRoleIdList = GetInternalStaffRole();
                        var telegramChatIdInternalStaffList = await (from u in _context.Users.Where(i => i.RoleIdList.Any(j => internalRoleIdList.Contains(j)))
                                                                     join t in _context.TelegramChats on u.Id equals t.UserId
                                                                     select t.TelegramChatId).ToListAsync();

                        foreach (var item in join)
                        {
                            var tmpMessage = $"Ngày Hiệp Kỵ Chung được tổ chức vào ngày {item.Config.DayGeneralNotification}/{item.Config.MonthGeneralNotification} Âm lịch (ngày {item.Config.DayGeneralNotification} tháng {item.Config.MonthGeneralNotification} Âm lịch)";

                            var chatIdList = new List<string>(item.UserWithChatList);
                            chatIdList.AddRange(telegramChatIdInternalStaffList);

                            foreach (var telegramChatId in chatIdList)
                            {
                                tasks.Add(_telegramService.SendMessageAsync(telegramChatId, tmpMessage, ETelegramNotiType.GeneralAnniversary));
                            }
                        }
                        var sendAnniversarySuccessTelegramList = new List<string>();
                        if (tasks.Count > 0)
                        {
                            HttpResponseMessage[] responses = await Task.WhenAll(tasks);

                            foreach (var response in responses)
                            {
                                string jsonResponse = await response.Content.ReadAsStringAsync();
                                var telegramResponse = JsonSerializer.Deserialize<TelegramResponse>(jsonResponse);

                                if (telegramResponse.Success)
                                {
                                    sendAnniversarySuccessTelegramList.Add(telegramResponse.TelegramChatId);
                                }
                                else
                                {
                                    // Handle errors if needed
                                    _logger.LogError($"Failed to send message to {telegramResponse.TelegramChatId}: {telegramResponse.ErrorMessage}");
                                }
                            }
                            sendAnniversarySuccessTelegramList = sendAnniversarySuccessTelegramList.Distinct().ToList();
                            try
                            {
                                _context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Failed to save changes after send telegram noti to {ex.Message}");

                            }

                        }
                    }
                    
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error send telegram noti to {ex.Message}");
            }

        }
        private async Task SendTelegramNoti(CancellationToken stoppingToken)
        {
            // Your logic to run daily
            _logger.LogInformation("SendTelegramNoti running at: {time}", DateTimeOffset.Now);
            try
            {
                using (var serviceScope = _serviceScopeFactory.CreateScope())
                {
                    var tokenSecretKey = Utils.DecodePassword(_configuration.GetSection("JWT:SecretKey").Value, EEncodeType.SHA_256);
                    var urlWeb = _configuration.GetSection("UrlWeb")?.Value;
                    var _context = serviceScope.ServiceProvider.GetRequiredService<SampleDBContext>();
                    var configDBList = await _context.Configs.ToListAsync();
                    //Danh sách linh cốt
                    var urnDB = await _context.Urns.Where(i => i.State == (int)EState.Active).Include(i => i.FamilyMembers).ToListAsync();
                    var familyMembers = await _context.User_Urns.ToListAsync();
                    var urnWithLunarDate = _mapper.Map<List<UrnViewModel>>(urnDB);
                    var now = DateTime.Now;
                    var joinUrnWithConfigTenant = from urn in urnWithLunarDate
                                                  join config in configDBList on urn.TenantId equals config.TenantId
                                                  select new { urn, config };

                    //Danh sách linh cốt cần nhắc giỗ
                    var urnWithWarningAnniversaryList = joinUrnWithConfigTenant.Where(i => i.urn.PreviewLunarDate.TotalDays > 0 && i.urn.PreviewLunarDate.TotalDays <= i.config.NumberOfDaysNoticeAnniversary).Select(i => i.urn).ToList();

                    //Danh sách linh cốt cần nhắc ký gửi
                    var urnWithWarningExpiredList = joinUrnWithConfigTenant.Where(i => now.Subtract(i.urn.ExpiredDate).TotalDays <= i.config.NumberOfDaysNoticeExpiredUrn && now.Subtract(i.urn.ExpiredDate).TotalDays >= 0).Select(i => i.urn).ToList();

                    //Danh sách chùa
                    var tenantIdList = (urnWithWarningAnniversaryList.Select(i => i.TenantId))
                                        .Concat(urnWithWarningExpiredList.Select(i => i.TenantId))
                                        .Distinct().ToList();
                    var tenantDBList = await _context.Tenants.Where(i => tenantIdList.Contains(i.Id)).ToListAsync();

                    var userIdList = (urnWithWarningAnniversaryList.SelectMany(i => i.FamilyMemberList).Select(i => i.Id))
                                     .Concat(urnWithWarningExpiredList.SelectMany(i => i.FamilyMemberList).Select(i => i.Id))
                                     .Distinct().ToList();
                    var userDBList = await _context.Users.Where(i => userIdList.Contains(i.Id)).ToListAsync();
                    var telegramChatDbList = await _context.TelegramChats.Where(i => userIdList.Contains(i.UserId)).Distinct().ToListAsync();

                    List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

                    var roleDBList = await _context.Roles.ToListAsync();

                    //Thêm noti cho nhân viên nhà chùa
                    var internalRoleIdList = GetInternalStaffRole();

                    var telegramChatIdInternalStaffList = await (from u in _context.Users.Where(i => i.RoleIdList.Any(j => internalRoleIdList.Contains(j)))
                                                                 join t in _context.TelegramChats on u.Id equals t.UserId
                                                                 select new
                                                                 {
                                                                     t.TelegramChatId,
                                                                     u.Id
                                                                 }).ToListAsync();
                    var staffIdList = telegramChatIdInternalStaffList.Select(i => i.Id).ToList();   

                    void prepareMessage(List<string> telegramChatIdList, ETelegramNotiType type,DateTime? lastNotiDateTime,int userId, string suffix, List<UrnViewModel> data, Config config, ref bool isHasNotiTelegram)
                {
                    var message = "";
                    var urnList = data.Where(j => j.FamilyMemberIdList.Contains(userId)).GroupBy(i => new { i.TenantId , i.Id}).ToList();
                    urnList.ForEach(u =>
                    {
                        var urnNameList = string.Join(", ", u.Select(i => i.Name));
                        var tenant = tenantDBList.First(j => j.Id == u.Key.TenantId);
                        var lunarDate = u.First().PreviewLunarDate.NearestLunarAnniversaryDate;
                        var lunarDateString = $"{lunarDate.day}/{lunarDate.month}/{lunarDate.year}";
                        var solarDate = u.First().PreviewLunarDate.NearestLunarAnniversaryDateToSolarDate;
                        if (type == ETelegramNotiType.Anniversary)
                        {
                            message += $"Linh cốt <b>{urnNameList}</b> tại <b>{tenant.Name}</b> {suffix} (Ngày âm lịch:<b>{lunarDateString}</b>, Ngày dương lịch: <b>{solarDate.ToString("dd/MM/yyyy")}</b>)\n";
                        }
                        else
                        {
                            message += $"Linh cốt <b>{urnNameList}</b> tại <b>{tenant.Name}</b> {suffix} (Ngày dương lịch: <b>{u.First().ExpiredDate.ToString("dd/MM/yyyy")}</b>)\n";
                        }
                    });

                        if (lastNotiDateTime.HasValue == false || now.Subtract(lastNotiDateTime.Value).TotalDays > config.RemindNotification)
                        {
                            telegramChatIdList.ForEach(telegramChatId =>
                            {
                                tasks.Add(_telegramService.SendMessageAsync(telegramChatId, message, type));
                            });
                            isHasNotiTelegram = true;
                        }
                    }
                    var sendAnniversarySuccessTelegramList = new List<string>();
                    var sendExpiredSuccessTelegramList = new List<string>();
                    telegramChatDbList.GroupBy(i => new { i.UserId, i.TelegramChatId }).ToList().ForEach(i =>
                    {
                        var isHasNotiTelegram = false;
                        var tenantId = userDBList.First(j => j.Id == i.Key.UserId).TenantId;
                        var config = configDBList.First(j => j.TenantId == tenantId);
                        var tmpTelegramChatIdList = new List<string>(telegramChatIdInternalStaffList.Select(i=>i.TelegramChatId));
                        tmpTelegramChatIdList.Add(i.Key.TelegramChatId);
                        tmpTelegramChatIdList = tmpTelegramChatIdList.Distinct().ToList();
                        prepareMessage(tmpTelegramChatIdList, ETelegramNotiType.Anniversary, i.First().LastSendAnniversaryNotiDateTime, i.Key.UserId, "sắp đến ngày giỗ", urnWithWarningAnniversaryList, config, ref isHasNotiTelegram);
                        prepareMessage(tmpTelegramChatIdList, ETelegramNotiType.Expired, i.First().LastSendExpiredNotiDateTime, i.Key.UserId, "sắp đến hạn ký gửi", urnWithWarningExpiredList, config, ref isHasNotiTelegram);
                        if (isHasNotiTelegram)
                        {
                            var user = userDBList.First(j => j.Id == i.Key.UserId);
                            var decodePassword = Utils.DecodePassword(user.Password, EEncodeType.SHA_256);
                            var enumActionList = roleDBList.Where(i=> user.RoleIdList.Contains(i.Id)).SelectMany(i => i.EnumActionList).Distinct().ToList();

                            var handlingClaimModel = Helper.GenerateLoginClaim(new Helper.LoginClaim()
                            {
                                EnumActionList = enumActionList,
                                RoleIdList = user.RoleIdList,
                                UserId = user.Id,
                                IsMobile = false,
                                TenantId = user.TenantId,
                                UserName = user.UserName,
                                Password = decodePassword,
                                RememberMe = false
                            });
                            var token = Helper.GenerateToken(handlingClaimModel, tokenSecretKey);
                            urlWeb += $"account/loginByToken?token={token}";
                            var exTraMessage = $"<b><a href=\"{urlWeb}\"><u>Nhấn vào đây</u></a> để xem thông tin (hiệu lực 24h)</b>\nHoặc gõ <b>/login</b> để nhận link mới khi hết hạn.\nNhấn <b>'Share Contact'</b> để chia sẻ số điện thoại nhận thông báo.";
                            //tasks.Add(_telegramService.SendMessageAsync("5247682503", exTraMessage, ETelegramNotiType.UrlAndAccount));
                            tasks.Add(_telegramService.SendMessageAsync(i.Key.TelegramChatId, exTraMessage, ETelegramNotiType.UrlAndAccount));
                        }
                    });
                    if (tasks.Count > 0)
                    {
                        HttpResponseMessage[] responses = await Task.WhenAll(tasks);

                        foreach (var response in responses)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            var telegramResponse = JsonSerializer.Deserialize<TelegramResponse>(jsonResponse);

                            if (telegramResponse.Success)
                            {
                                if (telegramResponse.Type == ETelegramNotiType.Anniversary)
                                {
                                    sendAnniversarySuccessTelegramList.Add(telegramResponse.TelegramChatId);
                                }
                                else
                                {
                                    sendExpiredSuccessTelegramList.Add(telegramResponse.TelegramChatId);
                                }
                            }
                            else
                            {
                                // Handle errors if needed
                                _logger.LogError($"Failed to send message to {telegramResponse.TelegramChatId}: {telegramResponse.ErrorMessage}");
                            }
                        }

                        sendAnniversarySuccessTelegramList = sendAnniversarySuccessTelegramList.Distinct().ToList();
                        sendExpiredSuccessTelegramList = sendExpiredSuccessTelegramList.Distinct().ToList();

                        foreach (var t in sendAnniversarySuccessTelegramList)
                        {
                            var conversation = telegramChatDbList.First(i => i.TelegramChatId == t);
                            conversation.LastSendAnniversaryNotiDateTime = now;
                        }
                        foreach (var t in sendExpiredSuccessTelegramList)
                        {
                            var conversation = telegramChatDbList.First(i => i.TelegramChatId == t);
                            conversation.LastSendExpiredNotiDateTime = now;
                        }
                        try
                        {
                            _context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to save changes after send telegram noti to {ex.Message}");

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error send telegram noti to {ex.Message}");
            }
        }
    }
}
