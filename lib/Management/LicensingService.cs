using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Flarial.Launcher.Services.Networking;
using Windows.ApplicationModel.Store.LicenseManagement;
using Windows.Globalization;
using Windows.System.Profile;

namespace Flarial.Launcher.Services.Management;

public static class LicensingService
{
    const string Uri = "https://storesdk.dsx.mp.microsoft.com/v8.0/Sdk/products/contentId?market={0}&locale=iv&deviceFamily={1}&productIds={2}";

    const string ProductIds = "9P5X4QVLC2XR,9NBLGGH2JHXJ";

    static readonly string s_uri = string.Format(Uri, "{0}", AnalyticsInfo.VersionInfo.DeviceFamily, ProductIds);

    public static async Task<bool> VerifyAsync()
    {
        var uri = string.Format(s_uri, new GeographicRegion().CodeTwoLetter);

        using var stream = await HttpService.GetAsync<Stream>(uri);
        using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);

        var element = XElement.Load(reader);
        var keyIds = element.Descendants("KeyIds").Select(_ => _.Value);
        var contentIds = element.Descendants("ContentIds").Select(_ => _.Value);

        var result = await LicenseManager.GetSatisfactionInfosAsync(contentIds, keyIds);
        if (result.ExtendedError is { }) throw result.ExtendedError;

        var values = result.LicenseSatisfactionInfos.Values;
        return values.Any(_ => _.IsSatisfied && !_.SatisfiedByTrial);
    }
}