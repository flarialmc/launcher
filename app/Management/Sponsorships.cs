using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;

namespace Flarial.Launcher.Management;

abstract class SponsorshipInfo
{
    internal SponsorshipInfo() { }

    protected abstract string BannerUri { get; }
    internal protected abstract string CampaignUri { get; }

    internal async Task<Stream?> StreamAsync()
    {
        try { return new MemoryStream(await HttpService.BytesAsync(BannerUri)); }
        catch { return null; }
    }

    internal static async Task<List<SponsorshipBlob>> GetAsync(IReadOnlyList<SponsorshipInfo> info)
    {
        Task<Stream?>[] tasks = [.. info.Select(_ => _.StreamAsync())];
        await Task.WhenAll(tasks);

        List<SponsorshipBlob> blobs = [];

        for (var index = 0; index < info.Count; index++)
        {
            if (await tasks[index] is not { } stream) continue;
            blobs.Add(new(info[index].CampaignUri, stream));
        }

        return blobs;
    }
}

sealed class SponsorshipBlob : IDisposable
{
    internal readonly string _uri;
    internal readonly Stream _stream;

    internal SponsorshipBlob(string uri, Stream stream)
    {
        _uri = uri;
        _stream = stream;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _stream.Dispose();
    }

    ~SponsorshipBlob() => Dispose();
}

static class ServerSponsorshipRegistry
{
    static readonly List<SponsorshipInfo> _info = [];

    static ServerSponsorshipRegistry()
    {
        _info.Add(new Google());
    }

    internal static async Task<List<SponsorshipBlob>> GetAsync() => await SponsorshipInfo.GetAsync(_info);

    sealed class Google : SponsorshipInfo
    {
        protected override string BannerUri => "https://www.solidbackgrounds.com/images/1920x1080/1920x1080-red-solid-color-background.jpg";
        internal protected override string CampaignUri => "https://google.com";
    }
}

static class PromoSponsorshipRegistry
{
    static readonly List<SponsorshipInfo> _info = [];

    static PromoSponsorshipRegistry()
    {
        _info.Add(new LiteByte());
    }

    internal static async Task<List<SponsorshipBlob>> GetAsync() => await SponsorshipInfo.GetAsync(_info);

    sealed class LiteByte : SponsorshipInfo
    {
        protected override string BannerUri => "https://litebyte.co/images/flarial.png";
        internal protected override string CampaignUri => "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";
    }
}