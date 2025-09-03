﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public enum OrderStatus { Pending = 0, Paid = 1, Shipped = 2, Delivered = 3, Cancelled = 4 }
    public class Order
    {
        [Key] 
        public int OID { get; set; }
        // This ensures EF will throw a DbUpdateConcurrencyException if two users try to update the same order at once
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        [ForeignKey("user")]
        public int UID { get; set; }
        public User user { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [JsonIgnore]
        public virtual ICollection <OrderProducts> OrderProducts { get; set; }
    }
}
