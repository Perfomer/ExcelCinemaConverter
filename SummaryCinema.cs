using System.Collections.Generic;

namespace ExcelConverter
{
    class SummaryCinema
    {

        public string cinemaName, cinemaCodename, cityName, cityCodename, timeStart = "00:00", timeEnd = "23:59";

        public List<Screen> screens;
        public List<string> screenNames;

        public SummaryCinema() { Init(); }

        public SummaryCinema(CinemaModel model)
        {
            Init();
            cinemaName = model.Cinema;
            cinemaCodename = model.CinemaCodeName;
            cityName = model.City;
            cityCodename = model.CityCodeName;

            string[] screensStr = model.Screens.Split(' '), screensDimStr = model.Dimensionalities.Split('.');
            for (int i = 0; i < screensStr.Length; i++)
            {
                string name = screensStr[i];
                Screen screen = new Screen(i, name);
                foreach (string dimensionality in screensDimStr[i].Split(',')) screen.dimensionalities.Add(dimensionality);

                screens.Add(screen);
                screenNames.Add(name);
            }
            

        }

        private void Init()
        {
            screens = new List<Screen>();
            screenNames = new List<string>();
        }
    }
}

