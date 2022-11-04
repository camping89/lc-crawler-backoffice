using System;
using System.Collections.Generic;
using LC.Crawler.BackOffice.Enums;

namespace LC.Crawler.BackOffice.Payloads;

public class CrawlEcommercePayload
{
    public CrawlEcommercePayload()
    {
        Products = new List<CrawlEcommerceProductPayload>();
    }
    public string Url { get; set; }
    public List<CrawlEcommerceProductPayload> Products { get; set; }
}

public class CrawlEcommerceProductPayload
{    
    public string                           Url              { get; set; }
    public  string                          Category         { get; set; }
    public  string                          Title            { get; set; }
    public  string                          Code             { get; set; }
    public  List<string>                    ImageUrls        { get; set; }
    public  string                          ShortDescription { get; set; }
    public  string                          Description      { get; set; }
    public  List<EcommerceProductVariant>   Variants         { get; set; }
    public  List<EcommerceProductAttribute> Attributes       { get; set; }
    public  List<EcommerceProductReview>    Reviews          { get; set; }
    public  List<EcommerceProductComment>   Comments         { get; set; }
    public  int                             TotalReview      => Reviews.Count; 
    public  int                             TotalComment     => Comments.Count;
    public string Brand { get; set; }
    public List<string> Tags { get; set; }
    public int TotalProducts { get; set; }
    public CrawlEcommerceProductPayload()
    {
        Variants   = new List<EcommerceProductVariant>();
        Attributes = new List<EcommerceProductAttribute>();
        ImageUrls  = new List<string>();
        Reviews    = new List<EcommerceProductReview>();
        Comments   = new List<EcommerceProductComment>();
        Tags = new List<string>();
    }
}

public class EcommerceProductVariant
{
    public Guid    Id              { get; set; }
    public string  SKU             { get; set; }
    public decimal RetailPrice     { get; set; }
    public double  DiscountRate    { get; set; }
    public decimal DiscountedPrice => RetailPrice * (decimal)(1 - DiscountRate);
}

public class EcommerceProductAttribute
{
    public Guid                Id    { get; set; }
    public string              Slug  { get; set; }
    public string Key   { get; set; }
    public string              Value { get; set; }
}

public class EcommerceProductReview
{
    public string Name { get; set; }
    public string Content { get; set; }
    public decimal Rating { get; set; }
    public int Likes { get; set; }
}

public class EcommerceProductComment
{
    public string Name { get; set; }
    public string Content { get; set; }
    public decimal Rating { get; set; }
    public int Likes { get; set; }
}