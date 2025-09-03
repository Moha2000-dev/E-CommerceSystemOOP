namespace E_CommerceSystem.Models
{
    public class UpdateProductDTO
    {
        public int PID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
