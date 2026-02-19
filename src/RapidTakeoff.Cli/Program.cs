using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using RapidTakeoff.Core.Projects;
using RapidTakeoff.Core.Takeoff.Drywall;
using RapidTakeoff.Core.Takeoff.Insulation;
using RapidTakeoff.Core.Takeoff.Studs;
using RapidTakeoff.Core.Units;
using RapidTakeoff.Rendering.WallStrips;
using RapidTakeoff.Rendering.Walls;

namespace RapidTakeoff.Cli;

/// <summary>
/// RapidTakeoff command line interface.
/// </summary>
public static class Program
{
    private static readonly Regex CommandTokenRegex = new("\"([^\"]*)\"|(\\S+)", RegexOptions.Compiled);

    /// <summary>
    /// Application entry point.
    /// </summary>
    public static int Main(string[] args)
    {
        return Run(args, allowInteractivePrompt: true);
    }

    private static int Run(string[] args, bool allowInteractivePrompt)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintHelp();
            if (allowInteractivePrompt && TryRunInteractivePrompt())
                return 0;
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
                "estimate" => RunEstimate(args.Skip(1).ToArray()),
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
        Console.WriteLine("  estimate   Project estimate from a JSON file");
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

        Console.WriteLine("Usage (estimate):");
        Console.WriteLine("  rapid estimate --project .\\examples\\exampleproject.json --format text");
        Console.WriteLine("  rapid estimate --project .\\examples\\exampleproject.json --format csv");
        Console.WriteLine("  rapid estimate --project .\\examples\\exampleproject2.json --format svg --out .\\out\\estimate.svg");
        Console.WriteLine();
        Console.WriteLine("Options (estimate):");
        Console.WriteLine("  --project <path>             Path to project JSON file (required)");
        Console.WriteLine("  --format <text|csv|svg>      Output format (default: text)");
        Console.WriteLine("  --out <path>                 Required when --format svg (output .svg path)");
        Console.WriteLine("                               SVG writes a wall-strip diagram to the --out file");
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

    private static int RunEstimate(string[] args)
    {
        var opts = ParseOptions(args);
        var projectPath = GetRequiredString(opts, "--project");
        var formatToken = GetOptionalString(opts, "--format", "text").Trim().ToLowerInvariant();
        var svgOutputPath = GetOptionalNullableString(opts, "--out");

        if (formatToken == "svg" && string.IsNullOrWhiteSpace(svgOutputPath))
            throw new ArgumentOutOfRangeException("--out", "--out is required when --format is 'svg'.");

        if (!File.Exists(projectPath))
            throw new FileNotFoundException("Project file was not found.", projectPath);

        var json = File.ReadAllText(projectPath);
        var project = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (project is null)
            throw new InvalidOperationException("Project file could not be parsed.");

        project.Validate();
        var validationWarnings = project.GetValidationWarnings();

        var wallLengths = project.WallLengthsFeet
            .Select(Length.FromFeet)
            .ToArray();
        var grossAreaSqFt = project.GetGrossWallAreaSquareFeet();
        var penetrationAreaSqFt = project.GetPenetrationAreaSquareFeet();
        var netArea = Area.FromSquareFeet(project.GetNetWallAreaSquareFeet());

        var sheet = project.Settings.DrywallSheet switch
        {
            "4x8" => DrywallSheet.Sheet4x8,
            "4x12" => DrywallSheet.Sheet4x12,
            _ => throw new ArgumentOutOfRangeException(nameof(project.Settings.DrywallSheet), "Drywall sheet must be '4x8' or '4x12'.")
        };

        var drywall = DrywallTakeoffCalculator.Calculate(netArea, sheet, project.Settings.DrywallWaste);
        var studs = StudTakeoffCalculator.Calculate(
            wallLengths,
            Length.FromInches(project.Settings.StudsSpacingInches),
            project.Settings.StudsWaste);
        var insulation = InsulationTakeoffCalculator.Calculate(
            netArea,
            new InsulationProduct(Area.FromSquareFeet(project.Settings.InsulationCoverageSquareFeet), "Insulation"),
            project.Settings.InsulationWaste);

        return formatToken switch
        {
            "text" => PrintEstimateText(project, netArea, grossAreaSqFt, penetrationAreaSqFt, validationWarnings, drywall, studs, insulation),
            "csv" => PrintEstimateCsv(project, netArea, grossAreaSqFt, penetrationAreaSqFt, validationWarnings, drywall, studs, insulation),
            "svg" => PrintEstimateSvg(project, netArea, grossAreaSqFt, penetrationAreaSqFt, validationWarnings, drywall, studs, insulation, svgOutputPath!),
            _ => throw new ArgumentOutOfRangeException("--format", "Format must be 'text', 'csv', or 'svg'.")
        };
    }

