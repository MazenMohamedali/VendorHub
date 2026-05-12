namespace VendorHub.DTOs.sharedDto
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        public bool HasNextPage => Page < TotalPages;

        public bool HasPreviousPage => Page > 1;

        public int NextPage => HasNextPage ? Page + 1 : Page;

        public int PreviousPage => HasPreviousPage ? Page - 1 : Page;
    }

    public class PagedResult
    {
        public List<object> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
        public int NextPage => HasNextPage ? Page + 1 : Page;
        public int PreviousPage => HasPreviousPage ? Page - 1 : Page;
    }
}
