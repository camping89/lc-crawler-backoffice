using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.Categories;

public interface ICategoryLongChauRepository : IRepository<Category, Guid>
{
        
}