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
    public Guid Id { get; } = Guid.NewGuid();
    public TravelPackage PackageItem { get; }
    public int Days { get; }
    public int Travelers { get; }
    public DateTime BookingDate { get; }
    public decimal TotalCost { get; }

    public decimal Subtotal => PackageItem.CalculateSubtotal(Days, Travelers);
    public decimal Fees => TotalCost - Subtotal;

    public Booking(TravelPackage packageItem, int days, int travelers)
    {
        PackageItem = packageItem;
        Days = days;
        Travelers = travelers;
        BookingDate = DateTime.Now;
        TotalCost = packageItem.CalculateTotal(days, travelers); // auto-calculated
    }
}

// ENCAPSULATION: list is private, exposed only through methods
public class ItineraryService
{
    private readonly List<Booking> _bookings = new();
    private readonly List<TravelPackage> _packages;

    public ItineraryService()
    {
        _packages = new List<TravelPackage>
        {
            new AdventurePackage { Id = 1, Name = "Himalaya Sky Trek", Country = "Nepal", BaseRatePerDay = 120m,
                DifficultyLevel = "Extreme", GearFeePerPerson = 85m,
                ImageUrl = Art("🏔️", "#38bdf8", "#6366f1"),
                Description = "Climb above the clouds on legendary trails with sherpa guides and sunrise views you'll never forget!" },
            new AdventurePackage { Id = 2, Name = "Amazon River Rafting", Country = "Brazil", BaseRatePerDay = 95m,
                DifficultyLevel = "Moderate", GearFeePerPerson = 40m,
                ImageUrl = Art("🛶", "#4ade80", "#0d9488"),
                Description = "Paddle through wild rapids, spot toucans and pink dolphins, and camp under a billion stars." },
            new AdventurePackage { Id = 3, Name = "Iceland Glacier Walk", Country = "Iceland", BaseRatePerDay = 140m,
                DifficultyLevel = "Moderate", GearFeePerPerson = 55m,
                ImageUrl = Art("🧊", "#67e8f9", "#3b82f6"),
                Description = "Strap on crampons and explore blue ice caves, then soak in a steamy geothermal lagoon." },
            new AdventurePackage { Id = 4, Name = "Patagonia Peak Chase", Country = "Chile", BaseRatePerDay = 160m,
                DifficultyLevel = "Extreme", GearFeePerPerson = 110m,
                ImageUrl = Art("🧗", "#fb923c", "#e11d48"),
                Description = "Jagged peaks, roaring winds and bragging rights. Only for the truly fearless!" },

            new ResortPackage { Id = 5, Name = "Maldives Overwater Villa", Country = "Maldives", BaseRatePerDay = 450m,
                ImageUrl = Art("🏝️", "#22d3ee", "#2dd4bf"),
                Description = "Wake up over crystal water, snorkel from your deck and sip coconuts all day long." },
            new ResortPackage { Id = 6, Name = "Bali Beach Club Escape", Country = "Indonesia", BaseRatePerDay = 180m,
                ImageUrl = Art("🌴", "#fbbf24", "#f472b6"),
                Description = "Infinity pools, surf lessons, spa days and sunset parties on the sand." },
            new ResortPackage { Id = 7, Name = "Santorini Sunset Suites", Country = "Greece", BaseRatePerDay = 320m,
                ImageUrl = Art("🌅", "#f472b6", "#a78bfa"),
                Description = "White-washed cliffside suites with the most dreamy sunsets in the Aegean." },
            new ResortPackage { Id = 8, Name = "Boracay Bliss Resort", Country = "Philippines", BaseRatePerDay = 150m, ResortTaxRate = 0.10m,
                ImageUrl = Art("🏖️", "#facc15", "#38bdf8"),
                Description = "Powdery white sand, island hopping and fresh seafood by the shore." },
        };
    }

    // Offline-safe inline SVG artwork
    private static string Art(string emoji, string c1, string c2) =>
        "data:image/svg+xml;utf8," + Uri.EscapeDataString(
            $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 240'>" +
            $"<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'><stop offset='0' stop-color='{c1}'/><stop offset='1' stop-color='{c2}'/></linearGradient></defs>" +
            $"<rect width='400' height='240' fill='url(#g)'/>" +
            $"<circle cx='330' cy='50' r='60' fill='white' fill-opacity='.18'/>" +
            $"<circle cx='60' cy='200' r='80' fill='white' fill-opacity='.12'/>" +
            $"<text x='200' y='155' font-size='96' text-anchor='middle'>{emoji}</text></svg>");

    public IReadOnlyList<TravelPackage> GetPackages() => _packages;
    public TravelPackage? GetPackageById(int id) => _packages.FirstOrDefault(p => p.Id == id);

    public void AddBooking(Booking booking) => _bookings.Add(booking);
    public void RemoveBooking(Guid id) => _bookings.RemoveAll(b => b.Id == id);
    public IReadOnlyList<Booking> GetBookings() => _bookings;
    public decimal CalculateGrandTotal() => _bookings.Sum(b => b.TotalCost);
}