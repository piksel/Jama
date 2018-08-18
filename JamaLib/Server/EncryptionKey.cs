using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Piksel.GrowlLib.Common;
using Piksel.GrowlLib.Cryptography;

namespace Piksel.GrowlLib.Server
{
    public class EncryptionKey
    {
        public static EncryptionKey Unencrypted = new EncryptionKey()
        {
            EncryptionType = EncryptionType.None
        };

        public string Password { get; private set; }

        public string Salt { get; private set; }

        protected byte[] Key { get; private set; }

        public string KeyHash { get; private set; }

        public HashType HashType { get; protected set; }

        public EncryptionType EncryptionType { get; protected set; }

        protected EncryptionKey(string password, HashType hashType, EncryptionType encryptiontype)
        {
            if (string.IsNullOrEmpty(password))
            {
                InitializeEmpty();
                return;
            }

            Password = password;
            HashType = hashType;
            EncryptionType = encryptiontype;

            byte[] input = Encoding.UTF8.GetBytes(password);
            var saltSegment = Crypto.AddSalt(ref input);

            Salt = Crypto.BytesToHex(saltSegment);

            var keyBytes = Crypto.GetHasher(hashType).ComputeHash(input);
            KeyHash = Crypto.BytesToHex(Crypto.GetHasher(hashType).ComputeHash(keyBytes));
            Crypto.ResizeKey(ref keyBytes, encryptiontype);

            Key = keyBytes;
        }


        private EncryptionKey() { }

        private void InitializeEmpty()
        {
            Password = string.Empty;
            Salt = KeyHash = null;
            Key = new byte[0];
        }

        internal string GetHeaderString()
        {
            var enc = EncryptionType.ToString().ToUpper();
            return (EncryptionType == EncryptionType.None)
                ? enc
                : $"{enc}:{KeyHash}.{Salt}";
        }

        public byte[] Encrypt(byte[] bytes, byte[] iv = null)
            => Crypto.GetEncryptor(EncryptionType, Key, iv).TransformFinalBlock(bytes, 0, bytes.Length);

        public byte[] Decrypt(byte[] bytes, byte[] iv)
            => Crypto.GetDecryptor(EncryptionType, Key, iv).TransformFinalBlock(bytes, 0, bytes.Length);

        public static bool Compare(string password, string keyHash, string salt, HashType hashType, EncryptionType encryptionType, out EncryptionKey matchingKey)
        {
            matchingKey = null;
            if (!string.IsNullOrEmpty(password))
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                Crypto.AddSalt(ref bytes, salt);

                var hash = Crypto.GetHasher(hashType).ComputeHash(bytes);
                var hexHash = Crypto.BytesToHex(Crypto.GetHasher(hashType).ComputeHash(hash));
                if (keyHash == hexHash)
                {
                    matchingKey = new EncryptionKey
                    {
                        Password = password,
                        Salt = salt,
                        KeyHash = keyHash,
                        HashType = hashType,
                        Key = hash,
                        EncryptionType = encryptionType
                    };
                    return true;
                }
            }
            return false;
        }
    }
}