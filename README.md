# ResetMstsLicensing
A simple Tetminal Services licensing registry keys reset tool

## Legal disclaimer
This tool automates a well-known thing that can be easily done manually yet the thing is non necessarily legal to do for everybody who might want to. Make sure that's legitimate in your case or don't use this tool. You must check your Windows / Terminal Services license, complying to it is a legal responsibility of yours.

## Binary downloads
A pre-built [ResetMstsLicensing.exe](/ResetMstsLicensing/bin/Release/ResetMstsLicensing.exe?raw=true) file is available for download in /ResetMstsLicensing/bin/Release, you can download and start using it straight away, no actual need to build it yourself from the source code.

## Technology / platform / requirements
It's a .Net console application written in C# (using Visual Studio 2017 Community Edition), built for .Net framework 4.5.2, meant to be ran on Windows Vista or newer (only tested to run on Windows 7 x86-64 English version) with alocal administrator rights and having no extra library dependencies though making use of native WinAPI DLLs (which means it can hardly be expected to be runnable with alternative .Net Framework implementations and on non-Windows OSes).

## When to use
When you receive an error message saying “Remote session was disconnected because there are no Remote Desktop client access licenses available for this computer. Please contact the server administrator“ but are sure that's a bug and your intention is legitimate.

## What it actually does
* Deletes the HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSLicensing registry key.
* Deletes the sole value in  in the HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Terminal Server\RCM\GracePeriod registry key.
* Launches the Remote Desktop Connection client application (mstc.exe) with administrator rights to let it initialise new MSLicensing registry key.

## How to use
Just run it, use the --help command line parameter for more details.

## Data/functionality loss possibility warning
Don't use it unless you are sure the registry keys mentioned above don't contain any data you actually need.

## Licensing problems
The program makes use of a class ("TokenManipulator" in the "TokenManipulator.cs" file) that is just a slightly modified version of one I've stumpled upon in the Internet but failed to find any copyright/licensing information for. However, the code I've written myself is totally free (public domain).

## Disclaimer
The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
