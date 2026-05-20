using MudBlazor;

namespace FinTracker.Web.Themes;

public static class FinTrackerMudTheme
{
    public static MudTheme Default { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0d9488",
            Secondary = "#0891b2",
            Tertiary = "#059669",
            AppbarBackground = "#0d9488",
            DrawerBackground = "#ffffff",
            Background = "#f1f5f9",
            Surface = "#ffffff",
            TextPrimary = "#0f172a",
            TextSecondary = "#475569",
            ActionDefault = "#64748b",
            LinesDefault = "#e2e8f0",
            Divider = "#e2e8f0"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#2dd4bf",
            Secondary = "#22d3ee",
            Tertiary = "#4ade80",
            AppbarBackground = "#0f766e",
            DrawerBackground = "#111827",
            Background = "#0b1220",
            Surface = "#1e293b",
            TextPrimary = "#e2e8f0",
            TextSecondary = "#94a3b8",
            ActionDefault = "#94a3b8",
            LinesDefault = "#334155",
            Divider = "#334155"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "14px"
        }
    };
}
