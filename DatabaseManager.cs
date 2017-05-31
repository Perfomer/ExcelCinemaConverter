
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ExcelConverter
{
    class DatabaseManager
    {
        private ApplicationContext mDatabase;

        private const string CREATE_EXPRESSION = "CREATE TABLE \"CinemaModels\" ( `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `City` TEXT NOT NULL, `Cinema` TEXT NOT NULL, `CinemaCodeName` TEXT NOT NULL UNIQUE, `CityCodeName` TEXT NOT NULL, `Screens` TEXT NOT NULL, `Dimensionalities` TEXT NOT NULL )";

        public DatabaseManager()
        {
            mDatabase = new ApplicationContext();
            mDatabase.Cinemas.Load();
        }

        public void Save() { mDatabase.SaveChanges(); }
        public void AddItem(CinemaModel item) { mDatabase.Cinemas.Add(item); }
        public void RemoveItem(CinemaModel item) { mDatabase.Cinemas.Remove(item); }
        public List<CinemaModel> ToList() { return mDatabase.Cinemas.ToList(); }

        public void AddConvertationResults(List<SummaryCinema> summaryCinemas)
        {
            foreach (SummaryCinema cinema in summaryCinemas)
            {
                CinemaModel model = new CinemaModel();
                model.Cinema = cinema.cinemaName;
                model.CinemaCodeName = cinema.cinemaCodename;
                model.City = cinema.cityName;
                model.CityCodeName = cinema.cityCodename;

                string screens = "", dimensionalities = "";
                foreach (Screen screen in cinema.screens)
                {
                    screens += screen.name + " ";
                    foreach (string dimensionality in screen.dimensionalities)
                    {
                        dimensionalities += dimensionality + ",";
                    }
                    dimensionalities = dimensionalities.Remove(dimensionalities.Length - 1, 1);
                    dimensionalities += ".";
                }

                model.Screens = screens.Remove(screens.Length - 1, 1);
                model.Dimensionalities = dimensionalities.Remove(dimensionalities.Length - 1, 1);

                AddItem(model);
            }
            Save();
        }

    }
}
