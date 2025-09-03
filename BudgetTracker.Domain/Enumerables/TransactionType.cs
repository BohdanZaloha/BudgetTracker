using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Domain.Enumerables
{
    public enum TransactionType : byte
    {
        Expense = 0,
        Income = 1
    }
}