    private static int PrintEstimateText(
        Project project,
        Area netArea,
        double grossAreaSqFt,
        double penetrationAreaSqFt,
        IReadOnlyList<string> validationWarnings,
        DrywallTakeoffResult drywall,
        StudTakeoffResult studs,
        InsulationTakeoffResult insulation)
    {
        Console.WriteLine("========================================");
        Console.WriteLine($"RapidTakeoff - Estimate: {project.Name}");
        Console.WriteLine("========================================");
        Console.WriteLine("Inputs");
        Console.WriteLine($"  Wall height (ft): {project.WallHeightFeet:0.###}");
        Console.WriteLine($"  Wall lengths (ft): {string.Join(", ", project.WallLengthsFeet.Select(x => x.ToString("0.###")))}");
        Console.WriteLine($"  Drywall sheet: {project.Settings.DrywallSheet}");
        Console.WriteLine($"  Drywall waste: {project.Settings.DrywallWaste:0.###}");
        Console.WriteLine($"  Stud spacing (in): {project.Settings.StudsSpacingInches:0.###}");
        Console.WriteLine($"  Stud waste: {project.Settings.StudsWaste:0.###}");
        Console.WriteLine($"  Insulation coverage (sqft): {project.Settings.InsulationCoverageSquareFeet:0.###}");
        Console.WriteLine($"  Insulation waste: {project.Settings.InsulationWaste:0.###}");
        Console.WriteLine();
        Console.WriteLine("Assumptions");
        Console.WriteLine("  Gross wall area = sum(wallLengthsFeet) * wallHeightFeet.");
        Console.WriteLine("  Net wall area = gross wall area - merged penetration area (overlaps are not double-counted).");
        Console.WriteLine($"  Stud spacing = {project.Settings.StudsSpacingInches:0.###} in on-center.");
        Console.WriteLine($"  Drywall sheet size = {drywall.Sheet.Width.TotalFeet:0.###}x{drywall.Sheet.Height.TotalFeet:0.###} ft.");
        Console.WriteLine($"  Drywall waste factor = {project.Settings.DrywallWaste:0.###}.");
        Console.WriteLine($"  Insulation coverage = {project.Settings.InsulationCoverageSquareFeet:0.###} sqft per unit.");
        Console.WriteLine($"  Insulation waste factor = {project.Settings.InsulationWaste:0.###}.");

        if (validationWarnings.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Warnings");
            foreach (var warning in validationWarnings)
                Console.WriteLine($"  - {warning}");
        }

        Console.WriteLine();
        Console.WriteLine("Results");
        Console.WriteLine($"  Wall gross area: {grossAreaSqFt:0.###} sqft");
        Console.WriteLine($"  Penetration area: {penetrationAreaSqFt:0.###} sqft");
        Console.WriteLine($"  Wall net area: {netArea.TotalSquareFeet:0.###} sqft");
        Console.WriteLine($"  Drywall sheets: {drywall.SheetCount} sheets ({drywall.Sheet.Width.TotalFeet:0.###}x{drywall.Sheet.Height.TotalFeet:0.###})");
        Console.WriteLine($"  Studs: {studs.TotalStuds} studs (base {studs.BaseStuds})");
        Console.WriteLine($"  Insulation: {insulation.Quantity} units");
        Console.WriteLine("========================================");
        return 0;
    }

    private static int PrintEstimateCsv(
        Project project,
        Area netArea,
        double grossAreaSqFt,
        double penetrationAreaSqFt,
        IReadOnlyList<string> validationWarnings,
        DrywallTakeoffResult drywall,
        StudTakeoffResult studs,
        InsulationTakeoffResult insulation)
    {
        Console.WriteLine("Category,Item,Quantity,Unit,Notes");
        Console.WriteLine($"Project,Name,1,project,\"{EscapeCsv(project.Name)}\"");
        Console.WriteLine($"Area,Wall Gross Area,{grossAreaSqFt:0.###},sqft,\"sum(lengths)*height\"");
        Console.WriteLine($"Area,Penetration Area,{penetrationAreaSqFt:0.###},sqft,\"merged area per wall; overlaps not double-counted\"");
        Console.WriteLine($"Area,Wall Net Area,{netArea.TotalSquareFeet:0.###},sqft,\"gross-openings\"");
        Console.WriteLine($"Assumptions,Stud Spacing,{project.Settings.StudsSpacingInches:0.###},in,\"on-center\"");
        Console.WriteLine($"Assumptions,Drywall Sheet,1,sheet,\"{drywall.Sheet.Width.TotalFeet:0.###}x{drywall.Sheet.Height.TotalFeet:0.###} ft\"");
        Console.WriteLine($"Assumptions,Drywall Waste,{project.Settings.DrywallWaste:0.###},fraction,\"\"");
        Console.WriteLine($"Assumptions,Insulation Coverage,{project.Settings.InsulationCoverageSquareFeet:0.###},sqft-per-unit,\"\"");
        Console.WriteLine($"Assumptions,Insulation Waste,{project.Settings.InsulationWaste:0.###},fraction,\"\"");
        Console.WriteLine($"Drywall,Sheet {drywall.Sheet.Width.TotalFeet:0.###}x{drywall.Sheet.Height.TotalFeet:0.###},{drywall.SheetCount},sheets,\"waste={drywall.WasteFactor:0.###}\"");
        Console.WriteLine($"Studs,Framing Stud,{studs.TotalStuds},studs,\"base={studs.BaseStuds}; spacing-in={studs.Spacing.TotalInches:0.###}; waste={studs.WasteFactor:0.###}\"");
        Console.WriteLine($"Insulation,Insulation Unit,{insulation.Quantity},units,\"coverage-sqft={insulation.Product.CoverageArea.TotalSquareFeet:0.###}; waste={insulation.WasteFactor:0.###}\"");
        foreach (var warning in validationWarnings)
            Console.WriteLine($"Warnings,Validation Warning,1,warning,\"{EscapeCsv(warning)}\"");
        return 0;
    }

    private static int PrintEstimateSvg(
        Project project,
        Area netArea,
        double grossAreaSqFt,
        double penetrationAreaSqFt,
        IReadOnlyList<string> validationWarnings,
        DrywallTakeoffResult drywall,
        StudTakeoffResult studs,
        InsulationTakeoffResult insulation,
        string outputPath)
    {
        var projectName = string.IsNullOrWhiteSpace(project.Name) ? "Untitled" : project.Name;
        var penetrationsByWall = project.Penetrations
            .GroupBy(p => p.WallIndex)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<PenetrationDto>)g.Select(ToRenderingPenetration).ToArray());

