using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Windows;

namespace FlipbookMaker.Backend
{
    public sealed class Cache
    {
        [JsonProperty]
        public List<string> Files { get; set; } = new();

        [JsonProperty]
        public int FrameSize { get; set; } = 8;

        [JsonProperty]
        public int ColumnNumber { get; set; } = 1;

        public void ValidateData()
        {
            if (!Utility.IsPowerOfTwo(FrameSize))
            {
                FrameSize = 64;
            }

            var invalidFiles = Files.Where(s => !File.Exists(s));
            if(invalidFiles.Any())
            {
                StringBuilder sb = new();
                sb.AppendLine("The following flipbook frames no longer exist!");
                foreach (var file in invalidFiles)
                    sb.AppendLine(file);

                MessageBox.Show(sb.ToString());
                Files = Files.Except(invalidFiles).ToList();
            }
        }
    }
}
