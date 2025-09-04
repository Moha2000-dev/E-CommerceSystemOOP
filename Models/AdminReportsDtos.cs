namespace E_CommerceSystem.DTOs
{
    public class BestSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RevenuePointDto { public DateTime Date { get; set; } public decimal Revenue { get; set; } }
    public class RevenueMonthDto { public int Year { get; set; } public int Month { get; set; } public decimal Revenue { get; set; } }

    public class TopRatedProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public double AvgRating { get; set; }
        public int ReviewsCount { get; set; }
    }

    public class ActiveCustomerDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
