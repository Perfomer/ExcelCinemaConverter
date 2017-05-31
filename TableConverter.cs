using System;
using System.Collections.Generic;

namespace ExcelConverter
{
    class TableConverter
    {

        private static int COLUMN_COUNT = 17;

        private string tcIdentifier { get; }
        private string tcFilmName { get; }
        private string tcFormat { get; set; }
        private string tcTimeStart { get; set; }
        private string tcTimeEnd { get; set; }
        private string tcRussianCityName { get; set; }
        private string tcRussianCinemaName { get; set; }

        private List<Screen> screens;

        public string tcCity { get; set; }
        public string tcCodeName { get; set; }
        public string tcScreens { get; set; }
       // public string tcDimensionalities { get; set; }
        public string tcDateStart { get; set; }
        public string tcDateEnd { get; set; }
        
        public List<string[]> tcConvertationResult { get; set; }

        public TableConverter(string id, string filmName, string format, string datesSource)
        {
            tcIdentifier = id;
            tcFilmName = filmName;
            tcFormat = format;

            string[] dates = EditDate(datesSource);
            tcDateStart = dates[0];
            tcDateEnd = dates[1];
        }

        public void InitCinema(string russianCityName, string russianCinemaName,  string startTime, string endTime)
        {
            tcRussianCityName = russianCityName;
            tcRussianCinemaName = russianCinemaName;

            string cityName = GeneralMethods.TranslateWord(russianCityName);

            SetData(cityName.ToUpper(), GeneralMethods.TranslateWord(russianCinemaName) + "_" + cityName, startTime, endTime);
        }

        private void SetData(string cityName, string cinemaName, string startTime, string endTime)
        {
            tcCity = cityName;
            tcCodeName = cinemaName;

            tcTimeStart = startTime;
            tcTimeEnd = endTime;
        }

        private void SetData(string cityName, string cinemaName, string startTime, string endTime, List<Screen> screens)
        {
            SetData(cityName, cinemaName, startTime, endTime);
            this.screens = screens;
        }
        
        public List<string[]> ConvertCinema(SummaryCinema cinema)
        {
            SetData(cinema.cityCodename, cinema.cinemaCodename, cinema.timeStart, cinema.timeEnd, cinema.screens);

            List<string[]> result = DoConvertation();

            return tcConvertationResult;
        }

        private List<string[]> DoConvertation()
        {
            List<string[]> result = new List<string[]>();

            for (int i = 0; i < screens.Count; i++)
            {
                Screen screen = screens[i];
                foreach (string dimensionality in screen.dimensionalities)
                {
                    result.Add(ConvertScreen(screen.name, dimensionality.ToString()));
                }
            }

            tcConvertationResult = result;
            return tcConvertationResult;
        }

     /*   public List<string[]> GetFinalResult(string city, string cinema, string startTime, string endTime)
        {
            for (int i = 0; i < tcConvertationResult.Count; i++)
            {
                string[] row = tcConvertationResult[i];

                row[3] = cinema;
                row[4] = city;
                row[14] = startTime;
                row[16] = endTime;
            }

            return tcConvertationResult;
        } */

        private string[] ConvertScreen(string screen, string dimensionality)
        {
            string[] result = new string[COLUMN_COUNT];

            result[0] = "RU";
            result[1] = tcIdentifier;
            result[2] = tcFilmName;
            result[3] = tcCodeName;
            result[4] = tcCity;
            result[5] = screen;
            result[6] = "y";
            result[7] = "TECH OPS (EUR)";
            result[8] = dimensionality;
            result[9] = "";
            result[10] = tcFormat;
            result[11] = "";
            result[12] = "WBKDM@warnerbros.com";
            result[13] = tcDateStart;
            result[14] = tcTimeStart;
            result[15] = tcDateEnd;
            result[16] = tcTimeEnd;

            return result;
        }


        public static string[] EditDate(string date)
        {
            string[] result = date.Split('-');
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = result[i].Replace('.', '-');
            }

            return result;
        }

        
    }
}
