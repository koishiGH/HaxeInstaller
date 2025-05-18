using System;
using System.Net.Http;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Collections.Generic;

namespace HaxeInstaller
{
    class Program
    {
        private static readonly string HAXE_DOWNLOAD_URL = "https://github.com/HaxeFoundation/haxe/releases/download/4.3.7/haxe-4.3.7-win64.exe";
        private static readonly string TEMP_DIR = Path.Combine(Path.GetTempPath(), "HaxeInstaller");
        private static readonly string INSTALLER_PATH = Path.Combine(TEMP_DIR, "haxe-installer.exe");
        private static readonly string HAXELIB_PATH = @"C:\Haxe\Libs";

        private static readonly Dictionary<string, Dictionary<string, string>> HAXELIB_PRESETS = new Dictionary<string, Dictionary<string, string>>
        {
            ["Main installation"] = new Dictionary<string, string>
            {
                ["actuate"] = "1.9.0",
                ["bits"] = "1.3.0",
                ["box2d"] = "1.2.3",
                //["discord_rpc"] = "git", // TODO: ADD GIT SUPPORT LMAO
                ["flixel-addons"] = "3.3.2",
                //["flixel-depth"] = "git", // TODO: ADD GIT SUPPORT LMAO
                //["flixel-studio"] = "git", // TODO: ADD GIT SUPPORT LMAO
                ["flixel-text-input"] = "2.0.2",
                ["flixel-tools"] = "1.5.1",
                ["flixel-ui"] = "2.6.4",
                ["flixel"] = "6.0.0",
                ["format"] = "3.7.0",
                ["formatter"] = "1.18.0",
                ["hmm"] = "3.1.0",
                ["hscript"] = "2.6.0",
                ["hxcodec"] = "3.0.2",
                ["hxcpp-debug-server"] = "1.2.4",
                ["hxcpp"] = "4.3.2",
                ["hxdiscord_rpc"] = "1.3.0",
                ["layout"] = "1.2.1",
                ["lime-samples"] = "7.0.0",
                ["lime"] = "8.2.2",
                ["newgrounds"] = "1.3.0",
                ["openfl-samples"] = "8.7.0",
                ["openfl"] = "9.4.1",
                ["polymod"] = "1.3.1",
                ["thx.core"] = "0.44.0"
            },
            ["Funkin' Latest"] = new Dictionary<string, string>
            {
                // WILL DO AFTER GETTING GIT SUPPORT WORKING
            },
            ["Funkin' Legacy (0.2x)"] = new Dictionary<string, string>
            {
                ["actuate"] = "1.9.0",
                ["flixel-tools"] = "1.5.1",
                ["flixel-ui"] = "2.5.0",
                ["flixel-addons"] = "2.11.0",
                ["flixel"] = "4.11.0",
                ["hscript"] = "2.6.0",
                ["hxcpp-debug-server"] = "1.2.4",
                ["hxcpp"] = "4.3.2",
                ["lime-samples"] = "7.0.0",
                ["lime"] = "8.2.2",
                ["lime-samples"] = "7.0.0",
                ["lime"] = "8.2.2",
                ["newgrounds"] = "1.3.0",
                ["openfl-samples"] = "8.7.0",
                ["openfl"] = "9.4.1",
                ["polymod"] = "1.3.1",
                ["thx.core"] = "0.44.0"
            },
        };

