using NPOI.HSSF.UserModel;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using System.Windows;

namespace ExcelConverter
{
    class ExcelWorker
    {
        public const string STATE_NORMAL = "StateNormal", STATE_ERROR = "StateError";
        public const int MAX_ROWS_INTERVAL = 10, LETTER_CHAR_START = 65;

        private IWorkbook mSourceWorkBook;
        protected ISheet mSourceWorkSheet;

        public string state;

        public int size;

        public ExcelWorker(string path)
        {
            state = STATE_NORMAL;
            try
            {
                FileStream file = new FileStream(path, FileMode.Open);
                try
                {
                    mSourceWorkBook = new HSSFWorkbook(file);
                    mSourceWorkSheet = (HSSFSheet) mSourceWorkBook.GetSheetAt(0);
                }
                catch (OfficeXmlFileException)
                {
                    FileStream fiale = new FileStream(path, FileMode.Open);
                    mSourceWorkBook = new XSSFWorkbook(fiale);
                    mSourceWorkSheet = (XSSFSheet) mSourceWorkBook.GetSheetAt(0);
                }
                file.Close();

                CalculateSize();
            }
            catch (IOException)
            {
                string[] parts = path.Split('\\');
                string filename = parts[parts.Length - 1];
                MessageBox.Show("Файл \"" + filename + "\" используется другим процессом.");
                state = STATE_ERROR;
                return;
            }
        }

        public string GetCellValue(int y, int x)
        {
            string value = "";
            try
            {
                var cell = mSourceWorkSheet.GetRow(x).GetCell(y);
                try
                {
                    if (cell != null) value = cell.StringCellValue;
                }
                catch (InvalidOperationException)
                {
                    double? valueInt = cell.NumericCellValue;
                    if (valueInt != null) value = valueInt.ToString();
                }
            }
            catch (NullReferenceException)
            {
                return "";
            }
            return value;
        }

        private void CalculateSize()
        {
            size = 0;

            for (int i = 0, rowsInterval = 0; rowsInterval < MAX_ROWS_INTERVAL; i++)
            {
                //if (i > 10) break;
                string cell = GetCellValue(1, i);
                
                if (String.IsNullOrEmpty(cell))
                {
                    rowsInterval++;
                    continue;
                }
                else rowsInterval = 0;


                if (Char.IsDigit(cell[0]))
                {
                    size++;
                }

            }
        }



    }
}
