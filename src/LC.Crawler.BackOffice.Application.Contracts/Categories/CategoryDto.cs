using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Categories
{
    public class CategoryDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}