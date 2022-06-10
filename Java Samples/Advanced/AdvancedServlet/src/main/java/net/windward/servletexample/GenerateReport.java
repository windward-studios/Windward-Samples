/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
*
* This file may be used in any way you wish. Windward Studios, Inc. assumes no
* liability for whatever you do with this file.
*/

package net.windward.servletexample;

import net.windward.datasource.DataSourceProvider;
import net.windward.datasource.salesforce.SalesForceDataSource;
import net.windward.datasource.dom4j.Dom4jDataSource;
import net.windward.datasource.jdbc.JdbcDataSource;
import net.windward.datasource.json.JsonDataSource;
import net.windward.datasource.odata.ODataDataSource;
import net.windward.env.DataSourceException;
import net.windward.format.htm.HtmlImage;
import net.windward.util.LicenseException;
import net.windward.xmlreport.ProcessDocx;
import net.windward.xmlreport.ProcessHtml;
import net.windward.xmlreport.ProcessPdf;
import net.windward.xmlreport.ProcessReport;
import net.windward.xmlreport.ProcessRtf;
import net.windward.xmlreport.ProcessXlsx;
import net.windward.xmlreport.SetupException;

import javax.servlet.ServletContext;
import javax.servlet.ServletException;
import javax.servlet.ServletOutputStream;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;
import java.io.ByteArrayOutputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.URL;
import java.util.HashMap;
import java.util.List;
import java.util.ListIterator;
import java.util.Map;

/**
 * This class generates a report and returns it to the browser.
 */

public class GenerateReport extends HttpServlet {

	/**
	 * How the data is stored
	 */
	public enum Datatype {
		FILE,
		URL, // may have credentials
		SALESFORCE
	}

	/**
	 * What type of database to use
	 */
	public enum DatasourceType {
		NONE(Datatype.FILE),
		XML(Datatype.FILE),
		DB2(Datatype.URL),
		Excel(Datatype.FILE),
		Access(Datatype.FILE),
		SqlServer(Datatype.URL),
		MySQL(Datatype.URL),
		Oracle(Datatype.URL),
		OData(Datatype.URL),
		Salesforce(Datatype.SALESFORCE),
		JSON(Datatype.FILE);

		private Datatype datatype;
		public Datatype getDatatype() { return datatype; }
		private DatasourceType(Datatype datatype){ this.datatype = datatype; }
	}

	/**
	 * Describes different output formats
	 */
	public enum Format {
		PDF, HTML, DOCX, XLSX, RTF
	}

	/**
	 * Where to find the properties file
	 */
	public static String propFile;

	/**
	 * Called by the servlet container to indicate to a servlet that the servlet is being
	 * placed into service. Set the location of WindwardReports.properties here.
	 */
	public void init() throws ServletException {
		super.init();
		ServletContext context = getServletContext();

		// Set properties file -- This is a context-param in web.xml.
		propFile = context.getInitParameter("PropFile");
		if (propFile == null)
			propFile = "/WEB-INF/WindwardReports.properties";
		propFile = context.getRealPath( propFile );
		System.setProperty( "WindwardReports.properties.filename", propFile );
		log( "Windward Reports property file at: " + propFile );
	}

	/**
	 * Called by the server (via the service method) to allow a servlet to handle a POST request.
	 * The HTTP POST method allows the client to send data of unlimited length to the Web server
	 * a single time and is useful when posting information such as credit card numbers.
	 *
	 * @param request An HttpServletRequest object that contains the request the client has made
	 * of the servlet.
	 *
	 * @param response An HttpServletResponse object that contains the response the servlet sends
	 * to the client.
	 *
	 * @exception IOException If an input or output error is detected when the servlet handles the request.
	 *
	 * @exception ServletException If the request for the POST could not be handled.
	 */
    public void doPost(HttpServletRequest request, HttpServletResponse response) throws IOException, ServletException {

		doGet(request, response);
	}

