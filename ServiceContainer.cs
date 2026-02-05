using NexusFinance.Services;

namespace NexusFinance;

/// <summary>
/// Lightweight Dependency Injection container (Service Locator Pattern).
/// WHY: Enables testability, loose coupling, and follows SOLID principles.
/// For production apps, consider Microsoft.Extensions.DependencyInjection.
/// </summary>
public sealed class ServiceContainer
{
    private static ServiceContainer? _instance;
    private static readonly object Lock = new();

    private readonly IDataService _dataService;
    private readonly GeminiAnalysisService _aiService;
    private readonly SecureStorageService _secureStorageService;

    private ServiceContainer()
    {
        // Singleton services - shared across the application
        _secureStorageService = new SecureStorageService();
        _dataService = new DataService();
        _aiService = new GeminiAnalysisService(_secureStorageService);
    }

    public static ServiceContainer Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    _instance ??= new ServiceContainer();
                }
            }
            return _instance;
        }
    }

    // Service Accessors
    public IDataService DataService => _dataService;
    public IAiService AiService => _aiService;
    public SecureStorageService SecureStorageService => _secureStorageService;

    /// <summary>
    /// For testing: allows injecting mock services.
    /// </summary>
    public static void Reset()
    {
        lock (Lock)
        {
            _instance = null;
        }
    }
}
