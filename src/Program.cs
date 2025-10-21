/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 * 
 * Program:                         ratSIGN
 * Description:                     Signs a file using a key file and the secret password and creates the signature file.
 *                                  IMPORTANT:
 *                                  The signature file is required to verify a published file.
 *                                  This signature file is also public.
 *                                  The key file contains the necessary signature information to sign files.
 *                                  This key file must be kept secret!
 * Current Version:                 1.0.9423.712 (19.10.2025)
 * Company:                         ratware
 * Author:                          Tom V. (ratware)
 * Email:                           info@ratware.de
 * Copyright:                       © 2025 ratware
 * License:                         Creative Commons Attribution 4.0 International (CC BY 4.0)
 * License URL:                     https://creativecommons.org/licenses/by/4.0/
 * Filename:                        Program.cs
 * Language:                        C# (.NET 8)
 * Required:                        ratCORE.Signing
 * 
 * 
 * You are free to use, share, and adapt this code for any purpose,
 * even commercially, provided that proper credit is given to the author.
 * See the license link above for details.
 *  
 * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 * 
 * History:
 * 
 *     - 19.10.2025 - Tom V. (ratware) - Version 1.0.9423.712
 *       Reviewed and approved
 * 
 * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 * 
 */

using ratCORE.Signing;
using System.Reflection;
using System.Text;

