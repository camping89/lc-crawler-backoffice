using System;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public class ProductAttributeDto : EntityDto<Guid>
    {
        public string Slug { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public Guid ProductId { get; set; }

    }
}