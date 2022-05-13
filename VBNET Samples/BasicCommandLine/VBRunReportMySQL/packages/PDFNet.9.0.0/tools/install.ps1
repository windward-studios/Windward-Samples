param($installPath, $toolsPath, $package, $project)

foreach ($reference in $project.Object.References)
{
    if($reference.Name -eq "PDFNet")
    {
        $reference.CopyLocal = $false;
    }
}