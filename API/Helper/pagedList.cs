using Microsoft.EntityFrameworkCore;

namespace API.Helper
{
    public class pagedList<T>:List<T>
    {
        public pagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages =(int)Math.Ceiling(count/(double)pageSize); 
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items);
        }

        public int CurrentPage { get; set; }  //5  15    10 2-1 1*5 6 
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public static async Task<pagedList<T>> CreateAsync(IQueryable<T> source,int pageNumber,int pageSize)
        {
            var count = await source.CountAsync();
            var items=await source.Skip((pageNumber -1)*pageSize).Take(pageSize).ToListAsync();

            return new pagedList<T>(items, count, pageNumber, pageSize); 
        }
    }
}
