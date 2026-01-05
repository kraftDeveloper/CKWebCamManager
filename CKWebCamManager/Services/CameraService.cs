using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace CKWebCamManager.Services
{
    public class CameraService : IHostedService
    {
        private readonly string _configPath = "config.json";
        private List<Camera> _cameras = new List<Camera>();
        private readonly List<Process> _ffmpegProcesses = new List<Process>();
        private readonly IWebHostEnvironment _env;

        public CameraService(IWebHostEnvironment env)
        {
            _env = env;
            LoadCameras();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartAllStreams();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopAllStreams();
            return Task.CompletedTask;
        }

        private void LoadCameras()
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                _cameras = settings?.Cameras ?? new List<Camera>();
                // Assign IDs if missing
                for (int i = 0; i < _cameras.Count; i++)
                {
                    if (string.IsNullOrEmpty(_cameras[i].Id))
                        _cameras[i].Id = Guid.NewGuid().ToString();
                }
            }
        }

        private void SaveCameras()
        {
            var settings = new AppSettings { Cameras = _cameras };
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }

        public List<Camera> GetAll() => _cameras;

        public Camera GetById(string id) => _cameras.FirstOrDefault(c => c.Id == id);

        public void Add(Camera camera)
        {
            camera.Id = Guid.NewGuid().ToString();
            _cameras.Add(camera);
            SaveCameras();
            StartStream(camera);
        }

        public void Update(string id, Camera updated)
        {
            var camera = GetById(id);
            if (camera != null)
            {
                camera.Name = updated.Name;
                camera.Ip = updated.Ip;
                camera.Username = updated.Username;
                camera.Password = updated.Password;
                SaveCameras();
                RestartStream(camera);
            }
        }

        public void Delete(string id)
        {
            var camera = GetById(id);
            if (camera != null)
            {
                StopStream(camera);
                _cameras.Remove(camera);
                SaveCameras();
            }
        }

        private void StartAllStreams()
        {
            foreach (var camera in _cameras)
            {
                StartStream(camera);
            }
        }

        private void StopAllStreams()
        {
            foreach (var proc in _ffmpegProcesses.ToArray())
            {
                try
                {
                    proc.Kill();
                    proc.Dispose();
                }
                catch { }
            }
            _ffmpegProcesses.Clear();
        }

        private void StartStream(Camera camera)
        {
            StopStream(camera); // Ensure no existing

            string streamDir = Path.Combine(_env.WebRootPath, "streams", camera.Id);
            Directory.CreateDirectory(streamDir);
            string m3u8Path = Path.Combine(streamDir, "stream.m3u8");

            string url = camera.Ip.Trim();
            if (!url.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase))
            {
                url = $"rtsp://{Uri.EscapeDataString(camera.Username ?? "")}:{Uri.EscapeDataString(camera.Password ?? "")}@{url}";
            }

            // FFmpeg command to transcode RTSP to HLS
            // Assume ffmpeg is in PATH or provide full path
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{url}\" -c:v libx264 -preset veryfast -f hls -hls_time 2 -hls_list_size 3 -hls_flags delete_segments+append_list \"{m3u8Path}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var proc = new Process { StartInfo = startInfo };
            try
            {
                proc.Start();
                _ffmpegProcesses.Add(proc);
            }
            catch (Exception ex)
            {
                // Log error, perhaps set camera status to offline
                Console.WriteLine($"Failed to start stream for {camera.Name}: {ex.Message}");
            }
        }

        private void StopStream(Camera camera)
        {
            var proc = _ffmpegProcesses.FirstOrDefault(p => !p.HasExited); // Simple, but better associate by id
            if (proc != null)
            {
                try
                {
                    proc.Kill();
                    proc.Dispose();
                }
                catch { }
                _ffmpegProcesses.Remove(proc);
            }

            // Clean up stream files
            string streamDir = Path.Combine(_env.WebRootPath, "streams", camera.Id);
            if (Directory.Exists(streamDir))
            {
                try { Directory.Delete(streamDir, true); } catch { }
            }
        }

        private void RestartStream(Camera camera)
        {
            StopStream(camera);
            StartStream(camera);
        }
    }

    public class AppSettings
    {
        public List<Camera> Cameras { get; set; } = new List<Camera>();
    }
}