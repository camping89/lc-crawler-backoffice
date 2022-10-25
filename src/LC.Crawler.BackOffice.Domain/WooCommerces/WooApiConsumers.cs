using System;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.DataSources;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.WooCommerces;

public class WooApiConsumers : DomainService
{
    public async Task GetProductBrandApi(DataSource dataSource)
    {
        var client = new RestClient($"{dataSource.PostToSite}/wp-json/wc/v3/products/brands");
        var request = new RestRequest();

        var authenticationString = $"{dataSource.Configuration.Username}:{dataSource.Configuration.Password}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
        
        request.AddHeader("Authorization", $"Basic {base64EncodedAuthenticationString}");
        var response = await client.ExecuteAsync(request);
        Console.WriteLine(response.Content);
    }
    
    public async Task GetArticleBrandApi(DataSource dataSource)
    {
        var client = new RestClient($"{dataSource.PostToSite}/wp-json/wp/v2/brands");
        var request = new RestRequest();

        var authenticationString = $"{dataSource.Configuration.Username}:{dataSource.Configuration.Password}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
        
        request.AddHeader("Authorization", $"Basic {base64EncodedAuthenticationString}");
        var response = await client.ExecuteAsync(request);
        Console.WriteLine(response.Content);
    }
}