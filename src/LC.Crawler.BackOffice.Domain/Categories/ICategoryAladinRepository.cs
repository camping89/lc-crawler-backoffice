using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.Categories;

public interface ICategoryAladinRepository : IRepository<Category, Guid>
{
        
}