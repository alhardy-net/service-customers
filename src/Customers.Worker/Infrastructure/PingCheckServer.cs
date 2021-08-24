using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Customers.Worker.Infrastructure
{
    public class PingCheckServer : IDisposable
    {
        private readonly HttpListener _httpListener = new();

        private CancellationTokenSource _cts = new();

        private Task _task;


        public PingCheckServer()
        {
            _httpListener.Prefixes.Add("http://*:80/ping/");
        }

        void IDisposable.Dispose()
        {
            Stop();
        }

        public PingCheckServer Start()
        {
            if (_task != null)
                throw new InvalidOperationException("The ping-check server has already been started.");

            _task = StartServer(_cts.Token);
            return this;
        }

        public async Task StopAsync()
        {
            // Signal the CTS to give a hint to the server thread that it is time to close up shop.
            _cts?.Cancel();

            try
            {
                if (_task == null)
                    return; // Never started.

                // This will re-throw any exception that was caught on the StartServerAsync thread.
                // Perhaps not ideal behavior but hey, if the implementation does not want this to happen
                // it should have caught it itself in the background processing thread.
                await _task;
            }
            catch (OperationCanceledException)
            {
                // We'll eat this one, though, since it can easily get thrown by whatever checks the CancellationToken.
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }

        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        private Task StartServer(CancellationToken cancellationToken)
        {
            _httpListener.Start();

            // Kick off the actual processing to a new thread and return a Task for the processing thread.
            return Task.Factory.StartNew(async delegate
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // There is no way to give a CancellationToken to GCA() so, we need to hack around it a bit.
                        var getContext = _httpListener.GetContextAsync();
                        getContext.Wait(cancellationToken);
                        var context = getContext.Result;
                        var response = context.Response;

                        try
                        {
                            try
                            {
                                response.ContentType = "text/plain";
                                response.StatusCode = 200;
                                var bytes = Encoding.ASCII.GetBytes("pong");
                                await response.OutputStream.WriteAsync(bytes.AsMemory(0, bytes.Length),
                                    cancellationToken);
                                await response.OutputStream.DisposeAsync();
                            }
                            catch (Exception ex)
                            {
                                // This can only happen before anything is written to the stream, so it
                                // should still be safe to update the status code and report an error.
                                response.StatusCode = 503;

                                if (!string.IsNullOrWhiteSpace(ex.Message))
                                    using (var writer = new StreamWriter(response.OutputStream))
                                    {
                                        writer.Write(ex.Message);
                                    }
                            }
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            Trace.WriteLine($"Error in PingCheckServer: {ex}");

                            try
                            {
                                response.StatusCode = 500;
                            }
                            catch
                            {
                                // Might be too late in request processing to set response code, so just ignore.
                            }
                        }
                        finally
                        {
                            response.Close();
                        }
                    }
                }
                finally
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}