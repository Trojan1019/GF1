public static class ProcessCommandUtility
{
    public static void ProcessCommand(string fileName, string argument, string workingDirectory)
    {
        System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo(fileName);
        processStartInfo.Arguments = argument;
        processStartInfo.CreateNoWindow = false;
        processStartInfo.ErrorDialog = true;
        processStartInfo.UseShellExecute = true;
        processStartInfo.WorkingDirectory = workingDirectory;

        if (processStartInfo.UseShellExecute)
        {
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.RedirectStandardError = false;
            processStartInfo.RedirectStandardInput = false;
        }
        else
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            processStartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
        }

        System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);

        if (process == null) return;
        
        process.WaitForExit();
        process.Close();
    }
}
