using CommunityToolkit.Mvvm.ComponentModel;

namespace AclsTracker.Models;

public partial class HsAndTsItem : ObservableObject
{
    public string Id { get; init; } = string.Empty;       // Unique identifier (e.g., "h-hypovolemia")
    public string Name { get; init; } = string.Empty;     // Display name in Spanish
    public string Category { get; init; } = string.Empty; // "H" or "T"

    [ObservableProperty]
    private bool _isChecked;                               // Whether item has been marked/considered

    [ObservableProperty]
    private bool _isDismissed;                             // Whether item has been ruled out (REGI-03)

    [ObservableProperty]
    private DateTime? _checkedAt;                          // Timestamp when checked/dismissed
}
