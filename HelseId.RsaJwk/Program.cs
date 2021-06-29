using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HelseId.RsaJwk
{
    static class Program
    {
        static int Main(string[] args)
        {
            var fileName = AppDomain.CurrentDomain.FriendlyName;

            if (args.Length < 1)
            {
                Console.WriteLine($"Usage: {fileName} rsa|ecdsa [name]");
                return 1;
            }

            var keyType = args[0];

            string prefix = null;
            if (args.Length >= 2)
            {
                prefix = args[1];
            }

            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                IgnoreReadOnlyProperties = true,
            };

            var jwkFileName = "jwk.json";
            var publicJwkFileName = "jwk_pub.json";

            if (prefix != null)
            {
                jwkFileName = $"{prefix}_{jwkFileName}";
                publicJwkFileName = $"{prefix}_{publicJwkFileName}";
            }

            JsonWebKey privateJwk, publicJwk;
            switch (keyType.ToLower())
            {
                case "rsa":
                    const int rsaKeySize = 4096;
                    (privateJwk, publicJwk) = GenerateRsaKey(rsaKeySize);
                    break;
                case "ecdsa":
                    (privateJwk, publicJwk) = GenerateEcdsaKey();
                    break;
                default:
                    Console.WriteLine($"Unsupported key type '{keyType}'");
                    return 1;
            }

            File.WriteAllText(jwkFileName, JsonSerializer.Serialize(privateJwk, serializerOptions));
            Console.WriteLine($"Wrote JWK to {jwkFileName}");
            File.WriteAllText(publicJwkFileName, JsonSerializer.Serialize(publicJwk, serializerOptions));
            Console.WriteLine($"Wrote public JWK to {publicJwkFileName}");

            return 0;
        }

        private static (JsonWebKey privateJwk, JsonWebKey publicJwk) GenerateRsaKey(int keySize)
        {
            var key = RSA.Create(keySize);
            var securityKey = new RsaSecurityKey(key);
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(securityKey);
            var privateJwk = new JsonWebKey
            {
                D = jwk.D,
                DP = jwk.DP,
                DQ = jwk.DQ,
                E = jwk.E,
                Kty = jwk.Kty,
                N = jwk.N,
                P = jwk.P,
                Q = jwk.Q,
                QI = jwk.QI,
            };
            var publicJwk = new JsonWebKey
            {
                Kty = jwk.Kty,
                N = jwk.N,
                E = jwk.E,
            };

            return (privateJwk, publicJwk);
        }

        private static (JsonWebKey privateJwk, JsonWebKey publicJwk) GenerateEcdsaKey()
        {
            var key = ECDsa.Create(ECCurve.NamedCurves.nistP521);
            var securityKey = new ECDsaSecurityKey(key);
            var jwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(securityKey);
            var privateJwk = new JsonWebKey
            {
                Kty = jwk.Kty,
                D = jwk.D,
                Crv = jwk.Crv,
                X = jwk.X,
                Y = jwk.Y
            };
            var publicJwk = new JsonWebKey
            {
                Kty = jwk.Kty,
                Crv = jwk.Crv,
                X = jwk.X,
                Y = jwk.Y
            };

            return (privateJwk, publicJwk);
        }
    }
}
