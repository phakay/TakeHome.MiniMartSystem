﻿using System.ComponentModel.DataAnnotations;

namespace MiniMart.Domain.Models
{
    public class TransactionQueryLog : EntityBase
    {
        public string RefId { get; set; }
        public TransactionStatus Status { get; set; }
        public string? StatusMessage { get; set; }
        public DateTime? LastChecked { get; set; }
        public DateTime LogDate { get; set; }
        public int RetryCount { get; set; }
        public decimal Amount { get; set; }
        [Timestamp] // ensure optimistic concurrency control
        public byte[] RowVersion { get; set; }
    }
}
