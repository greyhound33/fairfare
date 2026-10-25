using FairFare.BLL.Interfaces;
using FairFare.BLL.Services;
using FairFare.Domain.Entities;
using FairFare.Domain.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace FairFare.UI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IEventService _eventService;
    private readonly IFinancialService _financialService;
    private readonly IRouteService _routeService;
    private readonly IReportService _reportService;
    private readonly TripStorageService _storage;

    private TripEvent _trip;
    private string _tripTitle = "Weekend in Lviv";
    private string _participantName = string.Empty;
    private string _expenseDescription = "Dinner";
    private string _expenseAmount = "120";
    private string? _selectedPayer;
    private string _routeLocationName = "Rynok Square";
    private string _routeOrder = "1";
    private string _tripSummary = string.Empty;
    private string _balanceSummary = string.Empty;
    private string _expenseSummary = string.Empty;
    private string _routeSummary = string.Empty;
    private string _reportSummary = string.Empty;
    private string _errorMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> Participants { get; } = new();
    public ObservableCollection<string> Payers { get; } = new();

    public string TripTitle { get => _tripTitle; set => SetProperty(ref _tripTitle, value); }
    public string ParticipantName { get => _participantName; set => SetProperty(ref _participantName, value); }
    public string ExpenseDescription { get => _expenseDescription; set => SetProperty(ref _expenseDescription, value); }
    public string ExpenseAmount { get => _expenseAmount; set => SetProperty(ref _expenseAmount, value); }
    public string? SelectedPayer { get => _selectedPayer; set => SetProperty(ref _selectedPayer, value); }
    public string RouteLocationName { get => _routeLocationName; set => SetProperty(ref _routeLocationName, value); }
    public string RouteOrder { get => _routeOrder; set => SetProperty(ref _routeOrder, value); }
    public string TripSummary { get => _tripSummary; private set => SetProperty(ref _tripSummary, value); }
    public string BalanceSummary { get => _balanceSummary; private set => SetProperty(ref _balanceSummary, value); }
    public string ExpenseSummary { get => _expenseSummary; private set => SetProperty(ref _expenseSummary, value); }
    public string RouteSummary { get => _routeSummary; private set => SetProperty(ref _routeSummary, value); }
    public string ReportSummary { get => _reportSummary; private set => SetProperty(ref _reportSummary, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public ICommand CreateTripCommand { get; }
    public ICommand AddParticipantCommand { get; }
    public ICommand AddExpenseCommand { get; }
    public ICommand AddRouteLocationCommand { get; }
    public ICommand GenerateReportCommand { get; }

    public MainViewModel(
        IEventService eventService,
        IFinancialService financialService,
        IRouteService routeService,
        IReportService reportService,
        TripStorageService storage)
    {
        _eventService = eventService;
        _financialService = financialService;
        _routeService = routeService;
        _reportService = reportService;
        _storage = storage;

        var saved = Task.Run(() => _storage.LoadFirstAsync()).GetAwaiter().GetResult();
        if (saved is not null)
        {
            _trip = saved;
        }
        else
        {
            _trip = _eventService.CreateEvent(TripTitle);
            SeedDefaultTrip();
            Save();
        }

        CreateTripCommand = new RelayCommand(CreateTrip);
        AddParticipantCommand = new RelayCommand(AddParticipant);
        AddExpenseCommand = new RelayCommand(AddExpense);
        AddRouteLocationCommand = new RelayCommand(AddRouteLocation);
        GenerateReportCommand = new RelayCommand(GenerateReport);

        RefreshView();
    }

    private bool Save()
    {
        try
        {
            Task.Run(() => _storage.SaveAsync(_trip)).GetAwaiter().GetResult();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не вдалося зберегти в БД: " + ex.Message;
            return false;
        }
    }

    private void SeedDefaultTrip()
    {
        foreach (var participantName in new[] { "Anna", "Maksym", "Olena", "Taras" })
        {
            _eventService.AddParticipant(_trip, participantName);
        }

        _routeService.AddLocation(_trip, "Rynok Square", 1);
        _routeService.AddLocation(_trip, "Lychakiv Cemetery", 2);
        _routeService.AddLocation(_trip, "Castle Hill", 3);

        var payerId = _trip.Participants.First().Id;
        _financialService.AddExpense(_trip, "Hotel booking", 360m, payerId, _trip.Participants.Select(p => p.Id));
        _financialService.AddExpense(_trip, "Dinner", 180m, _trip.Participants[1].Id, _trip.Participants.Select(p => p.Id));
    }

    private void RefreshView()
    {
        var selectedPayer = SelectedPayer;
        Participants.Clear();
        Payers.Clear();
        foreach (var participant in _trip.Participants)
        {
            Participants.Add(participant.Name);
            Payers.Add(participant.Name);
        }

        SelectedPayer = selectedPayer is not null && Payers.Contains(selectedPayer)
            ? selectedPayer
            : Payers.FirstOrDefault();

        var balances = _financialService.CalculateBalances(_trip);
        TripTitle = _trip.Title;
        TripSummary = BuildTripSummary();
        BalanceSummary = BuildBalanceSummary(balances);
        ExpenseSummary = BuildExpenseSummary();

        var transfers = _financialService.OptimizeTransfers(_trip);
        RouteSummary = BuildRouteSummary();
        ReportSummary = BuildReportSummary(balances, transfers);
    }

    private string BuildTripSummary()
    {
        var participants = _trip.Participants.Select(participant => participant.Name);
        var lines = new List<string>
        {
            $"Trip: {_trip.Title}",
            $"Status: {(_trip.IsActive ? "Active" : "Completed")}",
            $"Participants: {string.Join(", ", participants)}",
            $"Expenses recorded: {_trip.Expenses.Count}",
            $"Route stops: {_trip.RouteLocations.Count}"
        };

        return string.Join(Environment.NewLine, lines);
    }

    private string BuildBalanceSummary(IReadOnlyList<BalanceEntry> balances)
    {
        if (balances.Count == 0)
        {
            return "No participants yet.";
        }

        var builder = new StringBuilder();
        foreach (var balance in balances)
        {
            builder.AppendLine($"{balance.ParticipantName}: {balance.Amount:C}");
        }

        return builder.ToString();
    }

    private string BuildExpenseSummary()
    {
        if (_trip.Expenses.Count == 0)
        {
            return "No expenses recorded yet.";
        }

        var builder = new StringBuilder();
        foreach (var expense in _trip.Expenses)
        {
            var payer = _trip.Participants.Single(p => p.Id == expense.PayerId).Name;
            builder.AppendLine($"{expense.Description}: {expense.TotalAmount:C} paid by {payer}");
        }

        return builder.ToString();
    }

    private string BuildRouteSummary()
    {
        if (_trip.RouteLocations.Count == 0)
        {
            return "No route locations yet.";
        }

        var builder = new StringBuilder();
        foreach (var location in _trip.RouteLocations.OrderBy(l => l.OrderIndex))
        {
            builder.AppendLine($"{location.OrderIndex}. {location.LocationName}");
        }

        return builder.ToString();
    }

    private string BuildReportSummary(IReadOnlyList<BalanceEntry> balances, IReadOnlyList<Transfer> transfers)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Trip report: {_trip.Title}");
        builder.AppendLine($"Created: {DateTime.Now:yyyy-MM-dd HH:mm}");
        builder.AppendLine();

        foreach (var balance in balances)
        {
            builder.AppendLine($"{balance.ParticipantName}: {balance.Amount:C}");
        }

        if (transfers.Count == 0)
        {
            builder.AppendLine();
            builder.AppendLine("No pending payments required.");
            return builder.ToString();
        }

        builder.AppendLine();
        foreach (var transfer in transfers)
        {
            var fromName = _trip.Participants.Single(participant => participant.Id == transfer.FromParticipantId).Name;
            var toName = _trip.Participants.Single(participant => participant.Id == transfer.ToParticipantId).Name;
            builder.AppendLine($"{fromName} pays {transfer.Amount:C} to {toName}.");
        }

        return builder.ToString();
    }

    private void CreateTrip()
    {
        var title = TripTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            ErrorMessage = "Trip title cannot be empty.";
            return;
        }

        _trip.Title = title;
        ErrorMessage = string.Empty;
        Save();
        RefreshView();
    }

    private void AddParticipant()
    {
        var name = ParticipantName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorMessage = "Participant name is required.";
            return;
        }

        try
        {
            _eventService.AddParticipant(_trip, name);
            ParticipantName = string.Empty;
            ErrorMessage = string.Empty;
            Save();
            RefreshView();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void AddExpense()
    {
        if (_trip.Participants.Count == 0)
        {
            ErrorMessage = "Add at least one participant before creating an expense.";
            return;
        }

        var description = ExpenseDescription.Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            ErrorMessage = "Expense description is required.";
            return;
        }

        if (!decimal.TryParse(ExpenseAmount, out var amount))
        {
            ErrorMessage = "Expense amount must be numeric.";
            return;
        }

        var payerName = SelectedPayer;
        if (string.IsNullOrWhiteSpace(payerName))
        {
            ErrorMessage = "Select a payer.";
            return;
        }

        var payer = _trip.Participants.Single(participant => participant.Name == payerName);
        var debtorIds = _trip.Participants.Select(participant => participant.Id).ToList();

        try
        {
            _financialService.AddExpense(_trip, description, amount, payer.Id, debtorIds);
            ErrorMessage = string.Empty;
            Save();
            RefreshView();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void AddRouteLocation()
    {
        var locationName = RouteLocationName.Trim();
        if (string.IsNullOrWhiteSpace(locationName))
        {
            ErrorMessage = "Route location name is required.";
            return;
        }

        var orderText = RouteOrder.Trim();
        var order = int.TryParse(orderText, out var parsedOrder) ? parsedOrder : _trip.RouteLocations.Count + 1;

        try
        {
            _routeService.AddLocation(_trip, locationName, order);
            RouteLocationName = string.Empty;
            RouteOrder = (_trip.RouteLocations.Count + 1).ToString();
            ErrorMessage = string.Empty;
            Save();
            RefreshView();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void GenerateReport()
    {
        var balances = _financialService.CalculateBalances(_trip);
        var transfers = _financialService.OptimizeTransfers(_trip);
        ReportSummary = BuildReportSummary(balances, transfers);
        _reportService.CreateReport(_trip, balances, transfers);
        Save();
        ErrorMessage = string.Empty;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}