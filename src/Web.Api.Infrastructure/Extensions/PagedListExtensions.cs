using System;
using System.Collections.Generic;
using System.Linq;
using Web.Api.Core.Domain;
using AutoMapper;

namespace Web.Api.Infrastructure.Extensions
{
  public static class PagedListExtensions
  {
    public static PagedResult<U> GetPaged<T, U>(this IQueryable<T> query,
                                                IMapper mapper,
                                                int page, int pageSize) where U: class
    {
        var result = new PagedResult<U>();
        result.CurrentPage = page;
        result.PageSize = pageSize;
        result.RowCount = query.Count();
     
        var pageCount = (double)result.RowCount / pageSize;
        result.PageCount = (int)Math.Ceiling(pageCount);
     
        var skip = (page - 1) * pageSize;
        result.Results = mapper.Map<List<U>>(query.Skip(skip)
                              .Take(pageSize)
                              .ToList());
        return result;
    }
  }
}