using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// Printer report.  The output is sent directly to a printer.
    /// </summary>
    public class ReportPrinter : Report
    {
        /// <summary>
        /// Create a new instance of the printer report.
        /// </summary>
        /// <param name="baseUri">Base URI of the running Windward service</param>
        /// <param name="template">Source template to process</param>
        /// <param name="printerName">Printer name to send the output to</param>
        public ReportPrinter(Uri baseUri, Stream template, string printerName)
            : base(baseUri, template)
        {
            MainPrinter = printerName;
        }

        protected override string OutputFormat
        {
            get
            {
                return "prn";
            }
        }
    }
}
