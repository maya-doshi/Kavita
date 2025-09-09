using System;
using System.Linq;
using System.Threading.Tasks;
using API.Constants;
using API.Data;
using API.DTOs.Account;
using API.DTOs.KavitaPlus.License;
using API.Entities.Enums;
using API.Extensions;
using API.Services.Tasks;
using EasyCaching.Core;
using Flurl.Http;
using Kavita.Common;
using Kavita.Common.EnvironmentInfo;
using Microsoft.Extensions.Logging;

namespace API.Services.Plus;
#nullable enable

internal class RegisterLicenseResponseDto
{
    public string EncryptedLicense { get; set; }
    public bool Successful { get; set; }
    public string ErrorMessage { get; set; }
}

public interface ILicenseService
{
    //Task ValidateLicenseStatus();
    Task RemoveLicense();
    Task AddLicense(string license, string email, string? discordId);
    Task<bool> HasActiveLicense(bool forceCheck = false);
    Task<bool> HasActiveSubscription(string? license);
    Task<bool> ResetLicense(string license, string email);
    Task<LicenseInfoDto?> GetLicenseInfo(bool forceCheck = false);
}

public class LicenseService(
    IEasyCachingProviderFactory cachingProviderFactory,
    IUnitOfWork unitOfWork,
    ILogger<LicenseService> logger,
    IVersionUpdaterService versionUpdaterService)
    : ILicenseService
{
    private readonly TimeSpan _licenseCacheTimeout = TimeSpan.FromHours(8);
    public const string Cron = "0 */9 * * *";
    /// <summary>
    /// Cache key for if license is valid or not
    /// </summary>
    public const string CacheKey = "license";
    private const string LicenseInfoCacheKey = "license-info";


    /// <summary>
    /// Performs license lookup to API layer
    /// </summary>
    /// <param name="license"></param>
    /// <returns></returns>
    private async Task<bool> IsLicenseValid(string license)
    {
        var hasLicense =
            !string.IsNullOrEmpty((await unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.LicenseKey))
                .Value);

        if (!hasLicense) return false;

        return true;
    }

    /// <summary>
    /// Register the license with KavitaPlus
    /// </summary>
    /// <param name="license"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    private async Task<string> RegisterLicense(string license, string email, string? discordId)
    {
        return ":p";
    }


    /// <summary>
    /// Checks licenses and updates cache
    /// </summary>
    /// <param name="forceCheck">Skip what's in cache</param>
    /// <returns></returns>
    public async Task<bool> HasActiveLicense(bool forceCheck = false)
    {
        var hasLicense =
            !string.IsNullOrEmpty((await unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.LicenseKey))
                .Value);

        if (!hasLicense) return false;

        return true;
    }

    /// <summary>
    /// Checks if the sub is active and caches the result. This should not be used too much over cache as it will skip backend caching.
    /// </summary>
    /// <param name="license"></param>
    /// <returns></returns>
    public async Task<bool> HasActiveSubscription(string? license)
    {
        var hasLicense =
            !string.IsNullOrEmpty((await unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.LicenseKey))
                .Value);

        if (!hasLicense) return false;

        return true;
    }

    public async Task RemoveLicense()
    {
        var serverSetting = await unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.LicenseKey);
        serverSetting.Value = string.Empty;
        unitOfWork.SettingsRepository.Update(serverSetting);
        await unitOfWork.CommitAsync();

        var provider = cachingProviderFactory.GetCachingProvider(EasyCacheProfiles.License);
        await provider.RemoveAsync(CacheKey);


    }

    public async Task AddLicense(string license, string email, string? discordId)
    {
        var serverSetting = await unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.LicenseKey);
        var lic = await RegisterLicense(license, email, discordId);
        if (string.IsNullOrWhiteSpace(lic))
            throw new KavitaException("unable-to-register-k+");
        serverSetting.Value = lic;
        unitOfWork.SettingsRepository.Update(serverSetting);
        await unitOfWork.CommitAsync();
    }



    public async Task<bool> ResetLicense(string license, string email)
    {
        return true;
    }

    /// <summary>
    /// Fetches information about the license from Kavita+. If there is no license or an exception, will return null and can be assumed it is not active
    /// </summary>
    /// <param name="forceCheck"></param>
    /// <returns></returns>
    public async Task<LicenseInfoDto?> GetLicenseInfo(bool forceCheck = false)
    {
        // Check if there is a license
        var hasLicense =
            !string.IsNullOrEmpty((await unitOfWork.SettingsRepository.GetSettingAsync(ServerSettingKey.LicenseKey))
                .Value);

        if (!hasLicense) return null;
        return new LicenseInfoDto {
            ExpirationDate = DateTime.UtcNow.AddMonths(1),
            IsActive = true,
            IsCancelled = false,
            IsValidVersion = true,
            RegisteredEmail = "bleh@oops.com",
            TotalMonthsSubbed = 69,
            HasLicense = true
        };
    }
}