        var walls = project.WallLengthsFeet
            .Select((length, index) => new WallSegmentDto(
                $"Wall {index + 1}",
                length,
                penetrationsByWall.TryGetValue(index, out var items) ? items : []))
            .ToArray();

        var summary = new SummaryDto(
            project.WallLengthsFeet.Sum(),
            netArea.TotalSquareFeet,
            drywall.SheetCount,
            studs.TotalStuds,
            insulation.Quantity);
        var assumptions = BuildAssumptions(project, grossAreaSqFt, penetrationAreaSqFt);

        var dto = new WallStripDto(
            projectName,
            project.WallHeightFeet,
            walls,
            summary,
            assumptions);

        var renderer = new WallStripSvgRenderer();
        var svg = renderer.Render(dto);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        File.WriteAllText(outputPath, svg, Encoding.UTF8);
        PrintAssumptionsBlock(assumptions);
        if (validationWarnings.Count > 0)
        {
            Console.WriteLine("Warnings");
            foreach (var warning in validationWarnings)
                Console.WriteLine($"  - {warning}");
        }
        Console.WriteLine($"Wrote SVG: {outputPath}");
        return 0;
    }

    private static IReadOnlyList<string> BuildAssumptions(Project project, double grossAreaSqFt, double penetrationAreaSqFt)
    {
        return
        [
            $"Gross wall area formula: sum(lengths) * height = {grossAreaSqFt:0.###} sqft.",
            $"Penetration deduction uses merged opening area per wall = {penetrationAreaSqFt:0.###} sqft.",
            $"Net wall area formula: gross - penetrations = {grossAreaSqFt - penetrationAreaSqFt:0.###} sqft.",
            $"Stud spacing: {project.Settings.StudsSpacingInches:0.###} in on-center.",
            $"Drywall sheet token: {project.Settings.DrywallSheet}.",
            $"Drywall waste factor: {project.Settings.DrywallWaste:0.###}.",
            $"Insulation coverage: {project.Settings.InsulationCoverageSquareFeet:0.###} sqft per unit.",
            $"Insulation waste factor: {project.Settings.InsulationWaste:0.###}."
        ];
    }

    private static void PrintAssumptionsBlock(IReadOnlyList<string> assumptions)
    {
        Console.WriteLine("Assumptions");
        foreach (var assumption in assumptions)
            Console.WriteLine($"  {assumption}");
    }

    private static PenetrationDto ToRenderingPenetration(ProjectPenetration penetration)
    {
        var id = string.IsNullOrWhiteSpace(penetration.Id)
            ? $"OPEN-{penetration.WallIndex + 1:00}"
            : penetration.Id;
        var type = string.IsNullOrWhiteSpace(penetration.Type) ? "opening" : penetration.Type;
        return new PenetrationDto(
            id,
            type,
            penetration.XFeet,
            penetration.YFeet,
            penetration.WidthFeet,
            penetration.HeightFeet);
    }

    private static string EscapeCsv(string value)
    {
        return value.Replace("\"", "\"\"");
    }

    private static bool TryRunInteractivePrompt()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        if (!IsLikelyOwnConsoleWindow())
            return false;

        Console.WriteLine();
        Console.WriteLine("Enter a command and options (example: estimate --project .\\examples\\exampleproject.json).");
        Console.WriteLine("Press Enter on an empty line to exit.");
        Console.WriteLine();

        while (true)
        {
            Console.Write("rapid> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                return true;

            var tokens = SplitCommandLine(line);
            if (tokens.Length == 0)
                continue;

            _ = Run(tokens, allowInteractivePrompt: false);
            Console.WriteLine();
        }
    }

    private static bool IsLikelyOwnConsoleWindow()
    {
        // Double-click launches usually create a console hosting only this process.
        // Terminal-launched runs typically include additional attached console processes.
        var processIds = new uint[8];
        var count = GetConsoleProcessList(processIds, (uint)processIds.Length);
        return count == 1;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetConsoleProcessList(uint[] processList, uint processCount);

    private static string[] SplitCommandLine(string input)
    {
        var matches = CommandTokenRegex.Matches(input);
        var result = new List<string>(matches.Count);

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success)
            {
                result.Add(match.Groups[1].Value);
            }
            else if (match.Groups[2].Success)
            {
                result.Add(match.Groups[2].Value);
            }
        }

        return result.ToArray();
    }
}
