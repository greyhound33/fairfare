using System.Text;
using System.Windows;
using FairFare.BLL;
using FairFare.Domain;

namespace FairFare.UI;

public partial class MainWindow : Window
{
    private readonly EventService _eventService = new();
    private readonly FinancialModule _financialModule = new();
    private readonly RouteService _routeService = new();
    private readonly ReportService _reportService = new();
    private TripEvent _trip;

    public MainWindow()
    {
        InitializeComponent();
        _trip = _eventService.CreateEvent("Weekend in Lviv");
        SeedDefaultTrip();
        RefreshView();
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
        _financialModule.AddExpense(_trip, "Hotel booking", 360m, payerId, _trip.Participants.Select(p => p.Id));
        _financialModule.AddExpense(_trip, "Dinner", 180m, _trip.Participants[1].Id, _trip.Participants.Select(p => p.Id));
    }

    private void RefreshView()
    {
        TripTitleTextBox.Text = _trip.Title;
        TripSummaryTextBlock.Text = BuildTripSummary();
        ParticipantsListBox.ItemsSource = _trip.Participants.Select(p => p.Name).ToList();
        PayerComboBox.ItemsSource = _trip.Participants.Select(p => p.Name).ToList();

        if (_trip.Participants.Count > 0 && (PayerComboBox.SelectedItem is null || !string.Equals((string)PayerComboBox.SelectedItem, _trip.Participants[0].Name, StringComparison.Ordinal)))
        {
            PayerComboBox.SelectedIndex = 0;
        }

        var balances = _financialModule.CalculateBalances(_trip);
        BalancesTextBlock.Text = BuildBalanceSummary(balances);
        ExpenseSummaryTextBlock.Text = BuildExpenseSummary();

        var transfers = _financialModule.OptimizeTransfers(_trip);
        RouteSummaryTextBlock.Text = BuildRouteSummary();
        ReportTextBlock.Text = BuildReportSummary(balances, transfers);
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

    private void CreateTripButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TripTitleTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Trip title cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _trip.Title = title;
        RefreshView();
    }

    private void AddParticipantButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ParticipantNameTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Participant name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _eventService.AddParticipant(_trip, name);
            ParticipantNameTextBox.Clear();
            RefreshView();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddExpenseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_trip.Participants.Count == 0)
        {
            MessageBox.Show("Add at least one participant before creating an expense.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var description = ExpenseDescriptionTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            MessageBox.Show("Expense description is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(ExpenseAmountTextBox.Text, out var amount))
        {
            MessageBox.Show("Expense amount must be numeric.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var payerName = PayerComboBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(payerName))
        {
            MessageBox.Show("Select a payer.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var payer = _trip.Participants.Single(participant => participant.Name == payerName);
        var debtorIds = _trip.Participants.Select(participant => participant.Id).ToList();

        try
        {
            _financialModule.AddExpense(_trip, description, amount, payer.Id, debtorIds);
            RefreshView();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddRouteButton_Click(object sender, RoutedEventArgs e)
    {
        var locationName = RouteLocationTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(locationName))
        {
            MessageBox.Show("Route location name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var orderText = RouteOrderTextBox.Text?.Trim();
        var order = int.TryParse(orderText, out var parsedOrder) ? parsedOrder : _trip.RouteLocations.Count + 1;

        try
        {
            _routeService.AddLocation(_trip, locationName, order);
            RouteLocationTextBox.Clear();
            RouteOrderTextBox.Text = (_trip.RouteLocations.Count + 1).ToString();
            RefreshView();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
    {
        var balances = _financialModule.CalculateBalances(_trip);
        var transfers = _financialModule.OptimizeTransfers(_trip);
        ReportTextBlock.Text = BuildReportSummary(balances, transfers);
        _reportService.CreateReport(_trip, balances, transfers);
    }
}
