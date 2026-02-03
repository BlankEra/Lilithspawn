using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Semver;


public class Releases
{
    public const string API_VERSION = "2022-11-28";

    public const string PROJECT_URL = "https://api.github.com/repos/Rhythia/Client";

    public static SemVersion CurrentVersion = SemVersion.Parse((string)ProjectSettings.GetSetting("application/config/version"));

    /// <summary>
    /// Select release information in the project repository
    /// </summary>
    public static async Task<ReleaseInfo> GetReleaseInfo(SemVersion version)
    {
        var client = Rhythia.HTTP_CLIENT;
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{PROJECT_URL}/releases/tags/{version.ToString()}");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Rhythia", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", API_VERSION);

        var response = await client.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Logger.Log($"Could not get release info: {ex.Message}");
            return null;
        }


        return await response.Content.ReadFromJsonAsync<ReleaseInfo>();
    }

    /// <summary>
    /// Selects the latest release in the project repository
    /// </summary>
    public static async Task<ReleaseInfo> GetLatestRelease(bool prerelease = true)
    {
        var client = Rhythia.HTTP_CLIENT;
        var request = new HttpRequestMessage(HttpMethod.Get, $"{PROJECT_URL}/releases/latest");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Rhythia", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", API_VERSION);

        var response = await client.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Logger.Log($"Could not get latest release: {ex.Message}");
            return null;
        }
        

        return JsonConvert.DeserializeObject<ReleaseInfo>(await response.Content.ReadAsStringAsync());
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ReleaseInfo
    {
        [JsonProperty(Required = Required.Always)]
        public string Url { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string TagName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool Prerelease { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<ReleaseAsset> Assets { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ReleaseAsset
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Url { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string BrowserDownloadUrl { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Digest { get; set; }
    }
}
