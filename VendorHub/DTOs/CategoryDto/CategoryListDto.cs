namespace VendorHub.DTOs.CategoryDto
{
    public class CategoryListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
    }
}
