namespace E_CommerceSystem.Models
{
    public class CategoryDtos
    {
        public record CategoryDto(int Id, string Name, string? Description); // Basic info about category
        public record CreateCategoryDto(string Name, string? Description); // Info needed to create a new category
        public record UpdateCategoryDto(string Name, string? Description);  // Info needed to update an existing category
    } 
}
