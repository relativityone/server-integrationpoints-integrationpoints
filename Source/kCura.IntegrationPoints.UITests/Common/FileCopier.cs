using System;
using System.Diagnostics;
using System.IO;
using kCura.IntegrationPoints.UITests.Logging;
using NUnit.Framework;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Common
{
	public static class FileCopier
	{
		private static readonly ILogger Log = LoggerFactory.CreateLogger(nameof(FileCopier));

		public static void UploadToImportDirectory(string sourcePath, string url, int workspaceId, string userName, string password, int timeoutInSeconds = 90)
		{
			string transferCliPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "ExternalDependencies", "TransferConsole");
			string transferCliExePath = Path.Combine(transferCliPath, @"Relativity.Transfer.Console.exe");

			string arguments = GetTransferConsoleArguments(sourcePath, url, workspaceId, userName, password);
			Log.Information("Starting Transfer Console at '{ExecPath}' with arguments: '{Args}'.", transferCliExePath, arguments);
			Log.Information("Timeout for execution is set to {Timeout} seconds.", timeoutInSeconds);

			Stopwatch sw = Stopwatch.StartNew();
			Process tapiProcess = Process.Start(GetProcessStartInfo(transferCliPath, transferCliExePath, arguments));
			try
			{
				Assert.NotNull(tapiProcess, "Could not start Transfer Console at '{0}' with arguments: '{1}'.", transferCliExePath,
					arguments);

				WaitForProcessToExit(tapiProcess, timeoutInSeconds);
				if (tapiProcess.ExitCode != 0)
				{
					throw new UiTestException(
						$"Transfer Console failed with exit code {tapiProcess.ExitCode}. Please check logs. LogConfig.xml is in '{transferCliPath}'.");
				}
				Log.Information("Transfer Console finished successfully after {Elapsed} seconds.", sw.Elapsed.Seconds);
			}
			finally
			{
				if (tapiProcess != null && !tapiProcess.HasExited)
				{
					tapiProcess.Kill();
				}
				tapiProcess?.Close();
			}
		}

		private static ProcessStartInfo GetProcessStartInfo(string workingDirectory, string fileName, string arguments)
		{
			return new ProcessStartInfo
			{
				WorkingDirectory = workingDirectory,
				FileName = fileName,
				Arguments = arguments,
				CreateNoWindow = true,
				UseShellExecute = false
			};
		}

		private static void WaitForProcessToExit(Process process, int timeoutInSeconds)
		{
			int totalMilliseconds = Convert.ToInt32(TimeSpan.FromSeconds(timeoutInSeconds).TotalMilliseconds);
			process.WaitForExit(totalMilliseconds);
			if (!process.HasExited)
			{
				throw new TimeoutException($"Process did not exit after {timeoutInSeconds} seconds.");
			}
		}

		// TODO add workspace resource pool handling - get rid of hardcoded "Files\" in targetpath
		private static string GetTransferConsoleArguments(string sourcePath, string url, int workspaceId, string userName, string password)
		{
			string[] processArgs =
			{
				@"/interactive-",
				@"/command:transfer",
				@"/direction:Upload",
				@"/configuration:""client=Web""",
				$@"/searchpath:""{sourcePath}""",
				$@"/targetpath:""DefaultFileRepository\EDDS{workspaceId}\DataTransfer\Import""",
				$@"/url:""{url}""",
				$@"/username:""{userName}""",
				$@"/password:""{password}""",
				$@"/workspaceid:{workspaceId}"
			};
			return string.Join(" ", processArgs);
		}
	}
}
