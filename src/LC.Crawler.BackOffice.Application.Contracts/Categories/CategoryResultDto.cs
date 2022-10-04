using System;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.Categories;

public class CategoryResultDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Slug { get; set; }
}