# ✈️ Wanderlust

A retro airport-themed travel booking app built with **Blazor Web App (.NET 8)** and **Tailwind CSS v4**. Browse destinations on a departures board, configure a trip, and manage your tickets.

## Features

- **Departures board**: search, filter (Expeditions / Retreats) and preview fares by stay length
- **Trip configurator**: set days and travelers with a live cost breakdown
- **My Tickets**: view booked trips, totals, and cancel with a confirmation dialog
  
## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for Tailwind CSS)

## Getting Started

From the project folder (`FreshBlazorApp`):

```bash
# 1. Install dependencies (first time only)
npm install
```

Then run these in **two separate terminals**:

**Terminal 1: Tailwind CSS watcher**
```bash
npm run watch:css
```

**Terminal 2: Blazor app with hot reload**
```bash
dotnet watch
```

The app opens in your browser. If it doesn't, check the URL shown in the terminal (usually `https://localhost:xxxx`).

## Project Structure

```
FreshBlazorApp/
├── Components/
│   ├── Layout/        # MainLayout
│   ├── Pages/         # Home, Details, Itinerary
│   └── Shared/        # ConfirmDialog
├── Models/            # TravelPackage, Booking, ItineraryService
├── wwwroot/css/       # Generated app.min.css
└── Program.cs
```

## Notes

- Bookings are stored **in memory**, so they reset when the app restarts.
- If a new file isn't picked up, stop `dotnet watch` and start it again.
