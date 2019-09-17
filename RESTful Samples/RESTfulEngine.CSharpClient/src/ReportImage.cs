using System;
using System.Collections.Generic;
using System.IO;

namespace RESTfulEngine.CSharpClient
{
    /// <summary>
    /// Generate a report as a set of images.  Each image is a distinct page.
    /// </summary>
    public class ReportImage : Report
    {
        /// <summary>
        /// Create a new instance of the image report.
        /// </summary>
        /// <param name="uri">Base URI.</param>
        /// <param name="imageFormat">Desired image format: eps, svg, bmp, gif, jpg, png.</param>
        /// <param name="template">The input template.</param>
        /// <param name="pages">The generated report.  Each item in the list is a distinct page.</param>
        public ReportImage(Uri uri, string imageFormat, Stream template, List<byte[]> pages)
        : base(uri, template, pages)
        {
            OutputFormat = imageFormat.Trim().ToLower();
        }

        protected override string OutputFormat { get; }
    }
}
