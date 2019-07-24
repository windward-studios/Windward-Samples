

using System;
using System.IO;
using System.Collections.Generic;
using WindwardReportsDrivers.net.windward.datasource;
using WindwardReportsDrivers.net.windward.datasource.ado;
using WindwardReportsDrivers.net.windward.datasource.xml;
using WindwardInterfaces.net.windward.api.csharp;
using net.windward.api.csharp;

namespace UsingDatasets
{
	/// <summary>
	/// Sample code to set datasets (SQL & XML) using a .rdlx file.
	/// </summary>
	public class RunUsingDatasets
	{
		/// <summary>
		/// Sample code to set datasets (SQL & XML) using a .rdlx file.
		/// </summary>
		/// <param name="args">nothing</param>
		static void Main(string[] args){
		
			// Initialize the engine
            Report.Init();

            // Open template file and create output file
			using (FileStream template = new FileStream("../../files/Sample Dataset Template.docx", 
													FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (FileStream output = new FileStream("../../files/Sample Dataset Report.pdf",
													FileMode.Create, FileAccess.Write, FileShare.None))
				{

					// Create report process
					using (Report myReport = new ReportPdf(template, output))
					{
						// read in the template
						myReport.ProcessSetup();

						// XML datasource
						using (FileStream xmlFile = new FileStream("../../files/SouthWind.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
						using (FileStream xmlSchema = new FileStream("../../files/SouthWind.xsd", FileMode.Open, FileAccess.Read, FileShare.Read))
						using (SaxonDataSourceImpl dsSaxon = new SaxonDataSourceImpl(xmlFile, xmlSchema))
						using (DataSetImpl dsEmployeesUnder5 = new DataSetImpl("employeesUnder5", "/windward-studios/Employees/Employee[@EmployeeID < 5]", dsSaxon))
            			using (DataSetImpl dsCustStartA = new DataSetImpl("CustStartA", "/windward-studios/Customers/Customer[starts-with(CompanyName, 'A')]", dsSaxon))
						// SQL datasource
						using (AdoDataSourceImpl dsAdo = new AdoDataSourceImpl("System.Data.SqlClient", "Data Source=mssql.windward.net;Initial Catalog=Northwind;User ID=demo;Password=demo"))
			            using (DataSetImpl dsEmployeesMoreThan5 = new DataSetImpl("EmpMoreThan5", "SELECT * FROM dbo.Employees WHERE(dbo.Employees.EmployeeID > 5)", dsAdo))
            			using (DataSetImpl dsCustStartWithB = new DataSetImpl("CustStartWithB", "SELECT * FROM dbo.Customers WHERE(dbo.Customers.CompanyName like 'B%')", dsAdo))
						{
							IDictionary<string, IReportDataSource> datasources = new Dictionary<string, IReportDataSource>();
							datasources.Add("SW", dsSaxon);
            				datasources.Add("employeesUnder5", dsEmployeesUnder5);
            				datasources.Add("CustStartA", dsCustStartA);
            				datasources.Add("MSSQL", dsAdo);
				            datasources.Add("EmpMoreThan5", dsEmployeesMoreThan5);
				            datasources.Add("CustStartWithB", dsCustStartWithB);

							myReport.ProcessData(datasources);
						}

						// all data applied, finish up the report.
						myReport.ProcessComplete();

						// no need to call close because of the using constructs
					}
				}
			}

			// Opens the finished report
			string fullPath = Path.GetFullPath("../../files/Sample Dataset Report.pdf");
			Console.Out.WriteLine(string.Format("launching {0}", fullPath));
            System.Diagnostics.Process.Start(fullPath);
		}
	}
}
