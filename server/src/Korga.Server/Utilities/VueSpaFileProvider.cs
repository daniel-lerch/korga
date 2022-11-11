using Korga.Server.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Text;

namespace Korga.Server.Utilities;

public class VueSpaFileProvider : IFileProvider
{
    private readonly IOptionsMonitor<HostingOptions> optionsMonitor;
    private readonly ILogger<VueSpaFileProvider> logger;
    private readonly string path;
    private MemoryFileInfo cached;

    public VueSpaFileProvider(string webroot, IOptionsMonitor<HostingOptions> optionsMonitor, ILogger<VueSpaFileProvider> logger)
    {
        // This file provider asserts that the file exists at startup.
        // A cached response will be delivered if it is deleted during runtime.

        this.optionsMonitor = optionsMonitor;
        this.logger = logger;

        path = Path.Combine(webroot, "index.html");
        var file = new FileInfo(path);
        if (!file.Exists) throw new FileNotFoundException("The default document template was not found.", path);

        cached = CreateCache(file.LastWriteTimeUtc, optionsMonitor.CurrentValue.PathBase);
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        // Directory contents are only used by Razor, default files and directory browser as of .NET 5
        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        if (subpath != "/index.html")
        {
            return new NotFoundFileInfo(subpath);
        }

        try
        {
            // Optimistic concurrency model allows multiple threads to create cached objects if the underlying file changes.

            DateTime lastWrite = File.GetLastWriteTimeUtc(path);
            string? pathBase = optionsMonitor.CurrentValue.PathBase;

            if (cached.LastModified != lastWrite || cached.PathBase != pathBase)
            {
                cached = CreateCache(lastWrite, pathBase);
            }
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Error while updating the default document cache");
        }

        return cached;
    }

    private MemoryFileInfo CreateCache(DateTime lastWrite, string? pathBase)
    {
        byte[] buffer;

        if (string.IsNullOrEmpty(pathBase))
        {
            buffer = File.ReadAllBytes(path);
        }
        else
        {
            buffer = Encoding.UTF8.GetBytes(
                File.ReadAllText(path)
                    .Replace("\"/", $"\"{pathBase}/")
                    .Replace("'/'", $"'{pathBase}/'")
            );
        }

        return new MemoryFileInfo("index.html", buffer, lastWrite, pathBase);
    }

    public IChangeToken Watch(string filter)
    {
        // Change tracking is only used by Razor view engine as of .NET 5
        return NullChangeToken.Singleton;
    }

    private class MemoryFileInfo : IFileInfo
    {
        private readonly byte[] _content;

        public MemoryFileInfo(string name, byte[] content, DateTimeOffset lastModified, string? pathBase)
        {
            Name = name;
            _content = content;
            Length = content.Length;
            LastModified = lastModified;
            PathBase = pathBase;
        }

        public bool Exists => true;
        public long Length { get; }
        public string? PhysicalPath => null;
        public string Name { get; }
        public DateTimeOffset LastModified { get; }
        public bool IsDirectory => false;
        public string? PathBase { get; }

        public Stream CreateReadStream()
        {
            return new MemoryStream(_content, writable: false);
        }
    }
}
