namespace E_CommerceSystem.Models
{
    public class UpdateOrderDTO
    {
        public int OID { get; set; }
        public OrderStatus Status { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