	/**
	 * Called by the server (via the service method) to allow a servlet to handle a GET request.
	 *
	 * @param request An HttpServletRequest object that contains the request the client has made
	 * of the servlet.
	 *
	 * @param response An HttpServletResponse object that contains the response the servlet sends
	 * to the client.
	 *
	 * @exception IOException If an input or output error is detected when the servlet handles the request.
	 *
	 * @exception ServletException If the request for the POST could not be handled.
	 */
    public void doGet(HttpServletRequest request, HttpServletResponse response) throws IOException, ServletException {

		ServletContext context = getServletContext();
		HttpSession session = request.getSession();
		DatasourceType datasourceType = null;
		Template templateData = null;
		String leaveRequestId = null; // for static/dynamic reports, need to set a variable
		Map<String, Object> map = null; // for variables if needed

		// make sure we have a license key
		try {
			ProcessReport.init();
		} catch (LicenseException le) {
			log("License error", le);
			throw new ServletException("License error", le);
		} catch (SetupException se) {
			log("Setup error", se);
			throw new ServletException("Setup error", se);
		}

		// ***** GET ALL PARAMETERS *****
		if (session.getAttribute("var") != null) {
			leaveRequestId = (String) session.getAttribute("var");
			// If we receive the var parameter, assume we are running the static or dynamic report pages
			// these can still get overridden if provided
			datasourceType = DatasourceType.XML;
			templateData = new FileDatasourceTemplate("/files/Example_Template.docx",
					datasourceType, Format.PDF, "/files/Example_Data.xml");
		}

		// ensure datasourcetype not null
		if (session.getAttribute("datasourcetype") != null) {
			if (session.getAttribute("datasourcetype") instanceof DatasourceType) {
				datasourceType = (DatasourceType) session.getAttribute("datasourcetype");
			} else {
				datasourceType = DatasourceType.valueOf((String) session.getAttribute("datasourcetype"));
			}
		}
		if (datasourceType == null) {
			throw new ParameterException("datasourcetype parameter is required");
		}

		// more variables based on datasourceType
		switch (datasourceType.getDatatype()) {
			case FILE:
				if (templateData == null || !(templateData instanceof FileDatasourceTemplate)) {
					templateData = new FileDatasourceTemplate();
				}
				FileDatasourceTemplate fileTemplateData = (FileDatasourceTemplate) templateData;
				if (session.getAttribute("datafile") != null) {
					fileTemplateData.datafile = (String) session.getAttribute("datafile");
				}
				break;
			case URL:
				if (templateData == null || !(templateData instanceof URLDatasourceTemplate)) {
					templateData = new URLDatasourceTemplate();
				}
				URLDatasourceTemplate urlTemplateData = (URLDatasourceTemplate) templateData;
				if (session.getAttribute("username") != null) {
					urlTemplateData.username = (String) session.getAttribute("username");
				}
				if (session.getAttribute("password") != null) {
					urlTemplateData.password = (String) session.getAttribute("password");
				}
				if (session.getAttribute("server") != null) {
					urlTemplateData.server = (String) session.getAttribute("server");
				}
				if (session.getAttribute("database") != null) {
					urlTemplateData.database = (String) session.getAttribute("database");
				}
				break;
			case SALESFORCE:
				if (templateData == null || !(templateData instanceof SalesforceTemplate)) {
					templateData = new SalesforceTemplate();
				}
				SalesforceTemplate salesforceTemplateData = (SalesforceTemplate) templateData;
				if (session.getAttribute("username") != null) {
					salesforceTemplateData.username = (String) session.getAttribute("username");
				}
				if (session.getAttribute("password") != null) {
					salesforceTemplateData.password = (String) session.getAttribute("password");
				}
				if (session.getAttribute("token") != null) {
					salesforceTemplateData.token = (String) session.getAttribute("token");
				}
				break;
			default:
				break;
		}
		if (session.getAttribute("template") != null) {
			templateData.template = (String) session.getAttribute("template");
		}
		if (session.getAttribute("format") != null) {
			if (session.getAttribute("format") instanceof Format) {
				templateData.format = (Format) session.getAttribute("format");
			} else {
				templateData.format = Format.valueOf((String) session.getAttribute("format"));
			}
			}
		templateData.datasourceType = datasourceType;

		// Handle the variable for static/dynamic reports
		if (leaveRequestId != null) {
			map = new HashMap<String, Object>();
			if (leaveRequestId.length() > 1)
				map.put("LeaveRequestId", (leaveRequestId).substring(0, 1));
            else
				map.put("LeaveRequestId", leaveRequestId);
		    map.put("CSRName", "John Brown");
		}

		// ***** PARAMETER ERROR CHECKING *****
		// check that templateData has everything it needs
		if (!templateData.isSet()) {
			throw new ParameterException("Not all required parameters have been passed in.");
		}
		
		// ***** RUN REPORT *****
		// output stream to the report (format-agnostic)
		ByteArrayOutputStream reportStream = new ByteArrayOutputStream();
		ProcessReport report = RunReport(context, templateData, map, reportStream);

		// ***** PREPARE OUTPUT *****
		// set all html images to delete when the session ends
		//   or when the VM ends (tomcat stopped)
		if (templateData.format == Format.HTML) {
			List list = ((ProcessHtml) report).getImageNames();
			for (ListIterator li = list.listIterator(); li.hasNext(); ) {
				HtmlImage img = (HtmlImage) li.next();
				if (img.getName().length() > 0) {
					FileList files = (FileList) session.getAttribute("FileList");
					if (files == null) {
						files = new FileList();
						session.setAttribute("FileList", files);
					}
					files.addFile(img.getName());

					// uncomment this if your server engine does not calls valueUnbind when it closes.
					// new File(img.getName()).deleteOnExit();
				}
			}
		}

		// output it
		switch (templateData.format) {
			case PDF:
			// note - this sometimes doesn't work with IE. It works fine with Netscape & Opera
			response.setContentType("application/pdf");
				break;
			case RTF:
				response.setContentType("application/rtf");
			case DOCX:
			response.setContentType("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
				break;
			case XLSX:
				response.setContentType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			case HTML:
			// It should not be text/css for the html with css
			response.setContentType("text/html");
				break;
			default:
			response.setContentType("text/plain");
		}

		// IE really really wants this for pdf files
		response.setBufferSize(reportStream.size());

		// write out
		ServletOutputStream out = response.getOutputStream();
		reportStream.writeTo(out);
		reportStream.close();
	}

	/**
	 * Runs a report based on the given parameters
	 * @param context the context of the servlet for getting paths and resources
	 * @param templateData a Template object which stores data about the template to run
	 * @param varMap (optional) any variables for the report
	 * @param reportStream an output stream to put the finished report it
	 * @return the ProcessReport object created to run the report
	 * @throws ServletException
	 * @throws IOException
	 */
	public static ProcessReport RunReport(ServletContext context,
										  Template templateData,
										  Map<String, Object> varMap,
										  OutputStream reportStream)
			throws ServletException, IOException {

		// define data file stream (so it can be closed properly if needed)
		InputStream datafileStream = null;

		// get an input stream to the template
		InputStream templateFile = context.getResourceAsStream(templateData.template);
		if (templateFile == null) {
			FileNotFoundException fnfe = new FileNotFoundException("Could not find file: " + context.getRealPath(templateData.template));
			context.log("Could not open template and/or data file", fnfe);
			throw fnfe;
		}
		context.log("Template: " + templateData.template);

		// try block to ensure GaeVFS cache is cleared
		try {
			// create the report
			ProcessReport report;
			try {
				// instantiate based on output format
				context.log("Output Format: " + templateData.format);
				switch (templateData.format) {
					case PDF:
						report = new ProcessPdf(templateFile, reportStream);
						break;
					case DOCX:
						report = new ProcessDocx(templateFile, reportStream);
						break;
					case HTML:
						report = new ProcessHtml(templateFile, reportStream);
						((ProcessHtml) report).setCss(ProcessHtml.CSS_INCLUDE, null, null);
						((ProcessHtml) report).setImagePath(context.getRealPath("/images"), "./images", "wr");
						break;
					case XLSX:
						report = new ProcessXlsx(templateFile, reportStream);
						break;
					case RTF:
						report = new ProcessRtf(templateFile, reportStream);
						break;
					default:
						throw new ParameterException("Format specified by format parameter is an invalid value or not supported");
				}
				report.processSetup();

				// cast templateData as needed (for switch datatype and switch datasourcetype)
				FileDatasourceTemplate fileTemplateData = null;
				URLDatasourceTemplate urlTemplateData = null;
				switch (templateData.datasourceType.getDatatype()) {
					case FILE:
						fileTemplateData = (FileDatasourceTemplate) templateData;
						String datafile = fileTemplateData.datafile;
						datafileStream = null;

						// If no datasource, no need to instantiate stream (expect datafile could be null)
						if (fileTemplateData.datasourceType == DatasourceType.NONE) {
							break;
						}

						// If it's a file, we will be able to get a real path and a resource from the servlet
						if (context.getRealPath(datafile) != null) {
							try {
								datafileStream = context.getResourceAsStream(fileTemplateData.datafile);
							} catch (Exception e) { // intentionally empty
							}
						}
						if (datafileStream == null) {
							// if not a file, it could be a URL to a file (for stream-based datasources)
							datafileStream = new URL(datafile).openStream();
						}
						if (datafileStream == null) {
							FileNotFoundException fnfe = new FileNotFoundException(
									"Could not open file: " + datafile);
							context.log("Could not open template and/or data file", fnfe);
							throw fnfe;
						}
						break;

					case URL:
						urlTemplateData = (URLDatasourceTemplate) templateData;
						break;
				}

				// instantiate data source provider and handle class names, url, etc.
				DataSourceProvider dsp;
				switch (templateData.datasourceType) {
					case NONE:
						context.log("No datasource.");
						dsp = null;
						break;
					case XML:
						context.log("Datasource: " + fileTemplateData.datafile);
						dsp = new Dom4jDataSource(datafileStream);
						break;
					case DB2:
						String driverclass = "com.ibm.db2.jcc.DB2Driver";
						String url = "jdbc:db2://" + urlTemplateData.server + "/" + urlTemplateData.database;
						context.log("Datasource: " + url);
						dsp = new JdbcDataSource(driverclass, url,
								urlTemplateData.username, urlTemplateData.password);
						break;
					case Access:
						driverclass = "sun.jdbc.odbc.JdbcOdbcDriver";
						String absoluteDataFile = context.getRealPath(fileTemplateData.datafile);
						url = "jdbc:odbc:Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ=" + absoluteDataFile;
						context.log("Datasource: " + fileTemplateData.datafile);
						dsp = new JdbcDataSource(driverclass, url);
						break;
					case Excel:
						driverclass = "sun.jdbc.odbc.JdbcOdbcDriver";
						absoluteDataFile = context.getRealPath(fileTemplateData.datafile);
						url = "jdbc:odbc:Driver={Microsoft Excel Driver (*.xls)};DBQ=" + absoluteDataFile;
						context.log("Datasource: " + fileTemplateData.datafile);
						dsp = new JdbcDataSource(driverclass, url);
						break;
					case SqlServer:
						driverclass = "com.microsoft.sqlserver.jdbc.SQLServerDriver";
						url = "jdbc:sqlserver://" + urlTemplateData.server +
								";DatabaseName=" + urlTemplateData.database;
						context.log("Datasource: " + url);
						dsp = new JdbcDataSource(driverclass, url,
								urlTemplateData.username, urlTemplateData.password);
						break;
					case MySQL:
						driverclass = "com.mysql.jdbc.Driver";
						url = "jdbc:mysql://" + urlTemplateData.server + "/" + urlTemplateData.database;
						context.log("Datasource: " + url);
						dsp = new JdbcDataSource(driverclass, url,
								urlTemplateData.username, urlTemplateData.password);
						break;
					case Oracle:
						driverclass = "oracle.jdbc.driver.OracleDriver";
						url = "jdbc:oracle:thin:@" + urlTemplateData.server;
						context.log("Datasource: " + url);
						dsp = new JdbcDataSource(driverclass, url,
								urlTemplateData.username, urlTemplateData.password);
					case OData:
						context.log("Datasource: " + urlTemplateData.server);
						dsp = new ODataDataSource(urlTemplateData.server);
						break;
					case Salesforce:
						SalesforceTemplate sfTemplateData = (SalesforceTemplate) templateData;
						context.log("Datasource: Salesforce");
						dsp = new SalesForceDataSource(sfTemplateData.username, sfTemplateData.password,
								sfTemplateData.token, true);
						break;
					case JSON:
						context.log("Datasource: " + fileTemplateData.datafile);
						dsp = new JsonDataSource(datafileStream, "UTF-8");
						break;
					default:
						dsp = null;
				}

				// var map if required
				if (varMap != null && dsp != null)
					report.setParameters(varMap);

				// Process data and close connection to datasource
				if (dsp != null) {
					report.processData(dsp, "");
					dsp.close();
				}
			} catch (DataSourceException dse) {
				context.log("process threw exception", dse);
				throw new ServletException("ProcessReport ctor threw exception", dse);
			} catch (SetupException se) {
				context.log("Setup error", se);
				throw new ServletException("Setup error", se);
			} catch (Exception ex) {
				context.log("processSetup threw exception", ex);
				throw new ServletException("processSetup threw exception", ex);
			}

			// process it
			context.log("Processing...");

			try {
				report.processComplete();
			} catch (Exception e) {
				context.log("process threw exception", e);
				throw new ServletException("ProcessReport.process() threw exception", e);
			}

			context.log("Finished");
			return report;
		} finally {
			templateFile.close();
			if (datafileStream != null)
				datafileStream.close();
		}
	}
}

/**
 * Class to store template data to pass into RunReport
 *
 * @see net.windward.servletexample.FileDatasourceTemplate
 * @see net.windward.servletexample.URLDatasourceTemplate
 */
class Template {
	public String template;
	public GenerateReport.DatasourceType datasourceType;
	public GenerateReport.Format format;

