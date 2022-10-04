using System;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ProductAttributes;

public class ProductAttributeResultDto : EntityDto<Guid>
{
    public string Key { get; set; }
    public string Value { get; set; }

}