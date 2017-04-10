using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;

namespace ResetMstsLicensing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var isTerminalServerGracePeriodRegistryValueDeletionIntended = true;

            var isMsLicensingRegistryKeyDeletionIntended = true;

            var isTerminalServerGracePeriodRegistryValueDeleted = false;

            var isMsLicensingRegistryKeyDeleted = false;

            var isMstcRunIntended = true;

            var isMstcRunIntentionNotSuppressedByNotDeletedfMsLicensingRegistryKey = false;

            var isAllOutputSuppressed = false;

            var argsNormalised = args != null && args.Length > 0
                ? args.Select(s => s.Trim().ToLowerInvariant()).ToArray()
                : new string[0];

            isAllOutputSuppressed = argsNormalised.Contains("--suppressalloutput");

            isTerminalServerGracePeriodRegistryValueDeletionIntended = !argsNormalised.Contains("--donttouchtsgp");

            isMsLicensingRegistryKeyDeletionIntended = !argsNormalised.Contains("--donttouchmslicensing");

            isMstcRunIntended = !argsNormalised.Contains("--dontstartmstc");

            isMstcRunIntentionNotSuppressedByNotDeletedfMsLicensingRegistryKey = argsNormalised.Contains("--dontsuppressmstconmslicensingnotdeleted");

            const string version = "1.0.0.0";

            const string copying = @"This is free and unencumbered software released into the public domain, visit unlicense.org to learn more.";

            const string source = @"github.com/strangeattr/ResetMstsLicensing";

            var help = new[]
            {
                "Application: ResetMstsLicensing.",

                $"Version: { version }.",

                "Version commentary: Absolutely untested so far, probably full of bugs but seemingly working on Windows 7 64-bit.",

                "Purpose: To reset Windows remore deskop (aka terminal) server and client licensing data saved when there is something wrong with it.",

                "Tip: Make sure it's legitimate in your case or don't use it.",

                "Requirements: Windows Vista or newer, .Net framework 4.5.2 or newer, local admlinstrator rights.",

                $"Source: { source }",

                "Parameter: --copying - Prints the licensing data.",

                "Parameter: --version - Prints the version.",

                "Parameter: --help - Prints this help message.",

                "Parameter: --suppressalloutput - Ensures silent operation suppressing any console output.",

                "Parameter: --donttouchtsgp - Forbids deleting the Terminal Server GracePeriod registry value.",

                "Parameter: --donttouchmslicensing - Forbids deleting the MSlicensing registry key.",

                "Parameter: --dontstartmstc - Forbids starting mstc.exe.",

                "Parameter: --dontsuppressmstconmslicensingnotdeleted - Forbids suppressing mstc.exe start by MSlicensing not being deleted."
            };

            if (!isAllOutputSuppressed)
            {
                if (argsNormalised.Contains("--copying"))
                {
                    Console.WriteLine(copying);

                    return;
                }

                if (argsNormalised.Contains("--version"))
                {
                    Console.WriteLine(version);

                    return;
                }

                if (argsNormalised.Contains("--help"))
                {
                    foreach (var line in help)
                        Console.WriteLine(line);

                    return;
                }

                Console.WriteLine($"The ResetMstsLicensing tool version { version }, run with --help to learn more.");

                Console.WriteLine();
            }

            try
            {
                if (isTerminalServerGracePeriodRegistryValueDeletionIntended)
                {
                    try
                    {
                        TokenManipulator.AddPrivilege("SeRestorePrivilege");

                        TokenManipulator.AddPrivilege("SeBackupPrivilege");

                        TokenManipulator.AddPrivilege("SeTakeOwnershipPrivilege");

                        using (var terminalServerGracePeriodRegistryKey
                            = Registry.LocalMachine.OpenSubKey(
                                @"System\CurrentControlSet\Control\Terminal Server\RCM\GracePeriod",
                                RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership))
                        {
                            if (terminalServerGracePeriodRegistryKey == null ||
                                terminalServerGracePeriodRegistryKey.ValueCount < 1)
                                isTerminalServerGracePeriodRegistryValueDeleted = true;
                            else if(terminalServerGracePeriodRegistryKey.ValueCount > 1)
                                throw new ApplicationException("More than one value found in the Terminal Server GracePeriod registry key.");
                            else
                            {
                                var administratorsNtAccount = new NTAccount("Administrators");

                                var networkServiceNtAccount = new NTAccount("NETWORK SERVICE");

                                var systemNtAccount = new NTAccount("SYSTEM");

                                var accessControl = terminalServerGracePeriodRegistryKey.GetAccessControl();

                                accessControl.SetOwner(administratorsNtAccount);

                                accessControl.AddAccessRule(new RegistryAccessRule(administratorsNtAccount,
                                    RegistryRights.FullControl, AccessControlType.Allow));

                                accessControl.AddAccessRule(new RegistryAccessRule(networkServiceNtAccount,
                                    RegistryRights.FullControl, AccessControlType.Allow));

                                accessControl.AddAccessRule(new RegistryAccessRule(systemNtAccount, RegistryRights.FullControl,
                                    AccessControlType.Allow));

                                terminalServerGracePeriodRegistryKey.SetAccessControl(accessControl);

                                terminalServerGracePeriodRegistryKey.Flush();

                                var terminalServerGracePeriodRegistryKeyValueNames =
                                    terminalServerGracePeriodRegistryKey.GetValueNames();

                                terminalServerGracePeriodRegistryKey.DeleteValue(
                                    terminalServerGracePeriodRegistryKeyValueNames[0]);

                                terminalServerGracePeriodRegistryKey.Close();

                                isTerminalServerGracePeriodRegistryValueDeleted = true;
                            }
                        }
                    }
                    catch (ArgumentException exception)
                    {
                        if (exception.Message.Contains("does not exist"))
                            isTerminalServerGracePeriodRegistryValueDeleted = true;
                        else
                            throw;
                    }
                    catch (Exception exception)
                    {
                        throw;
                    }
                    finally
                    {
                        TokenManipulator.RemovePrivilege("SeRestorePrivilege");

                        TokenManipulator.RemovePrivilege("SeBackupPrivilege");

                        TokenManipulator.RemovePrivilege("SeTakeOwnershipPrivilege");
                    }
                }

                if (isMsLicensingRegistryKeyDeletionIntended)
                {
                    try
                    {
                        TokenManipulator.AddPrivilege("SeRestorePrivilege");

                        TokenManipulator.AddPrivilege("SeBackupPrivilege");

                        TokenManipulator.AddPrivilege("SeTakeOwnershipPrivilege");

                        using (var msLicensingRegistryKey
                            = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\MSLicensing",
                                RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership))
                            if (msLicensingRegistryKey != null)
                            {
                                var administratorsNtAccount = new NTAccount("Administrators");

                                var networkServiceNtAccount = new NTAccount("NETWORK SERVICE");

                                var systemNtAccount = new NTAccount("SYSTEM");

                                var accessControl = msLicensingRegistryKey.GetAccessControl();

                                accessControl.SetOwner(administratorsNtAccount);

                                accessControl.AddAccessRule(new RegistryAccessRule(administratorsNtAccount,
                                    RegistryRights.FullControl, AccessControlType.Allow));

                                accessControl.AddAccessRule(new RegistryAccessRule(networkServiceNtAccount,
                                    RegistryRights.FullControl, AccessControlType.Allow));

                                accessControl.AddAccessRule(new RegistryAccessRule(systemNtAccount, RegistryRights.FullControl,
                                    AccessControlType.Allow));

                                msLicensingRegistryKey.SetAccessControl(accessControl);

                                msLicensingRegistryKey.Flush();

                                msLicensingRegistryKey.Close();
                            }

                        using (var microsoftRegistryKey
                            = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft",
                                RegistryKeyPermissionCheck.ReadWriteSubTree))
                            if (microsoftRegistryKey != null)
                            {
                                microsoftRegistryKey.DeleteSubKey("MSLicensing");

                                microsoftRegistryKey.Close();

                                isMsLicensingRegistryKeyDeleted = true;
                            }
                    }
                    catch (ArgumentException exception)
                    {
                        if (exception.Message.Contains("does not exist"))
                            isMsLicensingRegistryKeyDeleted = true;
                        else
                            throw;
                    }
                    catch (Exception exception)
                    {
                        throw;
                    }
                    finally
                    {
                        TokenManipulator.RemovePrivilege("SeRestorePrivilege");

                        TokenManipulator.RemovePrivilege("SeBackupPrivilege");

                        TokenManipulator.RemovePrivilege("SeTakeOwnershipPrivilege");
                    }
                }
            }
            catch (Exception exception)
            {
                if (!isAllOutputSuppressed)
                {
                    Console.WriteLine($"Error: an unhandled { exception.GetType() } has occured.");

                    Console.WriteLine();

                    Console.WriteLine($"Exception message: { exception.Message }");

                    Console.WriteLine();
                }
            }

            if (!isAllOutputSuppressed)
            {
                if (isTerminalServerGracePeriodRegistryValueDeleted)
                {
                    Console.WriteLine("Result: Terminal Server Grace Period registry value has been deleted or did not exist.");

                    Console.WriteLine();
                }


                if (isMsLicensingRegistryKeyDeleted)
                {
                    Console.WriteLine("Result: MSLicensing registry key has been deleted or did not exist.");

                    Console.WriteLine();
                }
                    
            }

            if (!isMsLicensingRegistryKeyDeleted && !isMstcRunIntentionNotSuppressedByNotDeletedfMsLicensingRegistryKey)
                isMstcRunIntended = false;

            if (isMstcRunIntended)
            {
                if (Environment.OSVersion.Version.Major < 6)
                {
                    if(!isAllOutputSuppressed)
                        Console.WriteLine("Error: the Windows version is older than Vista - can't start the mstc client the proper way.");

                    return;
                }

                try
                {
                    var appdataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);

                    var mstcExecutableFileFullName = Path.Combine(appdataDirectory, "mstsc.exe");

                    var mstcProcessStartInfo = new ProcessStartInfo(mstcExecutableFileFullName)
                    {
                        UseShellExecute = true,

                        Verb = "runas"
                    };

                    Process.Start(mstcProcessStartInfo);
                }
                catch (Exception exception)
                {
                    if (!isAllOutputSuppressed)
                    {
                        Console.WriteLine($"Error: failed to start mstc - an unhandled { exception.GetType() } has occured.");

                        Console.WriteLine();

                        Console.WriteLine($"Exception message: { exception.Message }");

                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
