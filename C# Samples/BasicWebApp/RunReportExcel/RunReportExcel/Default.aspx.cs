using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Win32;
using net.windward.api.csharp;
using System.IO;

namespace BasicWindwardEngine
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

			// if connector is not installed, tell user
			if (!IsExcelDotNetConnectorInstalled)
				throw new ApplicationException("Please install the Excel ADO.NET connector to run this example. Details at http://rpt.me/ExcelConnector");
		}

        protected void btnRunReport_Click(object sender, EventArgs e)
        {
            // DisplayReport.aspx will generate the report.
            Response.Redirect("DisplayReport.aspx");
        }

		#region test for connector

		/// <summary>
		/// Returns true if Excel connector is installed.
		/// </summary>
		public static bool IsExcelDotNetConnectorInstalled
		{
			get
			{
				try
				{
					String officeVersion = getOfficeVersion().ToString();
					if (Is64BitThread()) // 64 bit application in 64 bit os
					{
						if (File.Exists(Environment.GetEnvironmentVariable("ProgramFiles") + "\\Common Files\\Microsoft Shared\\OFFICE" + officeVersion + "\\ACECORE.dll"))
							return true;
					}
					else if (Is64BitOs()) // 32 bit application in 64 bit os
					{
						if (File.Exists(Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\\Common Files\\Microsoft Shared\\OFFICE" + officeVersion + "\\ACECORE.dll"))
							return true;
						String jetLocation = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\msjet40.dll";
						jetLocation = jetLocation.Replace("system32", "SysWOW64");
						if (File.Exists(jetLocation))
							return true;
					}
					else // 32 bit application in 32 bit os
					{
						if (File.Exists(Environment.GetEnvironmentVariable("ProgramFiles") + "\\Common Files\\Microsoft Shared\\OFFICE" + officeVersion + "\\ACECORE.dll"))
							return true;
						String jetLocation = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\msjet40.dll";
						if (File.Exists(jetLocation))
							return true;
					}
					return false;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		private static bool Is64BitThread()
		{
			return 8 == IntPtr.Size;
		}

		private static bool Is64BitOs()
		{
			return null != Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
		}

		public static int getOfficeVersion()
		{
			if (isWordInstalled())
			{
				using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(WORD_SUBKEY))
				{
					return getVersionFromAppKey(key);
				}
			}
			if (isExcelInstalled())
			{
				using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(EXCEL_SUBKEY))
				{
					return getVersionFromAppKey(key);
				}
			}
			if (isPowerPointInstalled())
			{
				using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(POWERPOINT_SUBKEY))
				{
					return getVersionFromAppKey(key);
				}
			}

			return -1;
		}

		private const String WORD_SUBKEY = "Word.Application";
		private const String EXCEL_SUBKEY = "Excel.Application";
		private const String POWERPOINT_SUBKEY = "PowerPoint.Application";

		public static bool isWordInstalled()
		{
			return isAppInstalled(WORD_SUBKEY);
		}

		public static bool isExcelInstalled()
		{
			return isAppInstalled(EXCEL_SUBKEY);
		}

		public static bool isPowerPointInstalled()
		{
			return isAppInstalled(POWERPOINT_SUBKEY);
		}
		private static bool isAppInstalled(string appKey)
		{
			using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(appKey))
			{
				if (key == null)
					return false;
			}
			return true;
		}

		private static int getVersionFromAppKey(RegistryKey key)
		{
			if (key == null)
				return -1;
			using (RegistryKey curVerSubKey = key.OpenSubKey("CurVer"))
			{
				if (curVerSubKey == null)
					return -1;
				String version = (String)curVerSubKey.GetValue("");
				int endIndex = version.LastIndexOf('.');
				version = version.Substring(endIndex + 1);
				int versionNum;
				if (int.TryParse(version, out versionNum))
					return versionNum;
				return -1;
			}
		}

		#endregion
        
    }
}