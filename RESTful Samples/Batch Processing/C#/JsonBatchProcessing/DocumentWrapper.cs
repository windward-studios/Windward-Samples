using WindwardRestApi.src.Model;

public class TemplateWrapper
{
	private Template template;

	public TemplateWrapper(string templateUrl, string templateFormatExtension, string outputFormatExtension)
	{
		Template.OutputFormatEnum outputFormat;
		Template.FormatEnum templateFormat;
		switch (outputFormatExtension.ToLower())
		{
			case "pdf":
				outputFormat = Template.OutputFormatEnum.Pdf;
				break;
			case "docx":
				outputFormat = Template.OutputFormatEnum.Docx;
				break;
			case "xlsx":
				outputFormat = Template.OutputFormatEnum.Xlsx;
				break;
			case "pptx":
				outputFormat = Template.OutputFormatEnum.Pptx;
				break;
			default:
				outputFormat = Template.OutputFormatEnum.Pdf;
				break;
		}
		switch (templateFormatExtension.ToLower())
		{
			case "docx":
				templateFormat = Template.FormatEnum.Docx;
				break;
			case "xlsx":
				templateFormat = Template.FormatEnum.Xlsx;
				break;
			case "pptx":
				templateFormat = Template.FormatEnum.Pptx;
				break;
			default:
				templateFormat = Template.FormatEnum.Docx;
				break;
		}

		// Create the template object based on the given information
		template = new Template(outputFormat, templateUrl, templateFormat);
	}

	// Add a datasource to the template
	public void addDataSource(DataSource dataSource)
	{
		template.Datasources.Add(dataSource);
	}

	// Set the given Input Parameter of the template to the 
	// given value
	public void setInputParameterInt(string name, int val)
	{
		Parameter param = new Parameter(name, val);
		template.Parameters.Add(param);
	}

	public Template GetTemplate()
	{
		return template;
	}


}
