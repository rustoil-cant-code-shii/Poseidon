using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoseidonGUI;

public sealed class TerminalSession : IDisposable
{
    private readonly Process _process;
    private readonly CancellationTokenSource _cts = new();

    public event Action<string>? OutputReceived;

    public bool IsRunning =>
        !_process.HasExited;

    public TerminalSession()
    {
        _process = new Process();

        _process.StartInfo = new ProcessStartInfo
        {
            FileName = "/usr/bin/script",

            Arguments =
                "-qefc \"/bin/bash --noprofile --norc -i -c 'export PS1=\\\"[\\\\u@\\\\h \\\\w]$ \\\"; exec /bin/bash --noprofile --norc -i'\" /dev/null",

            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

            UseShellExecute = false,
            CreateNoWindow = true,

            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        _process.StartInfo.Environment["TERM"] =
            "xterm-256color";

        _process.StartInfo.Environment["SHELL"] =
            "/bin/bash";

        _process.Exited += Process_Exited;

        _process.Start();

        _ = ReadOutputAsync(
            _process.StandardOutput,
            _cts.Token);

        _ = ReadOutputAsync(
            _process.StandardError,
            _cts.Token);
    }

    private async Task ReadOutputAsync(
        System.IO.StreamReader reader,
        CancellationToken cancellationToken)
    {
        char[] buffer = new char[4096];

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int count = await reader.ReadAsync(
                    buffer,
                    0,
                    buffer.Length);

                if (count == 0)
                    break;

                string output =
                    new(buffer, 0, count);

                OutputReceived?.Invoke(output);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the terminal session is disposed.
        }
        catch (ObjectDisposedException)
        {
            // Stream was closed during shutdown.
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                OutputReceived?.Invoke(
                    $"\n[POSEIDON] Terminal output error:\n{ex.Message}\n");
            }
        }
    }

    private void Process_Exited(
        object? sender,
        EventArgs e)
    {
        OutputReceived?.Invoke(
            "\n[POSEIDON] Terminal session ended.\n");
    }

    public async Task WriteAsync(string text)
    {
        if (!IsRunning)
            return;

        try
        {
            await _process.StandardInput.WriteAsync(text);
            await _process.StandardInput.FlushAsync();
        }
        catch (ObjectDisposedException)
        {
            // Terminal was closed.
        }
        catch (InvalidOperationException)
        {
            // Standard input is no longer available.
        }
    }

    public async Task SendCommandAsync(string command)
    {
        await WriteAsync(command + "\n");
    }

    public void Dispose()
    {
        _cts.Cancel();

        try
        {
            if (!_process.HasExited)
            {
                _process.StandardInput.Close();
                _process.Kill();
            }
        }
        catch
        {
            // Process already exited or was already disposed.
        }

        _process.Dispose();
        _cts.Dispose();
    }
}
