using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.Categories
{
    public class GetCategoriesInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public CategoryType? CategoryType { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public GetCategoriesInput()
        {

        }
    }
}