namespace ratSIGN
{
    internal class Program
    {
        internal static async Task Main(string[] args)
        {
            // no args, show help
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            try
            {
                // switch args
                switch (args[0].ToLowerInvariant())
                {
                    case "keygen":
                        {
                            string outDir = GetArg(args, "--out") ?? ".";
                            string? iterArg = GetArg(args, "--iterations");
                            string? nameArg = GetArg(args, "--name");

                            int iterations = 300_000;
                            if (!string.IsNullOrWhiteSpace(iterArg))
                                if (!int.TryParse(iterArg, out iterations) || iterations < 10_000)
                                    throw new ArgumentException("--iterations must be a positive integer greater or equal 10.000.");

                            string password = ReadPasswordConfirm("Password: ", "Repeat: ");
                            string path = await KeyGen.GenerateAsync(outDir, password, iterations, nameArg);
                            Console.WriteLine($"Key file created: {path}");
                            break;
                        }

                    case "sign":
                        {
                            string file = GetArg(args, "--file") ?? throw new ArgumentException("--file missing");
                            string key = GetArg(args, "--key") ?? throw new ArgumentException("--key missing");
                            string? outSig = GetArg(args, "--out");
                            string? comment = GetArg(args, "--comment");
                            string password = ReadPassword("Password: ");
                            string sigPath = await Signer.SignFileAsync(file, key, password, outSig, comment);
                            Console.WriteLine($"Signature written: {sigPath}");
                            break;
                        }

                    case "verify":
                        {
                            bool ok;
                            string file = GetArg(args, "--file") ?? throw new ArgumentException("--file missing");
                            string sig = GetArg(args, "--sig") ?? throw new ArgumentException("--sig missing");
                            string? pub = GetArg(args, "--pub");
                            string? keyid = GetArg(args, "--keyid");

                            if (pub is null && keyid is null)
                                throw new ArgumentException("Either --pub or --keyid must be specified.");

                            if (pub is not null && keyid is not null)
                                throw new ArgumentException("Both --pub and --keyid provided. --pub takes precedence.");

                            if (!string.IsNullOrWhiteSpace(pub))
                                ok = await Verifier.VerifyFileWithPublicKeyAsync(file, sig, pub);
                            else
                                ok = await Verifier.VerifyFileWithKeyIdAsync(file, sig, keyid!);

                            Console.WriteLine(ok ? "✅ Valid signature." : "❌ Invalid signature.");
                            Environment.ExitCode = ok ? 0 : 1;
                            break;
                        }

                    case "keyid":
                        {
                            string pub = GetArg(args, "--pub") ?? throw new ArgumentException("--pub missing");
                            string keyId;
                            using (var sha = System.Security.Cryptography.SHA256.Create())
                                keyId = Convert.ToBase64String(sha.ComputeHash(Convert.FromBase64String(pub)));
                            Console.WriteLine($"KeyId: {keyId}");
                            Environment.ExitCode = 0;
                            break;
                        }

                    case "version":
                        {
                            var exe = AppDomain.CurrentDomain.FriendlyName;
                            var ver = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "n/a";
                            var description = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "n/a";
                            var copyright = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "n/a";

                            Console.WriteLine($"""
                                {exe} - Version: {ver} - Copyright: {copyright}
                                {description}
                                """);
                            Environment.ExitCode = 0;
                            break;
                        }

                    default:
                        PrintHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.ExitCode = 2;
            }
        }

        private static void PrintHelp()
        {
            var exe = AppDomain.CurrentDomain.FriendlyName;
            var ver = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "n/a";

            Console.WriteLine($"""
                Usage: {exe} [Option] [Arguments]
                Version: {ver}
                Signs a file using a key file and the secret password and creates the signature file.
                IMPORTANT:
                - The signature file is required to verify a published file.
                  This signature file is also public.
                - The key file contains the necessary signature information to sign files.
                  This key file must be kept secret!

                USAGE
                    {exe} keygen   --out <dir> [--iterations <N>] [--name <base>]
                    {exe} sign     --file <path> --key <key.sec.json> [--out <sig.ratsig>] [--comment "<text>"]
                    {exe} verify   --file <path> --sig <sig.ratsig> (--pub <base64> | --keyid <base64>)
                    {exe} keyid    --pub <base64>

                COMMANDS
                    keygen     Generate an ECDSA-P256 key pair and write an encrypted key file (.sec.json).
                    sign       Create a detached signature (.ratsig) for a file using an encrypted key file.
                    verify     Verify a file and its .ratsig against a trusted public key (anchor).
                    keyid      Compute the KeyId = Base64(SHA256(pub)) for a given public key.

                OPTIONS
                    keygen:
                    --out <dir>            Output directory for the key file.
                    --iterations <N>       PBKDF2 iterations (default: 300000).
                    --name <base>          Optional base name for the key file (default: auto from KeyId).

                    sign:
                    --file <path>          File to sign.
                    --key <key.sec.json>   Encrypted key file (contains AES-GCM protected private scalar D).
                    --out <sig.ratsig>     Output signature path (default: <file>.ratsig).
                    --comment "<text>"     Optional trusted comment; becomes part of the signed message.

                    verify:
                    --file <path>          File to verify.
                    --sig <sig.ratsig>     Detached signature file.
                    --pub <base64>         Trusted public key (uncompressed EC point; 65 bytes Base64).
                    --keyid <base64>       Trusted KeyId = Base64(SHA256(pub)). Use either --pub or --keyid.

                    keyid:
                    --pub <base64>         Public key (uncompressed EC point; 65 bytes Base64).
             
                    version:
                                           Shows product informations.
             
                NOTES
                    • Algorithm: ECDSA P-256 with SHA-256. Signatures are DER-encoded.
                    • Public key format: uncompressed EC point: 0x04 || X(32) || Y(32), Base64-encoded.
                    • KeyId = Base64(SHA256(pub)). Prefer --keyid for a compact, stable trust anchor.
                    • The trusted comment is included in the signature (message = SHA256(file) || UTF8(comment)).
                    • Always verify with --pub or --keyid. Do not trust a public key embedded in .ratsig alone.

                EXAMPLES
                    # Generate a new key
                    {exe} keygen --out . --iterations 300000

                    # Sign a file
                    {exe} sign --file ./payload.bin --key ./ratsign_abcdef12.sec.json --comment "release:1"

                    # Verify with trusted public key (Base64 of uncompressed EC point)
                    {exe} verify --file ./payload.bin --sig ./payload.bin.ratsig --pub BM5X...LFfU=

                    # Verify with trusted KeyId
                    {exe} verify --file ./payload.bin --sig ./payload.bin.ratsig --keyid 8/zk...8LEs=

                    # Compute KeyId from a public key
                    {exe} keyid --pub BM5X...LFfU=

                EXIT CODES
                    0  Success (valid signature / command completed)
                    1  Verification failed (invalid signature or not trusted)
                    2  General error (invalid arguments, I/O, crypto error)
             """);
        }

        private static string? GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        private static string ReadPassword(string prompt)
        {
            Console.Write(prompt);
            var sb = new StringBuilder();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                    sb.Append(key.KeyChar);
            }
            Console.WriteLine();
            return sb.ToString();
        }

        private static string ReadPasswordConfirm(string prompt1, string prompt2)
        {
            string p1 = ReadPassword(prompt1);
            string p2 = ReadPassword(prompt2);
            if (!string.Equals(p1, p2, StringComparison.Ordinal))
                throw new Exception("Passwords do not match.");
            return p1;
        }
    }
}