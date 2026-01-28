using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;
using Windows.Storage;

namespace Flarial.Launcher.Management;

abstract class SponsorshipItem
{
    internal SponsorshipItem() { }
    protected abstract string BannerUrl { get; }
    protected abstract string CampaignUrl { get; }

    internal async Task<Tuple<Stream, string>?> GetAsync()
    {
        try { return new(new MemoryStream(await HttpStack.GetBytesAsync(BannerUrl)), CampaignUrl); }
        catch { return null; }
    }
}

static class SponsorshipRegistry
{
    internal static readonly SponsorshipItem _leftSponsorship = new LiteByteHosting();

    internal static readonly SponsorshipItem _centerSponsorship = new CollapseNetwork();

    internal static readonly SponsorshipItem _rightSponsorship = new ExampleSponsorship();

    /*
        - Register & store sponsorships in the launcher.
        - We can easily swap these out as required.
    */

    sealed class LiteByteHosting : SponsorshipItem
    {
        protected override string BannerUrl => "https://litebyte.co/images/flarial.png";
        protected override string CampaignUrl => "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";
    }

    sealed class CollapseNetwork : SponsorshipItem
    {
        protected override string CampaignUrl => "https://collapsemc.com";
        protected override string BannerUrl => "https://collapsemc.com/assets/other/ad-banner.png";
    }

    sealed class ExampleSponsorship : SponsorshipItem
    {
        /*
            - Should be as a placeholder.
            - Only used for testing purposes.
        */

        protected override string CampaignUrl => "https://example.com";
        protected override string BannerUrl => "https://www.solidbackgrounds.com/images/1920x1080/1920x1080-white-solid-color-background.jpg";
    }
}