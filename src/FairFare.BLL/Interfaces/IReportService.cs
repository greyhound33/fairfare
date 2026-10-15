using FairFare.Domain.Entities;

namespace FairFare.BLL.Interfaces;

public interface IReportService
{
    public Report CreateReport(TripEvent tripEvent, IReadOnlyList<BalanceEntry> balances, IReadOnlyList<Transfer> transfers);
}
