namespace LC.Crawler.BackOffice.Core;

public static class FileExtendHelper
{
    public static async Task<byte[]?> DownloadFile(string url)
    {
        try
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            using var client = new HttpClient(clientHandler);
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