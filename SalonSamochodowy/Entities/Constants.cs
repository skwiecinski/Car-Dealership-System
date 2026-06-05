namespace SalonSamochodowy.Entities
{
    public static class RoleNames
    {
        public const string Admin = "Administrator";
        public const string Kierownik = "Kierownik";
        public const string Sprzedawca = "Sprzedawca";
        public const string Serwisant = "Serwisant";
        public const string Klient = "Klient";
    }

    public static class JobStatuses
    {
        public const string Pending = "Oczekujące";
        public const string InProgress = "W trakcie";
        public const string Finished = "Zakończone";
    }

    public static class OrderStatuses
    {
        public const string Pending = "Oczekujące";
        public const string InProgress = "W trakcie";
        public const string Finished = "Zrealizowane";
        public const string FinishedAlt = "Sfinalizowane";
        public const string Reserved = "Zarezerwowane";
        public const string Canceled = "Anulowane";
    }
}
