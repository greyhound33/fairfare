using FairFare.Domain.Entities;
using FairFare.Domain.Models;

namespace FairFare.BLL.Interfaces;

public interface IReportService
{
    public Report CreateReport(TripEvent tripEvent, IReadOnlyList<BalanceEntry> balances, IReadOnlyList<Transfer> transfers);
}
