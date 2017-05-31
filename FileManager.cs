using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ExcelConverter
{
    class FileManager
    {

        private static HSSFWorkbook fmWorkbook;
        private static HSSFSheet fmWorksheet;

        private static FileStream fmFileStream;

        public static bool SaveFile(List<List<string[]>> data, string pathToDirectory)
        {
            List<List<string[]>> formats = Split(data);

            foreach (List<string[]> format in formats)
            {
                PrepareManager();

                WriteData(format);

                string path = pathToDirectory + "\\Ask_result_" + format[0][8] + ".xls";
                try { fmFileStream = new FileStream(path, FileMode.Create); }
                catch (IOException)
                {
                    MessageBox.Show("Файл \"" + path + "\" используется (открыт). Нельзя сохранить новый вместо старого, пока открыт старый.");
                    return false;
                }


                SaveFile();
                MessageBox.Show("Файл \"" + path + "\" успешно сохранён.");
            }
            
            return true;
        }

        private static void PrepareManager()
        {
            fmWorkbook = new HSSFWorkbook();

            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();

            fmWorkbook.DocumentSummaryInformation = dsi;
            fmWorkbook.SummaryInformation = si;

            fmWorksheet = (HSSFSheet) fmWorkbook.CreateSheet("Worksheet");

            fmWorksheet.AlternativeExpression = false;
            fmWorksheet.AlternativeFormula = false;
        }

        private static void SaveFile()
        {
            fmWorkbook.Write(fmFileStream);
            fmFileStream.Close();
        }

        private static void WriteData(List<List<string[]>> data)
        {
            for (int i = 0, currentRow = 0; i < data.Count; i++)
            {
                List<string[]> cinema = data[i];
                for (int j = 0; j < cinema.Count; j++, currentRow++)
                {
                    string[] rowData = cinema[j];
                    HSSFRow row = (HSSFRow) fmWorksheet.CreateRow(currentRow);

                    for (int k = 0; k < rowData.Length; k++)
                    {
                        row.CreateCell(k).SetCellValue(rowData[k]);
                    }
                }
            }
        }

        private static void WriteData(List<string[]> data)
        {
            for (int j = 0; j < data.Count; j++)
            {
                string[] rowData = data[j];
                HSSFRow row = (HSSFRow)fmWorksheet.CreateRow(j);

                for (int k = 0; k < rowData.Length; k++)
                {
                    row.CreateCell(k).SetCellValue(rowData[k]);
                }
            }

        }

        private static List<List<string[]>> Split(List<List<string[]>> data)
        {
            List<List<string[]>> results = new List<List<string[]>>();
            List<string> dimensionalities = new List<string>();

            foreach (List<string[]> cinema in data)
            {
                foreach (string[] screen in cinema)
                {
                    string dimensionality = screen[8];

                    if (!dimensionalities.Contains(dimensionality))
                    {
                        dimensionalities.Add(dimensionality);
                        results.Add(new List<string[]>());
                    }

                    results[dimensionalities.IndexOf(dimensionality)].Add(screen);
                }
            }


            return results;
        }

    }
}
