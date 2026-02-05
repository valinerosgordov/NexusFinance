# Data Persistence Issue - Fixed

## Problem
Old transaction data (sample data) was persisting across application restarts, showing:
- Salary - NexusAI: ₱125,000
- AWS Cloud Services: ₱-12,500

## Root Cause
NexusFinance stores all data in a persistent JSON file at:
```
%APPDATA%\NexusFinance\data.json
```

When the app starts for the first time, it creates **sample/demo data** including:
- 3 sample projects (NexusAI, FinSync, Personal)
- 2 sample transactions
- 2 sample accounts
- 2 sample investments

This data is saved to disk and persists across app restarts.

## Solution Implemented

### 1. Added `ClearAllData()` Method
**Files Changed:**
- `Services/IDataService.cs` - Added interface method
- `Services/DataService.cs` - Implemented the method with `CreateEmptyData()` helper

**How it works:**
- Replaces all data with empty collections
- Keeps essential categories (Income, Salary, Infrastructure, etc.)
- Saves the empty state to disk

### 2. Added "Clear All Data" Button in Settings
**Files Changed:**
- `ViewModels/SettingsViewModel.cs` - Added `ClearAllDataCommand`
- `Views/SettingsView.xaml` - Added "Data Management" section with button

**Features:**
- ⚠️ Shows confirmation dialog before clearing
- Lists exactly what will be deleted
- Shows success message after completion
- Updates status message

### 3. Added Missing Commands
**Added to SettingsViewModel:**
- `DeleteApiKeyCommand` - Delete saved API key
- `OpenApiKeyLinkCommand` - Open Google AI Studio in browser

## How to Use

### Option 1: Clear Data from Settings (Recommended)
1. Run the application
2. Navigate to **Settings** (⚙️ Settings)
3. Scroll down to **"Data Management"** section
4. Click **🗑️ Clear All Data** button
5. Confirm the action
6. **Restart the application** to see fresh, empty state

### Option 2: Manually Delete Data File
1. Close the application
2. Open Windows Explorer
3. Navigate to: `%APPDATA%\NexusFinance\`
4. Delete `data.json` file
5. Restart the application (will create empty data.json)

### Option 3: Command Line
```powershell
# Delete the data file
Remove-Item "$env:APPDATA\NexusFinance\data.json" -Force

# Delete the entire data folder (including logs)
Remove-Item "$env:APPDATA\NexusFinance" -Recurse -Force
```

## Data Storage Location

### Main Data File
```
C:\Users\{YourUsername}\AppData\Roaming\NexusFinance\data.json
```

### API Key (Encrypted)
```
C:\Users\{YourUsername}\AppData\Local\NexusFinance\secure.dat
```

### Error Logs
```
C:\Users\{YourUsername}\AppData\Roaming\NexusFinance\Logs\errors_YYYY-MM-DD.log
```

## What Gets Cleared

When you click "Clear All Data", the following are **DELETED**:
- ✅ All transactions
- ✅ All projects
- ✅ All accounts
- ✅ All investments
- ✅ All payables
- ✅ All receivables
- ✅ All team members

What **REMAINS**:
- ✅ Default categories (Income, Salary, Infrastructure, Food, Rent, Transport)
- ✅ Your API key (stored separately in secure.dat)
- ✅ Language settings
- ✅ Application preferences

## Testing the Fix

1. **Before Fix:**
   - Open app → See sample transactions
   - Close app → Reopen → Still see sample transactions

2. **After Fix:**
   - Open app → See sample transactions
   - Go to Settings → Click "Clear All Data"
   - Restart app → See empty dashboard (no transactions)

## Technical Details

### Code Changes Summary
```csharp
// IDataService.cs
public interface IDataService
{
    // ... existing methods
    void ClearAllData();  // ✨ NEW
}

// DataService.cs
public void ClearAllData()
{
    lock (_dataLock)
    {
        _data = CreateEmptyData();  // Reset to empty
        SaveData();                  // Persist to disk
    }
}

private static AppData CreateEmptyData()
{
    return new AppData
    {
        Projects = new List<Project>(),      // Empty
        Transactions = new List<Transaction>(), // Empty
        Accounts = new List<Account>(),      // Empty
        Investments = new List<Investment>(),   // Empty
        Categories = GetDefaultCategories(), // Keep categories
        Payables = new List<Payable>(),      // Empty
        Receivables = new List<Receivable>(),   // Empty
        TeamMembers = new List<TeamMember>()    // Empty
    };
}
```

### ViewModel Command
```csharp
[RelayCommand]
private void ClearAllData()
{
    var result = MessageBox.Show(
        "Are you sure you want to clear ALL data?...",
        "Clear All Data",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning
    );
    
    if (result == MessageBoxResult.Yes)
    {
        _dataService.ClearAllData();
        StatusMessage = "✅ All data cleared. Restart the app.";
    }
}
```

## Future Improvements

1. **Add "Export Data" before clearing**
   - Allow users to backup data before deleting
   - Export to JSON/CSV format

2. **Add "Import Data" feature**
   - Restore from backup
   - Import from other financial apps

3. **Add "Reset to Demo Data" option**
   - Useful for testing/demos
   - Separate from clear action

4. **Add data auto-backup**
   - Daily/weekly automatic backups
   - Keep last N versions

## Troubleshooting

### Data still showing after clear?
- **Solution:** Make sure to restart the app after clicking "Clear All Data"
- ViewModels cache data in memory - restart forces reload

### Can't find Settings button?
- **Location:** Settings tab is in the main navigation menu
- **Icon:** ⚙️ (gear icon)

### Button doesn't work?
- **Check:** Make sure app was built successfully (`dotnet build`)
- **Check:** Look for any errors in the console

### Want to keep sample data?
- Don't click "Clear All Data"
- Sample data is useful for testing/demo purposes

## Related Files
- `Services/IDataService.cs` - Interface definition
- `Services/DataService.cs` - Data persistence logic
- `ViewModels/SettingsViewModel.cs` - Settings UI logic
- `Views/SettingsView.xaml` - Settings UI design

## Commit Message
```
fix: Add 'Clear All Data' feature to remove persistent sample data

- Add ClearAllData() method to IDataService and DataService
- Implement CreateEmptyData() to reset app state
- Add "Data Management" section in SettingsView
- Add ClearAllDataCommand with confirmation dialog
- Add DeleteApiKeyCommand and OpenApiKeyLinkCommand
- Update SettingsViewModel with proper dependency injection

Fixes issue where sample transactions persisted across app restarts.
Users can now clear all data from Settings → Data Management.
```

---

**Status:** ✅ **FIXED** - Ready to test
**Date:** 2026-02-05
