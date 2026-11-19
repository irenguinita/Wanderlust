namespace FreshBlazorApp.Models;

// ABSTRACTION: base class cannot be instantiated
public abstract class TravelPackage
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public decimal BaseRatePerDay { get; set; }
    public string ImageUrl { get; set; } = "";
    public string Description { get; set; } = "";

    public abstract decimal CalculateTotal(int days, int travelers);
    public abstract string GetPackageType();

    // Shared by all subtypes; fees = CalculateTotal - CalculateSubtotal
    public virtual decimal CalculateSubtotal(int days, int travelers)
    {
        return BaseRatePerDay * days * travelers;
    }
}

// INHERITANCE + POLYMORPHISM
public class AdventurePackage : TravelPackage
{
    public string DifficultyLevel { get; set; } = "Moderate"; // "Moderate" | "Extreme"
    public decimal GearFeePerPerson { get; set; }

    public override decimal CalculateTotal(int days, int travelers)
        => (BaseRatePerDay * days * travelers) + (GearFeePerPerson * travelers);

    public override string GetPackageType() => "Adventure";
}

public class ResortPackage : TravelPackage
{
    public decimal ResortTaxRate { get; set; } = 0.12m;

    public override decimal CalculateTotal(int days, int travelers)
    {
        var subtotal = BaseRatePerDay * days * travelers;
        return subtotal + (subtotal * ResortTaxRate);
    }

    public override string GetPackageType() => "Resort";
}

public class Booking
{
    public Guid Id { get; }
    public TravelPackage PackageItem { get; }
    public int Days { get; }
    public int Travelers { get; }
    public DateTime BookingDate { get; }
    public decimal TotalCost { get; }

    public decimal Subtotal => PackageItem.CalculateSubtotal(Days, Travelers);
    public decimal Fees => TotalCost - Subtotal;

    // Used when the user makes a NEW booking
    public Booking(TravelPackage packageItem, int days, int travelers)
    {
        Id = Guid.NewGuid();
        PackageItem = packageItem;
        Days = days;
        Travelers = travelers;
        BookingDate = DateTime.Now;
        TotalCost = packageItem.CalculateTotal(days, travelers); // auto-calculated
    }

    // Used when loading an existing booking back from the database
    public Booking(Guid id, TravelPackage packageItem, int days, int travelers,
                   DateTime bookingDate, decimal totalCost)
    {
        Id = id;
        PackageItem = packageItem;
        Days = days;
        Travelers = travelers;
        BookingDate = bookingDate;
        TotalCost = totalCost;
    }
}

// ENCAPSULATION: the database client is private, exposed only through methods
public class ItineraryService
{
    private readonly Supabase.Client _supabase;
    private bool _initialized;
    private List<TravelPackage>? _packages; // cached after the first load

    public ItineraryService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        await _supabase.InitializeAsync();
        _initialized = true;
    }

    // ---------- Packages ----------

    public async Task<IReadOnlyList<TravelPackage>> GetPackagesAsync()
    {
        if (_packages is not null) return _packages;

        await EnsureInitializedAsync();
        var result = await _supabase.From<PackageRow>()
            .Order("id", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        _packages = result.Models.Select(ToPackage).ToList();
        return _packages;
    }

    public async Task<TravelPackage?> GetPackageByIdAsync(int id)
    {
        var packages = await GetPackagesAsync();
        return packages.FirstOrDefault(p => p.Id == id);
    }

    // Turns a database row into the right subclass (polymorphism)
    private static TravelPackage ToPackage(PackageRow r) => r.PackageType switch
    {
        "Adventure" => new AdventurePackage
        {
            Id = r.Id, Name = r.Name, Country = r.Country,
            BaseRatePerDay = r.BaseRatePerDay, Description = r.Description,
            ImageUrl = Art(r.Emoji, r.ColorFrom, r.ColorTo),
            DifficultyLevel = r.DifficultyLevel ?? "Moderate",
            GearFeePerPerson = r.GearFeePerPerson ?? 0m
        },
        "Resort" => new ResortPackage
        {
            Id = r.Id, Name = r.Name, Country = r.Country,
            BaseRatePerDay = r.BaseRatePerDay, Description = r.Description,
            ImageUrl = Art(r.Emoji, r.ColorFrom, r.ColorTo),
            ResortTaxRate = r.ResortTaxRate ?? 0.12m
        },
        _ => throw new InvalidOperationException($"Unknown package type: {r.PackageType}")
    };

    // Offline-safe inline SVG artwork
    private static string Art(string emoji, string c1, string c2) =>
        "data:image/svg+xml;utf8," + Uri.EscapeDataString(
            $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 240'>" +
            $"<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'><stop offset='0' stop-color='{c1}'/><stop offset='1' stop-color='{c2}'/></linearGradient></defs>" +
            $"<rect width='400' height='240' fill='url(#g)'/>" +
            $"<circle cx='330' cy='50' r='60' fill='white' fill-opacity='.18'/>" +
            $"<circle cx='60' cy='200' r='80' fill='white' fill-opacity='.12'/>" +
            $"<text x='200' y='155' font-size='96' text-anchor='middle'>{emoji}</text></svg>");

    // ---------- Bookings ----------

    public async Task<List<Booking>> GetBookingsAsync()
    {
        await EnsureInitializedAsync();
        var packages = await GetPackagesAsync();

        var result = await _supabase.From<BookingRow>()
            .Order("booking_date", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var bookings = new List<Booking>();
        foreach (var row in result.Models)
        {
            var package = packages.FirstOrDefault(p => p.Id == row.PackageId);
            if (package is null) continue;
            bookings.Add(new Booking(row.Id, package, row.Days, row.Travelers,
                                     row.BookingDate, row.TotalCost));
        }
        return bookings;
    }

    public async Task AddBookingAsync(Booking booking)
    {
        await EnsureInitializedAsync();
        await _supabase.From<BookingRow>().Insert(new BookingRow
        {
            Id = booking.Id,
            PackageId = booking.PackageItem.Id,
            Days = booking.Days,
            Travelers = booking.Travelers,
            BookingDate = booking.BookingDate,
            TotalCost = booking.TotalCost
        });
    }

    public async Task RemoveBookingAsync(Guid id)
    {
        await EnsureInitializedAsync();
        await _supabase.From<BookingRow>()
            .Where(b => b.Id == id)
            .Delete();
    }

    // Takes the list you already loaded, so no extra database call
    public decimal CalculateGrandTotal(IEnumerable<Booking> bookings)
        => bookings.Sum(b => b.TotalCost);
}