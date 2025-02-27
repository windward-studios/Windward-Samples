using System;
using System.Collections.Generic;
using System.Text;
using WindwardRestApi.src.Model;

namespace JsonBatchProcessing
{
    internal class DocumentResult
    {
        public string JobName { get; set; }
        public Document Document { get; set; }

        public string JobId { get; set; }
    }
}
