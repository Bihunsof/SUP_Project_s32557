using SUP_Project_s32557.Api.Models.Enums;

namespace SUP_Project_s32557.Api.Models;

public class Discount
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OfferType OfferType { get; set; }
    public decimal Percentage { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly ActiveTo { get; set; }
}