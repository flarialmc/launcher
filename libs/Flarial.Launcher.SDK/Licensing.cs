using System;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Windows.Globalization;
using Windows.System.Profile;
using System.Threading.Tasks;
using static System.Xml.XmlDictionaryReaderQuotas;
using static System.Runtime.Serialization.Json.JsonReaderWriterFactory;
using static Windows.ApplicationModel.Store.LicenseManagement.LicenseManager;

namespace Flarial.Launcher.SDK;

/// <summary>
/// Provides methods to manage licenses for Minecraft: Bedrock Edition.
/// </summary>

public static class Licensing
{
    const string Uri = "https://storesdk.dsx.mp.microsoft.com/v8.0/Sdk/products/contentId?market={0}&locale=iv&deviceFamily={1}&productIds={2}";

    const string Name = @"SOFTWARE\Flarial.Launcher.SDK";

    static readonly string _uri = string.Format(Uri, "{0}", AnalyticsInfo.VersionInfo.DeviceFamily, "9P5X4QVLC2XR,9NBLGGH2JHXJ");

    static readonly HttpClient _client = new();

    /// <summary>
    ///  Checks licenses for Minecraft: Bedrock Edition.
    /// </summary>

    /// <returns>
    /// If licenses exists then <c>true</c> else <c>false</c>.
    /// </returns>

    public static async Task<bool> CheckAsync()
    {
        var uri = string.Format(_uri, new GeographicRegion().CodeTwoLetter);
        using var stream = await _client.GetStreamAsync(uri);
        using var reader = CreateJsonReader(stream, Max);

        var element = XElement.Load(reader);
        var keyIds = element.Descendants("KeyIds").Select(_ => _.Value);
        var contentIds = element.Descendants("ContentIds").Select(_ => _.Value);

        var result = await GetSatisfactionInfosAsync(contentIds, keyIds);
        if (result.ExtendedError is not null) throw result.ExtendedError;

        var values = result.LicenseSatisfactionInfos.Values;
        return values.Any(_ => _.IsSatisfied && !_.SatisfiedByTrial);
    }
}