using System;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace LauncherPhantom.Managers
{
    public class EncryptionManager
    {
        private static EncryptionManager? _instance;
        private static readonly object _lock = new();

        public static EncryptionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new EncryptionManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Encrypts a string using DPAPI (Data Protection API)
        /// </summary>
        public string Encrypt(string plainText)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return plainText;

                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = ProtectedData.Protect(plainTextBytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EncryptionManager] Error en Encrypt: {ex.Message}");
                return plainText;
            }
        }

        /// <summary>
        /// Decrypts a string using DPAPI
        /// </summary>
        public string Decrypt(string cipherText)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                    return cipherText;

                var cipherTextBytes = Convert.FromBase64String(cipherText);
                var plainTextBytes = ProtectedData.Unprotect(cipherTextBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainTextBytes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EncryptionManager] Error en Decrypt: {ex.Message}");
                return cipherText;
            }
        }
    }
}