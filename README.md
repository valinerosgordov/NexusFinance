# ⚡ NexusFinance

<!-- ![Logo Placeholder](docs/logo.png) -->

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Native-0078D4?logo=windows)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**A high-performance, native Windows desktop application for advanced personal and project financial planning.**

NexusFinance is a modern **"Quant Terminal"** for developers, freelancers, and solo entrepreneurs who need to manage both **Personal** and **Business/Project finances** in one place. Built with a stunning **Dark/Neon aesthetic** inspired by trading terminals and cyberpunk design.

---

## ✨ Features

- 📊 **Multi-View Dashboard** – Real-time KPIs for Net Worth, Income, Expenses, and Savings Rate
- 🚀 **Project Analytics** – Track revenue, costs, and profitability across multiple projects (e.g., NexusAI, FinSync)
- 👛 **Wallet & Investments** – Monitor bank accounts, crypto holdings, stocks, and real estate investments
- ➕ **Transaction Input** – Fast, keyboard-optimized data entry with Income/Expense categorization
- 💎 **Double-Entry Ledger** *(Planned)* – Accounting-grade transaction tracking
- 📈 **Live Charts** – Beautiful, animated charts powered by LiveCharts2
- 🌙 **Dark/Neon UI** – Eye-friendly theme with vibrant accent colors (Violet, Green, Red, Cyan, Gold)
- 🎨 **MVVM Architecture** – Clean separation of concerns using CommunityToolkit.Mvvm

---

## 🛠️ Tech Stack

| Technology | Purpose | Version |
|-----------|---------|---------|
| [.NET 8](https://dotnet.microsoft.com/) | Runtime Framework | 8.0 |
| [WPF (Windows Presentation Foundation)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/) | Native Windows UI | .NET 8 |
| [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) | MVVM Framework | 8.3.2 |
| [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) | UI Components | 5.1.0 |
| [LiveCharts2](https://github.com/beto-rodriguez/LiveCharts2) | Data Visualization (SkiaSharp) | 2.0.0-rc4.3 |
| [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) | ORM (Planned) | 8.0.11 |
| [SQLite](https://www.sqlite.org/) | Local Database (Planned) | 3.x |
| [ClosedXML](https://github.com/ClosedXML/ClosedXML) | Excel Export (Planned) | 0.104.1 |

---

## 🚀 Getting Started

### Prerequisites

- **Windows 10/11** (64-bit)
- **.NET 8 SDK** – [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** or **JetBrains Rider** (recommended for WPF development)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/NexusFinance.git
   cd NexusFinance
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run --project NexusFinance.csproj
   ```

Alternatively, open `financialplanner.sln` in Visual Studio and press **F5**.

---

## 📸 Screenshots

> **Note:** Screenshots will be added soon. The application features a stunning dark/neon UI with multiple views.

<!-- ![Dashboard](docs/screenshots/dashboard.png) -->
<!-- ![Projects](docs/screenshots/projects.png) -->
<!-- ![Wallet](docs/screenshots/wallet.png) -->

---

## 🏗️ Project Structure

```
NexusFinance/
├── App.xaml                    # Application entry point & global resources
├── App.xaml.cs                 # Application code-behind
├── MainWindow.xaml             # Main window layout (sidebar + content area)
├── MainWindow.xaml.cs          # Main window code-behind
├── ViewModels/                 # MVVM ViewModels
│   ├── MainViewModel.cs        # Navigation & active view management
│   ├── DashboardViewModel.cs   # Dashboard data & logic
│   ├── ProjectAnalyticsViewModel.cs
│   ├── WalletViewModel.cs
│   └── TransactionInputViewModel.cs
├── Views/                      # XAML User Controls
│   ├── DashboardView.xaml      # Main dashboard with KPIs & charts
│   ├── ProjectAnalyticsView.xaml
│   ├── WalletView.xaml
│   └── TransactionInputView.xaml
├── Converters/                 # WPF Value Converters
│   └── AmountToWidthConverter.cs
└── NexusFinance.csproj         # Project file
```

---

## 🎨 Design Philosophy

NexusFinance is built with a **"Developer-First"** mindset:

- **Fast Data Entry:** Keyboard shortcuts and auto-complete for rapid transaction logging.
- **Information Density:** Show as much data as possible without clutter (inspired by Bloomberg Terminal).
- **Visual Hierarchy:** Use color psychology – Green (income), Red (expense), Violet (primary), Cyan (business).
- **Zero Latency:** Native WPF + SkiaSharp rendering for 60fps animations.

---

## 🗺️ Roadmap

### ✅ Phase 1: Core UI (Completed)
- [x] Dashboard with KPI cards
- [x] Project Analytics view
- [x] Wallet & Investments view
- [x] Transaction Input form
- [x] Sidebar navigation
- [x] Dark/Neon theme

### 🚧 Phase 2: Data Layer (In Progress)
- [ ] SQLite database setup with EF Core
- [ ] Double-Entry Ledger implementation
- [ ] Transaction CRUD operations
- [ ] Project/Category management

### 📅 Phase 3: Advanced Features (Planned)
- [ ] Excel Import/Export (ClosedXML)
- [ ] Multi-currency support with live exchange rates
- [ ] Budget tracking & alerts
- [ ] Recurring transactions
- [ ] Monte Carlo risk simulation
- [ ] AI-powered insights (Semantic Kernel)

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

**Coding Standards:**
- Follow [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use MVVM pattern strictly (no logic in code-behind)
- Use `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`)
- Enable nullable reference types (`<Nullable>enable</Nullable>`)

---

## 📜 License

This project is licensed under the **MIT License** – see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) – Beautiful Material Design components for WPF
- [LiveCharts2](https://github.com/beto-rodriguez/LiveCharts2) – Amazing data visualization library
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) – Modern MVVM helpers
- Inspired by Bloomberg Terminal, TradingView, and Cyberpunk aesthetics

---

## 📧 Contact

**Project Maintainer:** [Your Name]  
**Email:** your.email@example.com  
**GitHub:** [@yourusername](https://github.com/yourusername)

---

<p align="center">
  Made with ⚡ and 💜 for the Developer Community
</p>
