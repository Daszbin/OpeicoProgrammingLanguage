using OpeicoCompiler;
using Spectre.Console;
using System.Security.Cryptography;

namespace Opeico;

internal class TerminalOpeico
{
    public static async Task Perform(string[] args)
    {
        string userInput = args[0];

        switch (userInput)
        {
            case "--encrypt":
                string sourceFilename = args[1];
                string destinationFilename = args[1] + ".encrypt";

                AnsiConsole.MarkupLine($"[bold yellow]Encrypting File: " + sourceFilename + " to " + destinationFilename + " [/]");

                await using (FileStream sourceStream = File.OpenRead(sourceFilename))
                await using (FileStream destinationStream = File.Create(destinationFilename))
                using (Aes provider = Aes.Create())
                using (ICryptoTransform cryptoTransform = provider.CreateEncryptor())
                await using (CryptoStream cryptoStream = new(destinationStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    destinationStream.Write(provider.IV, 0, provider.IV.Length);
                    await sourceStream.CopyToAsync(cryptoStream);
                    string mykey = System.Convert.ToBase64String(provider.Key);
                    await File.WriteAllTextAsync(sourceFilename + ".key", mykey);
                    Console.WriteLine(mykey);
                }
                break;
            case "--encrypted":
                string encFile = args[1] + ".encrypt";
                string encKeyfile = args[1] + ".key";

                byte[] keyEncrypted = Convert.FromBase64String(await File.ReadAllTextAsync(encKeyfile));

                string? plainText = null;

                await using (FileStream sourceStream = File.OpenRead(encFile))
                using (Aes provider = Aes.Create())
                {
                    byte[] IV = new byte[provider.IV.Length];
                    _ = sourceStream.Read(IV, 0, IV.Length);
                    using ICryptoTransform cryptoTransform = provider.CreateDecryptor(keyEncrypted, IV);
                    await using CryptoStream cryptoStream = new(sourceStream, cryptoTransform, CryptoStreamMode.Read);
                    using StreamReader reader = new(cryptoStream);
                    plainText = await reader.ReadToEndAsync();
                }
                Console.WriteLine(new Compiler().Go(plainText, isFile: false, debug: 0));
                break;
            case "--decrypt":
                string encryptedFile = args[1] + ".encrypt";
                string unencryptedFile = args[1];
                string keyfile = args[1] + ".key";

                byte[] key = Convert.FromBase64String(await File.ReadAllTextAsync(keyfile));

                AnsiConsole.MarkupLine($"[bold yellow]Decrypting File: " + encryptedFile + " with key: " + keyfile + "[/]");
                await using (FileStream sourceStream = File.OpenRead(encryptedFile))
                await using (FileStream destinationStream = File.Create(unencryptedFile))
                using (Aes provider = Aes.Create())
                {
                    byte[] IV = new byte[provider.IV.Length];
                    _ = sourceStream.Read(IV, 0, IV.Length);
                    using ICryptoTransform cryptoTransform = provider.CreateDecryptor(key, IV);
                    await using CryptoStream cryptoStream = new(sourceStream, cryptoTransform, CryptoStreamMode.Read);
                    await cryptoStream.CopyToAsync(destinationStream);
                }
                break;
            case "-d":
            case "--debug":
                Console.WriteLine(new Compiler().Go(args[1], isFile: true, debug: 1));
                break;
            case "-v":
            case "--version":
                {
                    AnsiConsole.MarkupLine($"[bold yellow]Current Version:[/] {CurrentVersion.Get()}");
                    break;
                }
            case "-i":
            case "--inline":
                {
                    Console.WriteLine(new Compiler().Go(args[1], isFile: false, debug: 0));
                    break;
                }
            default:
                Console.WriteLine(new Compiler().Go(args[0], isFile: true, debug: 0));
                break;
        }
    }
}