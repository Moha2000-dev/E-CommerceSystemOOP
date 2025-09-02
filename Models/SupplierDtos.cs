namespace E_CommerceSystem.Models
{
    public class SupplierDtos
    {

    public record SupplierDto(int Id, string Name, string ContactEmail, string? Phone);
    public record CreateSupplierDto(string Name, string ContactEmail, string? Phone);
    public record UpdateSupplierDto(string Name, string ContactEmail, string? Phone);

}
}
