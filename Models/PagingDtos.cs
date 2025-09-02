namespace E_CommerceSystem.Models
{
    public class PagingDtos
    {


    public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

    public record ProductListDto(
        int Id,
        string ProductName,
        decimal Price, // add prop to Product later if needed
        string? ImageUrl, // add prop to Product later if needed
        string Category, // add prop to Product later if needed
        string Supplier);

}
}
