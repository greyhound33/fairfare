namespace FairFare.DAL;

public static class FairFareDb
{
    // Лише для локальної розробки в Docker. Для іншого сервера задайте змінну середовища.
    public const string DefaultConnectionString =
        "Host=localhost;Port=5433;Database=fairfare;Username=fairfare;Password=fairfare";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("FAIRFARE_CONNECTION") ?? DefaultConnectionString;
}
