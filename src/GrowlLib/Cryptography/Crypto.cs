using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Piksel.GrowlLib.Cryptography
{
    public static class Crypto
    {
        public const int SaltLength = 8;

        private static RandomNumberGenerator rng = new RNGCryptoServiceProvider();

        public static HashAlgorithm GetHasher(HashType hashType)
        {
            switch (hashType) {
                case HashType.MD5: return new MD5CryptoServiceProvider();
                case HashType.SHA1: return new SHA1CryptoServiceProvider();
                case HashType.SHA256: return new SHA256CryptoServiceProvider();
                case HashType.SHA384: return new SHA384CryptoServiceProvider();
                case HashType.SHA512: return new SHA512CryptoServiceProvider();
            }

            return null;
        }

        public static ICryptoTransform GetEncryptor(EncryptionType encryptionType, byte[] key, byte[] iv = null)
            => GetCryptoTransform(encryptionType, true, key, iv);

        public static ICryptoTransform GetDecryptor(EncryptionType encryptionType, byte[] key, byte[] iv = null)
            => GetCryptoTransform(encryptionType, false, key, iv);

        public static ICryptoTransform GetCryptoTransform(EncryptionType encryptionType, bool encrypt, byte[] key, byte[] iv = null)
        {
            SymmetricAlgorithm sa = null;
            switch (encryptionType)
            {
                case EncryptionType.None: return new TransparentTransform();
                case EncryptionType.DES: sa = new DESCryptoServiceProvider(); break;
                case EncryptionType.TripleDES: sa = new TripleDESCryptoServiceProvider(); break;
                case EncryptionType.RC2: sa = new RC2CryptoServiceProvider(); break;
                case EncryptionType.AES: sa = new AesCryptoServiceProvider(); break;
            }

            if (iv != null)
            {
                sa.IV = iv;
            }
            else
            {
                sa.GenerateIV();
            }

            sa.Key = key;

            return encrypt ? sa.CreateEncryptor() : sa.CreateDecryptor();
        }

        internal static ArraySegment<byte> AddSalt(ref byte[] input, string saltHex = null)
        {
            Array.Resize(ref input, input.Length + SaltLength);
            var saltSegment = new ArraySegment<byte>(input, input.Length - SaltLength, SaltLength);
            if (saltHex == null)
            {
                rng.GetBytes(saltSegment.Array, saltSegment.Offset, saltSegment.Count);
            }
            else
            {

            }

            return saltSegment;
        }

        public static void ResizeKey(ref byte[] key, EncryptionType ea)
        {
            int keySize = -1;
            if (ea == EncryptionType.RC2 || ea == EncryptionType.DES) keySize = 8;
            else if (ea == EncryptionType.TripleDES || ea == EncryptionType.AES) keySize = 24;
            else return;

            Array.Resize(ref key, keySize);
        }

        public static string BytesToHex(IList<byte> bytes)
        {
            var count = bytes.Count;
            var sb = new StringBuilder(count * 2);
            foreach (var b in bytes)
            {
                sb.Append(Convert.ToString(b, 16));
            }
            return sb.ToString();
        }

        public static byte[] HexToBytes(string hex)
        {
            var count = hex.Length / 2;
            var bytes = new byte[count];

            for(int i=0; i<count; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public static void HexToBytes(string hex, ArraySegment<byte> target)
        {
            var count = hex.Length / 2;
            if (count > target.Count) throw new ArgumentOutOfRangeException();

            for (int i = 0; i < count; i++)
            {
                target.Array[target.Offset + i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
        }
    }
}