        private static async Task<bool> IsHaxeInstalled()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "haxe",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return false;
                    }

                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine($"Haxe is already installed (version: {output.Trim()})");
                        return true;
                    }
                }
            }
            catch
            {
                // err haxe shouldn't be installed
            }
            return false;
        }

        static async Task Main(string[] args)
        {
            try
            {
                if (await IsHaxeInstalled())
                {
                    Console.WriteLine("Skipping Haxe installation...");
                }
                else
                {
                    Console.WriteLine("Starting Haxe installation...");
                    
                    if (!Directory.Exists(TEMP_DIR))
                    {
                        Directory.CreateDirectory(TEMP_DIR);
                    }

                    Console.WriteLine("Downloading Haxe installer...");
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(HAXE_DOWNLOAD_URL);
                        response.EnsureSuccessStatusCode();
                        using (var fileStream = File.Create(INSTALLER_PATH))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }

                    Console.WriteLine("Starting Haxe installer...");
                    Console.WriteLine("Please complete the installation in the installer window.");
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = INSTALLER_PATH,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null)
                        {
                            throw new Exception("Failed to start Haxe installer");
                        }

                        Console.WriteLine("Waiting for installation to complete...");
                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"Installation failed with exit code: {process.ExitCode}");
                        }
                    }

                    File.Delete(INSTALLER_PATH);

                    Console.WriteLine("Haxe installation completed successfully!");
                }

                Console.WriteLine("\nSetting up haxelib directory...");
                if (!Directory.Exists(HAXELIB_PATH))
                {
                    Directory.CreateDirectory(HAXELIB_PATH);
                }

                Console.WriteLine("Configuring haxelib...");
                var setupStartInfo = new ProcessStartInfo
                {
                    FileName = "haxelib",
                    Arguments = $"setup {HAXELIB_PATH}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var setupProcess = Process.Start(setupStartInfo))
                {
                    if (setupProcess == null)
                    {
                        throw new Exception("Failed to start haxelib setup process");
                    }

                    var setupOutput = await setupProcess.StandardOutput.ReadToEndAsync();
                    var setupError = await setupProcess.StandardError.ReadToEndAsync();
                    await setupProcess.WaitForExitAsync();

                    if (setupProcess.ExitCode != 0)
                    {
                        throw new Exception($"Haxelib setup failed: {setupError}");
                    }
                    Console.WriteLine("Haxelib setup completed successfully!");
                }

                Console.WriteLine("\nWould you like to install haxelibs? (y/n)");
                var haxelibResponse = Console.ReadLine()?.ToLower();
                
                if (haxelibResponse == "y" || haxelibResponse == "yes")
                {
                    Console.WriteLine("\nAvailable presets:");
                    Console.WriteLine("1. Main installation");
                    Console.WriteLine("2. Funkin' Legacy (0.2x)");
                    Console.Write("\nSelect a preset (1 or 2): ");
                    
                    var presetChoice = Console.ReadLine();
                    string selectedPreset = presetChoice switch
                    {
                        "1" => "Main installation",
                        "2" => "Funkin' Legacy (0.2x)",
                        _ => null
                    };

                    if (selectedPreset != null)
                    {
                        await InstallHaxelibs(selectedPreset);
                    }
                    else
                    {
                        Console.WriteLine("Invalid preset selection.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static async Task InstallHaxelibs(string presetName)
        {
            Console.WriteLine($"\nInstalling haxelibs for {presetName} preset...");
            
            if (!HAXELIB_PRESETS.TryGetValue(presetName, out var libs))
            {
                throw new Exception($"Preset '{presetName}' not found");
            }

            bool limeInstalled = false;

            foreach (var lib in libs)
            {
                Console.WriteLine($"Installing {lib.Key} version {lib.Value}...");
                var startInfo = new ProcessStartInfo
                {
                    FileName = "haxelib",
                    Arguments = $"install {lib.Key} {lib.Value}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        throw new Exception($"Failed to start haxelib process for {lib.Key}");
                    }

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine($"Warning: Failed to install {lib.Key}: {error}");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully installed {lib.Key}");
                        if (lib.Key == "lime")
                        {
                            limeInstalled = true;
                        }
                    }
                }
            }

            if (limeInstalled)
            {
                Console.WriteLine("\nWould you like to setup lime? (y/n)");
                var limeResponse = Console.ReadLine()?.ToLower();
                
                if (limeResponse == "y" || limeResponse == "yes")
                {
                    Console.WriteLine("Setting up lime...");
                    var limeSetupStartInfo = new ProcessStartInfo
                    {
                        FileName = "haxelib",
                        Arguments = "run lime setup -y",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var limeSetupProcess = Process.Start(limeSetupStartInfo))
                    {
                        if (limeSetupProcess == null)
                        {
                            throw new Exception("Failed to start lime setup process");
                        }

                        var limeOutput = await limeSetupProcess.StandardOutput.ReadToEndAsync();
                        var limeError = await limeSetupProcess.StandardError.ReadToEndAsync();
                        await limeSetupProcess.WaitForExitAsync();

                        if (limeSetupProcess.ExitCode != 0)
                        {
                            Console.WriteLine($"Warning: Lime setup failed: {limeError}");
                        }
                        else
                        {
                            Console.WriteLine("Lime setup completed successfully!");
                        }
                    }
                }
            }

            Console.WriteLine("\nListing installed libraries:");
            var listStartInfo = new ProcessStartInfo
            {
                FileName = "haxelib",
                Arguments = "list",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var listProcess = Process.Start(listStartInfo))
            {
                if (listProcess == null)
                {
                    throw new Exception("Failed to start haxelib list process");
                }

                var listOutput = await listProcess.StandardOutput.ReadToEndAsync();
                var listError = await listProcess.StandardError.ReadToEndAsync();
                await listProcess.WaitForExitAsync();

                if (listProcess.ExitCode != 0)
                {
                    Console.WriteLine($"Warning: Failed to list libraries: {listError}");
                }
                else
                {
                    Console.WriteLine(listOutput);
                }
            }

            Console.WriteLine("\nHaxelib installation completed!");
        }
    }
}