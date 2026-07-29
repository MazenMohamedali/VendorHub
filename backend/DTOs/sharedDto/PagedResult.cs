namespace VendorHub.DTOs.sharedDto
{
    public class PagedResult<T>
    {
        private const int MaxPageSize = 50; 
        private int _page = 1;
        private int _pageSize = 10;

        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }

        public int Page
        {
            get => _page;
            set => _page = value <= 0 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value <= 0 ? 10 : Math.Min(value, MaxPageSize);
        }

        public int TotalPages => TotalCount == 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
        public int NextPage => HasNextPage ? Page + 1 : Page;
        public int PreviousPage => HasPreviousPage ? Page - 1 : Page;
        public int SkipCount => (Page - 1) * PageSize;
    }
}
