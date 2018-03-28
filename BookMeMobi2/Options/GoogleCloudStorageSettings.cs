using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BookMeMobi2
{
    public class GoogleCloudStorageSettings
    {
        [JsonIgnore]
        public string BucketName { get; set; }
    }
}