	protected Template() { }

	protected Template(String template, GenerateReport.DatasourceType datasourceType, GenerateReport.Format format) {
		this.template = template;
		this.datasourceType = datasourceType;
		this.format = format;
		}

	public boolean isSet() {
		return (template != null && datasourceType != null && format != null);
	}
}

/**
 * Class to store template data; specifically for file-based datasources (such as XML)
 *
 * @see net.windward.servletexample.Template
 */
class FileDatasourceTemplate extends Template {
	public String datafile = null;

	public FileDatasourceTemplate() { }

	public FileDatasourceTemplate(String template, GenerateReport.DatasourceType datasourceType,
								  GenerateReport.Format format, String datafile) {
		super(template, datasourceType, format);
		this.datafile = datafile;
	}

	public boolean isSet() {
		return (datafile != null
				&& super.isSet());
	}
}

/**
 * Class to store template data; specifically for connection-based datasources (such as SQL)
 *
 * @see net.windward.servletexample.Template
 */
class URLDatasourceTemplate extends Template {
	public String username;
	public String password;
	public String server;
	public String database;

	public URLDatasourceTemplate() { }

	public URLDatasourceTemplate(String template, GenerateReport.DatasourceType datasourceType,
								 GenerateReport.Format format, String username, String password,
								 String server, String database) {
		super(template, datasourceType, format);
		this.username = username;
		this.password = password;
		this.server = server;
		this.database = database;
	}

	public boolean isSet() {
		return (server != null && database != null && super.isSet());
	}
}

/**
 * Class to store Salesforce template data in; Salesforce is unique because it requires a username/password/token
 *
 * @see net.windward.servletexample.Template
 */
class SalesforceTemplate extends Template {
	public String username;
	public String password;
	public String token;

	public SalesforceTemplate() { }

	public SalesforceTemplate(String template, GenerateReport.DatasourceType datasourceType,
							  GenerateReport.Format format, String username, String password, String token) {
		super(template, datasourceType, format);
		this.username = username;
		this.password = password;
		this.token = token;
	}

	public boolean isSet() {
		return (username != null && password !=  null && token != null && super.isSet());
    }
}



