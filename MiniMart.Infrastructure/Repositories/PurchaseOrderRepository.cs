using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Infrastructure.Repositories
{
    public class PurchaseOrderRepository : RepositoryBase<PurchaseOrder>, IPurchaseOrderRepository
    {
        public PurchaseOrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PurchaseOrder?> GetByReferenceId(string referenceId)
        {
            return await _context.PurchaseOrders.FirstOrDefaultAsync(x => x.TransactionReference == referenceId);
        }
    }
}
