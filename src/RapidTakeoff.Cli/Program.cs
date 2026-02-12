using RapidTakeoff.Core.Takeoff.Drywall;
using RapidTakeoff.Core.Takeoff.Insulation;
using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Cli;

/// <summary>
/// RapidTakeoff command line interface.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static int Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintHelp();
            return 0;
        }

        var command = args[0].Trim().ToLowerInvariant();

        try
        {
            return command switch
            {
                "drywall" => RunDrywall(args.Skip(1).ToArray()),
                "studs" => RunStuds(args.Skip(1).ToArray()),
                "insulation" => RunInsulation(args.Skip(1).ToArray()),
                _ => UnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] {ex.Message}");
            return 1;
        }
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"[ERROR] Unknown command: {command}");
        PrintHelp();
        return 1;
    }

    private static bool IsHelp(string s) =>
        s.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
        s.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
        s.Equals("help", StringComparison.OrdinalIgnoreCase);

    private static void PrintHelp()
    {
        Console.WriteLine("RapidTakeoff CLI");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  drywall    Drywall sheet takeoff from wall lengths and height");
        Console.WriteLine("  studs      Stud takeoff from wall lengths and spacing");
        Console.WriteLine("  insulation Insulation roll/bag takeoff from wall lengths and height");
        Console.WriteLine();

        Console.WriteLine("Usage (drywall):");
        Console.WriteLine("  rapid drywall --height-feet 8 --lengths-feet 12,10,12,10 --sheet 4x8 --waste 0.10");
        Console.WriteLine();
        Console.WriteLine("Options (drywall):");
        Console.WriteLine("  --height-feet <number>       Wall height in feet (required)");
        Console.WriteLine("  --lengths-feet <csv>         Comma-separated wall lengths in feet (required)");
        Console.WriteLine("  --sheet <4x8|4x12>           Sheet size (default: 4x8)");
        Console.WriteLine("  --waste <number>             Waste factor as fraction (default: 0.10)");
        Console.WriteLine("  --price-per-sheet <number>   Optional $/sheet for cost estimate");
        Console.WriteLine();

        Console.WriteLine("Usage (studs):");
        Console.WriteLine("  rapid studs --lengths-feet 12,10,12,10 --spacing-in 16 --waste 0.05");
        Console.WriteLine();
        Console.WriteLine("Options (studs):");
        Console.WriteLine("  --lengths-feet <csv>         Comma-separated wall lengths in feet (required)");
        Console.WriteLine("  --spacing-in <number>        Stud spacing in inches (required)");
        Console.WriteLine("  --waste <number>             Waste factor as fraction (default: 0.00)");
        Console.WriteLine("  --price-per-stud <number>    Optional $/stud for cost estimate");
        Console.WriteLine();

        Console.WriteLine("Usage (insulation):");
        Console.WriteLine("  rapid insulation --height-feet 8 --lengths-feet 12,10,12,10 --coverage-sqft 40 --waste 0.10");
        Console.WriteLine();
        Console.WriteLine("Options (insulation):");
        Console.WriteLine("  --height-feet <number>       Wall height in feet (required)");
        Console.WriteLine("  --lengths-feet <csv>         Comma-separated wall lengths in feet (required)");
        Console.WriteLine("  --coverage-sqft <number>     Coverage per roll/bag in sqft (required)");
        Console.WriteLine("  --waste <number>             Waste factor as fraction (default: 0.10)");
        Console.WriteLine("  --name <text>                Optional product name");
        Console.WriteLine("  --price-per-unit <number>    Optional $/roll or $/bag for cost estimate");
        Console.WriteLine();
    }

    private static int RunDrywall(string[] args)
    {
        var opts = ParseOptions(args);

        var heightFeet = GetRequiredDouble(opts, "--height-feet");
        var lengthsCsv = GetRequiredString(opts, "--lengths-feet");
        var waste = GetOptionalDouble(opts, "--waste", 0.10);

        var sheetToken = GetOptionalString(opts, "--sheet", "4x8").Trim().ToLowerInvariant();
        var sheet = sheetToken switch
        {
            "4x8" => DrywallSheet.Sheet4x8,
            "4x12" => DrywallSheet.Sheet4x12,
            _ => throw new ArgumentOutOfRangeException("--sheet", "Sheet must be '4x8' or '4x12'.")
        };

        var pricePerSheet = GetOptionalNullableDouble(opts, "--price-per-sheet");

        var height = Length.FromFeet(heightFeet);
        var lengths = ParseCsvDoubles(lengthsCsv);
        if (lengths.Count == 0)
            throw new ArgumentOutOfRangeException("--lengths-feet", "At least one wall length is required.");

        var totalLength = Length.FromFeet(lengths.Sum());
        var netArea = Area.FromRectangle(totalLength, height);

        var result = DrywallTakeoffCalculator.Calculate(netArea, sheet, waste);

        PrintDrywallReport(heightFeet, lengths, result, pricePerSheet);
        return 0;
    }

    private static void PrintDrywallReport(double heightFeet, List<double> lengthsFeet, DrywallTakeoffResult result, double? pricePerSheet)
    {
        var totalLen = lengthsFeet.Sum();

        Console.WriteLine("========================================");
        Console.WriteLine("RapidTakeoff - Drywall Takeoff");
        Console.WriteLine("========================================");
        Console.WriteLine($"Walls (ft): {string.Join(" + ", lengthsFeet.Select(x => x.ToString("0.###")))}");
        Console.WriteLine($"Total length: {totalLen:0.###} ft");
        Console.WriteLine($"Height: {heightFeet:0.###} ft");
        Console.WriteLine();
        Console.WriteLine($"Net area:   {result.NetArea.TotalSquareFeet:0.###} sqft");
        Console.WriteLine($"Waste:      {result.WasteFactor:0.###}");
        Console.WriteLine($"Gross area: {result.GrossArea.TotalSquareFeet:0.###} sqft");
        Console.WriteLine();
        Console.WriteLine($"Sheet:      {result.Sheet.Width.TotalFeet:0.###}x{result.Sheet.Height.TotalFeet:0.###} ft ({result.Sheet.Area.TotalSquareFeet:0.###} sqft)");
        Console.WriteLine($"Sheets:     {result.SheetCount}");

        if (pricePerSheet is not null)
        {
            var cost = result.SheetCount * pricePerSheet.Value;
            Console.WriteLine($"Price/sheet: ${pricePerSheet.Value:0.##}");
            Console.WriteLine($"Est. cost:   ${cost:0.##}");
        }

        Console.WriteLine("========================================");
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];

            if (!a.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"Unexpected token '{a}'. Options must start with --.");

            if (i + 1 >= args.Length)
                throw new ArgumentException($"Missing value for option '{a}'.");

            var value = args[i + 1];
            if (value.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"Missing value for option '{a}'.");

            dict[a] = value;
            i++;
        }

        return dict;
    }

    private static string GetRequiredString(Dictionary<string, string> opts, string key)
    {
        if (!opts.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            throw new ArgumentOutOfRangeException(key, $"{key} is required.");
        return value;
    }

    private static double GetRequiredDouble(Dictionary<string, string> opts, string key)
    {
        var s = GetRequiredString(opts, key);
        if (!double.TryParse(s, out var v))
            throw new ArgumentOutOfRangeException(key, $"{key} must be a number.");
        return v;
    }

    private static double GetOptionalDouble(Dictionary<string, string> opts, string key, double defaultValue)
    {
        if (!opts.TryGetValue(key, out var s))
            return defaultValue;

        if (!double.TryParse(s, out var v))
            throw new ArgumentOutOfRangeException(key, $"{key} must be a number.");

        return v;
    }

    private static string GetOptionalString(Dictionary<string, string> opts, string key, string defaultValue) =>
        opts.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : defaultValue;

    private static double? GetOptionalNullableDouble(Dictionary<string, string> opts, string key)
    {
        if (!opts.TryGetValue(key, out var s))
            return null;

        if (!double.TryParse(s, out var v))
            throw new ArgumentOutOfRangeException(key, $"{key} must be a number.");

        return v;
    }

    private static string? GetOptionalNullableString(Dictionary<string, string> opts, string key)
    {
        if (!opts.TryGetValue(key, out var s))
            return null;

        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    private static List<double> ParseCsvDoubles(string csv)
    {
        var parts = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var vals = new List<double>(parts.Length);

        foreach (var p in parts)
        {
            if (!double.TryParse(p, out var v))
                throw new ArgumentOutOfRangeException("--lengths-feet", $"Invalid number '{p}'.");
            if (v < 0)
                throw new ArgumentOutOfRangeException("--lengths-feet", "Lengths cannot be negative.");
            vals.Add(v);
        }

        return vals;
    }

    private static int RunStuds(string[] args)
    {
        var opts = ParseOptions(args);

        var lengthsCsv = GetRequiredString(opts, "--lengths-feet");
        var spacingIn = GetRequiredDouble(opts, "--spacing-in");
        var waste = GetOptionalDouble(opts, "--waste", 0.0);
        var pricePerStud = GetOptionalNullableDouble(opts, "--price-per-stud");

        var lengthsFeet = ParseCsvDoubles(lengthsCsv);
        if (lengthsFeet.Count == 0)
            throw new ArgumentOutOfRangeException("--lengths-feet", "At least one wall length is required.");

        var wallLengths = lengthsFeet
            .Select(Length.FromFeet)
            .ToArray();

        var spacing = Length.FromInches(spacingIn);

        var result = RapidTakeoff.Core.Takeoff.Studs.StudTakeoffCalculator
            .Calculate(wallLengths, spacing, waste);

        PrintStudReport(lengthsFeet, spacingIn, result, pricePerStud);
        return 0;
    }

    private static void PrintStudReport(
        List<double> lengthsFeet,
        double spacingIn,
        RapidTakeoff.Core.Takeoff.Studs.StudTakeoffResult result,
        double? pricePerStud)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("RapidTakeoff - Stud Takeoff");
        Console.WriteLine("========================================");

        Console.WriteLine($"Walls (ft): {string.Join(" + ", lengthsFeet.Select(x => x.ToString("0.###")))}");
        Console.WriteLine($"Spacing: {spacingIn:0.###} in OC");
        Console.WriteLine();

        for (var i = 0; i < result.StudsPerWall.Count; i++)
        {
            Console.WriteLine($"Wall {i + 1}: {result.StudsPerWall[i]} studs");
        }

        Console.WriteLine();
        Console.WriteLine($"Base studs: {result.BaseStuds}");
        Console.WriteLine($"Waste:      {result.WasteFactor:0.###}");
        Console.WriteLine($"Total studs: {result.TotalStuds}");

        if (pricePerStud is not null)
        {
            var cost = result.TotalStuds * pricePerStud.Value;
            Console.WriteLine($"Price/stud: ${pricePerStud.Value:0.##}");
            Console.WriteLine($"Est. cost:  ${cost:0.##}");
        }

        Console.WriteLine("========================================");
    }

    private static int RunInsulation(string[] args)
    {
        var opts = ParseOptions(args);

        var heightFeet = GetRequiredDouble(opts, "--height-feet");
        var lengthsCsv = GetRequiredString(opts, "--lengths-feet");
        var coverageSqFt = GetRequiredDouble(opts, "--coverage-sqft");
        var waste = GetOptionalDouble(opts, "--waste", 0.10);
        var name = GetOptionalNullableString(opts, "--name");
        var pricePerUnit = GetOptionalNullableDouble(opts, "--price-per-unit");

        var lengthsFeet = ParseCsvDoubles(lengthsCsv);
        if (lengthsFeet.Count == 0)
            throw new ArgumentOutOfRangeException("--lengths-feet", "At least one wall length is required.");

        var totalLength = Length.FromFeet(lengthsFeet.Sum());
        var height = Length.FromFeet(heightFeet);
        var netArea = Area.FromRectangle(totalLength, height);

        var product = new InsulationProduct(Area.FromSquareFeet(coverageSqFt), name);
        var result = InsulationTakeoffCalculator.Calculate(netArea, product, waste);

        PrintInsulationReport(heightFeet, lengthsFeet, coverageSqFt, result, pricePerUnit);
        return 0;
    }

    private static void PrintInsulationReport(
        double heightFeet,
        List<double> lengthsFeet,
        double coverageSqFt,
        InsulationTakeoffResult result,
        double? pricePerUnit)
    {
        var totalLen = lengthsFeet.Sum();

        Console.WriteLine("========================================");
        Console.WriteLine("RapidTakeoff - Insulation Takeoff");
        Console.WriteLine("========================================");
        Console.WriteLine($"Walls (ft): {string.Join(" + ", lengthsFeet.Select(x => x.ToString("0.###")))}");
        Console.WriteLine($"Total length: {totalLen:0.###} ft");
        Console.WriteLine($"Height: {heightFeet:0.###} ft");
        Console.WriteLine();
        Console.WriteLine($"Product:    {result.Product.Name ?? "Insulation"}");
        Console.WriteLine($"Coverage:   {coverageSqFt:0.###} sqft per unit");
        Console.WriteLine($"Net area:   {result.NetArea.TotalSquareFeet:0.###} sqft");
        Console.WriteLine($"Waste:      {result.WasteFactor:0.###}");
        Console.WriteLine($"Gross area: {result.GrossArea.TotalSquareFeet:0.###} sqft");
        Console.WriteLine($"Quantity:   {result.Quantity}");

        if (pricePerUnit is not null)
        {
            var cost = result.Quantity * pricePerUnit.Value;
            Console.WriteLine($"Price/unit: ${pricePerUnit.Value:0.##}");
            Console.WriteLine($"Est. cost:  ${cost:0.##}");
        }

        Console.WriteLine("========================================");
    }
}
