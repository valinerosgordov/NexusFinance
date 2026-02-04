using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NexusFinance.Services;

/// <summary>
/// Provides secure storage for sensitive data using Windows DPAPI (Data Protection API).
/// Data is encrypted per-user and can only be decrypted on the same machine by the same user.
/// </summary>
public class SecureStorageService
{
    private static readonly string StoragePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NexusFinance",
        "secure.dat"
    );

    /// <summary>
    /// Saves an API key securely using Windows DPAPI encryption.
    /// </summary>
    /// <param name="key">The API key to encrypt and save.</param>
    public void SaveApiKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("API key cannot be empty.", nameof(key));
        }

        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(StoragePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Convert to bytes
            var plainTextBytes = Encoding.UTF8.GetBytes(key);

            // Encrypt using DPAPI (CurrentUser scope)
            var encryptedBytes = ProtectedData.Protect(
                plainTextBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser
            );

            // Save to file
            File.WriteAllBytes(StoragePath, encryptedBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save API key securely.", ex);
        }
    }

    /// <summary>
    /// Loads and decrypts the API key from secure storage.
    /// </summary>
    /// <returns>The decrypted API key, or null if not found or decryption failed.</returns>
    public string? LoadApiKey()
    {
        try
        {
            if (!File.Exists(StoragePath))
            {
                return null;
            }

            // Read encrypted bytes
            var encryptedBytes = File.ReadAllBytes(StoragePath);

            // Decrypt using DPAPI
            var decryptedBytes = ProtectedData.Unprotect(
                encryptedBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser
            );

            // Convert back to string
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException)
        {
            // Key was encrypted by different user or on different machine
            return null;
        }
        catch (Exception)
        {
            // File corrupted or other error
            return null;
        }
    }

    /// <summary>
    /// Deletes the stored API key.
    /// </summary>
    public void DeleteApiKey()
    {
        try
        {
            if (File.Exists(StoragePath))
            {
                File.Delete(StoragePath);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to delete API key.", ex);
        }
    }

    /// <summary>
    /// Checks if an API key is stored.
    /// </summary>
    public bool HasApiKey()
    {
        return File.Exists(StoragePath) && LoadApiKey() != null;
    }
}
