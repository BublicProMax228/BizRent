namespace BizRent.Data
{
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static string Username { get; set; }
        public static string FullName { get; set; }
        public static int RoleId { get; set; }
        public static string RoleName { get; set; }

        public static bool IsAdmin => RoleId == 1;
        public static bool IsManager => RoleId == 2;
        public static bool IsAccountant => RoleId == 3;

        public static void Init(Users user)
        {
            Id = user.Id;
            Username = user.Username;
            FullName = $"{user.LastName} {user.FirstName}";
            RoleId = user.RoleId;
            RoleName = user.Roles?.Name ?? "Неизвестно";
        }

        public static void Clear()
        {
            Id = 0;
            Username = string.Empty;
            FullName = string.Empty;
            RoleId = 0;
            RoleName = string.Empty;
        }

        public static string GetInfo()
        {
            return $"{FullName} ({RoleName})";
        }
    }
}