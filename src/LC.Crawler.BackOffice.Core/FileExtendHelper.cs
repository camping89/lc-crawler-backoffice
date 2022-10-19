using System.Net.Http.Headers;

namespace LC.Crawler.BackOffice.Core;

public static class FileExtendHelper
{
    public static async Task<byte[]?> DownloadFile(string url)
    {
        try
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            clientHandler.UseDefaultCredentials = true;

            using var client = new HttpClient(clientHandler);
            client.DefaultRequestHeaders.Add("User-Agent", "Other");
            using var result = await client.GetAsync(url);
            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsByteArrayAsync();
            }
        }
        catch (Exception e)
        {
            return null;
        }

        return null;
    }
}