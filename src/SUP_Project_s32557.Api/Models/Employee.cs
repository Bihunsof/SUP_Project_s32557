using SUP_Project_s32557.Api.Models.Enums;

namespace SUP_Project_s32557.Api.Models;

public class Employee
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
}