﻿using FileProject;
using OfficeOpenXml;

namespace FileProject;

public class ExcelManager
{
    //File names
    public const string GRI_dataPath = "GRI_2017_2020.xlsx";
    public const string metaDataPath = "Metadata2006_2016.xlsx";

    //Rapport file
    private const string rapportFilePath = "DownloadRapport.xlsx";

    public ExcelManager()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //Find XL dokuments with Links
        try
        {
            //Check for files
            if (!File.Exists(GRI_dataPath) || !File.Exists(metaDataPath))
            {
                Console.WriteLine("XL files not found");
                Console.ReadKey();
                return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine("XL not found");
            Console.ReadKey();
        }

        if(!File.Exists(rapportFilePath))
        {
            //Excel.Application excelApp = new Excel.Application();
            //Excel.Workbook workbook = excelApp.Workbooks.Add(XlFileFormat.xlWorkbookNormal);
            //Excel.Worksheet worksheet = (Excel.Worksheet)workbook.ActiveSheet;

            //// Save as xlsm file
            //workbook.SaveAs("example.xlsm", Excel.XlFileFormat.xlOpenXMLWorkbookMacroEnabled);
        }
    }

    public int GetNumberOfGRI_Rows()
    {
        using (var package = new ExcelPackage(GRI_dataPath))
        {
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            return worksheet.Rows.Count();
        }
    }

    public List<string> ReadMetaRow(int rowIndex) => ReadRowFromExcel(metaDataPath, rowIndex);

    public List<string> ReadGRIRow(int rowIndex) => ReadRowFromExcel(GRI_dataPath, rowIndex);


    public List<string> ReadRowFromExcel(string filePath, int rowIndex)
    {
        using (var package = new ExcelPackage(filePath))
        {
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            //Get cells
            List<string> values = new List<string>();
            //Cell A
            values.Add(worksheet.Cells[rowIndex, 1].Value?.ToString());
            //Cell AL
            ExcelRange al = worksheet.Cells[rowIndex, 38];
            if (al.Value != null && al.Value.ToString() != "")
                values.Add(al.Value.ToString());
            else
            {
                //Cell AM
                ExcelRange am = worksheet.Cells[rowIndex, 39];
                if (am.Value != null && am.Value.ToString() != "")
                    values.Add(am.Value.ToString());
            }

            return values;
        }
    }

    public async Task<List<URL_Data>> ReadMultipulRowsWithLinks(string filePath, int startRow, int endRow)
    {
        if(startRow >= endRow)
        {
            Console.WriteLine("Reading mul link where start is bigger then end");
            return null;
        }

        using (var package = new ExcelPackage(filePath))
        {
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            List<URL_Data> values = new List<URL_Data>();
            
            for (int i = startRow; i < endRow; i++)
            {
                //Avoid header
                if (i == 1)
                    values.Add(new URL_Data("", "", false));
                else
                {
                    //Get cells         
                    //Cell A
                    string cellA = "";
                    cellA = worksheet.Cells[i, 1].Value?.ToString();

                    //Find Link
                    string link = "";
                    //Cell AL
                    ExcelRange al = worksheet.Cells[i, 38];
                    if (al.Value != null && al.Value.ToString() != "")
                        link = al.Value.ToString();
                    else
                    {
                        //Cell AM
                        ExcelRange am = worksheet.Cells[i, 39];
                        if (am.Value != null && am.Value.ToString() != "")
                            link = am.Value.ToString();
                    }

                    values.Add(new URL_Data(cellA, link, true));
                }
            }
            return values;
        }
    }


    public void WriteRapport(List<URL_Data> urlDataList)
    {
        // Get the current directory
        string currentDirectory = Directory.GetCurrentDirectory();

        // Create a file path relative to the current directory
        string filePath = Path.Combine(currentDirectory, "rapport.xlsx");

        // Create a new ExcelPackage object
        using (var package = new ExcelPackage())
        {
            // Create a new workbook
            var workbook = package.Workbook;

            // Create a new worksheet named "rapport"
            var worksheet = workbook.Worksheets.Add("rapport");

            // Set up the header row
            worksheet.Cells[1, 1].Value = "BR_Number";
            worksheet.Cells[1, 2].Value = "Download Status";
            worksheet.Cells[1, 3].Value = "URL";

            // Start writing data from row 2
            int rowIndex = 2;

            foreach (var urlData in urlDataList)
            {
                worksheet.Cells[rowIndex, 1].Value = urlData.BR_Nummer;
                worksheet.Cells[rowIndex, 2].Value = urlData.validLink ? "Downloaded" : "Failed to download";
                worksheet.Cells[rowIndex, 3].Value = urlData.URL;

                rowIndex++;
            }

            // Save the workbook
            package.SaveAs(filePath);

            Console.WriteLine($"Rapport saved successfully to {filePath}");
        }
    }
}
