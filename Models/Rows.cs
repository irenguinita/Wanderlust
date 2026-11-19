using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FreshBlazorApp.Models;

[Table("packages")]
public class PackageRow : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("package_type")] public string PackageType { get; set; } = "";
    [Column("name")] public string Name { get; set; } = "";
    [Column("country")] public string Country { get; set; } = "";
    [Column("base_rate_per_day")] public decimal BaseRatePerDay { get; set; }
    [Column("description")] public string Description { get; set; } = "";
    [Column("emoji")] public string Emoji { get; set; } = "";
    [Column("color_from")] public string ColorFrom { get; set; } = "";
    [Column("color_to")] public string ColorTo { get; set; } = "";
    [Column("difficulty_level")] public string? DifficultyLevel { get; set; }
    [Column("gear_fee_per_person")] public decimal? GearFeePerPerson { get; set; }
    [Column("resort_tax_rate")] public decimal? ResortTaxRate { get; set; }
}

[Table("bookings")]
public class BookingRow : BaseModel
{
    // true = we send our own Guid when inserting
    [PrimaryKey("id", true)]
    public Guid Id { get; set; }

    [Column("package_id")] public int PackageId { get; set; }
    [Column("days")] public int Days { get; set; }
    [Column("travelers")] public int Travelers { get; set; }
    [Column("booking_date")] public DateTime BookingDate { get; set; }
    [Column("total_cost")] public decimal TotalCost { get; set; }
}