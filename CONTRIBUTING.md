# Contributing to NexusFinance

Thank you for your interest in contributing to NexusFinance! 🎉

## 🚀 Quick Start

1. **Fork** the repository
2. **Clone** your fork locally
3. Create a **feature branch**: `git checkout -b feature/YourFeature`
4. Make your changes following our [coding standards](#coding-standards)
5. **Test** your changes thoroughly
6. **Commit** with a clear message: `git commit -m "Add: feature description"`
7. **Push** to your fork: `git push origin feature/YourFeature`
8. Open a **Pull Request** with a detailed description

## 📝 Coding Standards

### C# Conventions

- Follow [Microsoft's C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use **file-scoped namespaces** (`namespace NexusFinance;`)
- Enable **nullable reference types** (already enabled in the project)
- Use **var** for local variables when type is obvious
- Use **expression-bodied members** for simple properties/methods

### MVVM Architecture

- **NO logic in code-behind** (`.xaml.cs` files should only contain `InitializeComponent()`)
- All business logic goes in **ViewModels**
- Use `CommunityToolkit.Mvvm` source generators:
  - `[ObservableProperty]` for properties
  - `[RelayCommand]` for commands
- ViewModels should inherit from `ObservableObject`

### Example ViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NexusFinance.ViewModels;

public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Hello";

    [RelayCommand]
    private void DoSomething()
    {
        // Logic here
    }
}
```

### XAML Conventions

- Use **meaningful names** for controls that are referenced in code-behind
- Keep XAML clean and readable (proper indentation)
- Use **data binding** (`{Binding}`) instead of code-behind manipulation
- Prefer **styles and templates** over inline properties

## 🎨 UI/UX Guidelines

- **Dark Theme First:** All new UI should use the dark/neon aesthetic
- **Color Palette:**
  - Primary: `#8A2BE2` (Violet)
  - Income/Positive: `#00E676` (Neon Green)
  - Expense/Negative: `#FF1744` (Neon Red)
  - Business/Projects: `#00BCD4` (Cyan)
  - Investments: `#FFD700` (Gold)
  - Background: `#050505` (Deep Black)
  - Cards: `#1E1E1E` (Dark Grey)
- **Icons:** Use emoji or MaterialDesign icons
- **Rounded Corners:** `CornerRadius="8"` or `"12"` for cards

## 🧪 Testing

- Write **unit tests** for ViewModels (using xUnit + FluentAssertions)
- Manually test all UI changes on Windows 10 and Windows 11
- Ensure the app runs without errors after your changes

## 📦 Commit Message Format

Use [Conventional Commits](https://www.conventionalcommits.org/):

- `feat: add dark mode toggle`
- `fix: correct calculation in savings rate`
- `docs: update installation instructions`
- `style: format code with dotnet-format`
- `refactor: extract common logic into helper`
- `test: add unit tests for DashboardViewModel`
- `chore: update NuGet packages`

## 🔍 Pull Request Checklist

Before submitting your PR, ensure:

- [ ] Code follows the project's coding standards
- [ ] All files use the correct namespace (`NexusFinance.*`)
- [ ] No `Console.WriteLine` or debug code left in
- [ ] No hardcoded file paths or secrets
- [ ] XAML builds without errors
- [ ] Application runs and your feature works as expected
- [ ] Commit messages are clear and descriptive
- [ ] PR description explains **what** and **why** (not just "fixed bug")

## 🤔 Questions?

If you have questions or need help:

1. Check the [README.md](README.md) first
2. Browse existing [Issues](../../issues)
3. Open a new [Discussion](../../discussions) if needed

Thank you for contributing! 🚀
