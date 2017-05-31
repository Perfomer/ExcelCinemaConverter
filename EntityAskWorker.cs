using System;
using System.Collections.Generic;

namespace ExcelConverter
{
    class EntityAskWorker : ExcelWorker
    {

        public HashSet<string> cinemaCodenames, linkedCinemas;
        public List<CinemaAsk> cinemas;

        public EntityAskWorker(string path, List<CinemaModel> linkedCinemas)
            :base(path)
        {
            this.cinemaCodenames = new HashSet<string>();
            this.linkedCinemas = new HashSet<string>();
            this.cinemas = new List<CinemaAsk>();
            
            foreach(CinemaModel model in linkedCinemas)
            {
                this.linkedCinemas.Add(model.CinemaCodeName);
            }

            Init();
        }

        private void Init()
        {
            for (int i = 0; i < size; i++)
            {
                string codename = GetCellValue(3, i);

                if (linkedCinemas.Contains(codename)) continue;

                string city = GetCellValue(4, i), 
                    screen  = GetCellValue(5, i), dimensionality = GetCellValue(8, i);

                CinemaAsk cinema;


                if (cinemaCodenames.Contains(codename))
                {
                    cinema = GetCinema(codename);
                    if (String.IsNullOrEmpty(cinema.city)) cinema.city = city;
                    cinema.screens.Add(screen);
                    cinema.dimensionalities.Add(new List<string>());
                    cinema.dimensionalities[cinema.dimensionalities.Count - 1].Add(dimensionality);
                }
                else
                {
                    cinema = new CinemaAsk(codename, city, screen, dimensionality);
                    cinemas.Add(cinema);
                    cinemaCodenames.Add(codename);
                }


            }
        }

        public CinemaAsk GetCinema(string codename)
        {
            foreach (CinemaAsk cinema in cinemas)
                if (cinema.name.Equals(codename)) return cinema;
            
            return null;
        }

        public List<string> Search(string word)
        {
            List<string> results = new List<string>();
            
            foreach (string codeword in cinemaCodenames)
                if ((codeword.ToUpper()).Contains(word.ToUpper()))
                    results.Add(codeword);

            return results;
        }

    }

    class CinemaAsk
    {
        public string name, city, primalDimensionality;
        public List<string> screens;
        public List<List<string>> dimensionalities;

        public CinemaAsk(string name, string city, string screen, string dimensionality)
        {
            screens = new List<string>();

            primalDimensionality = dimensionality;
            dimensionalities = new List<List<string>>();
            dimensionalities.Add(new List<string>());
            dimensionalities[0].Add(dimensionality);

            this.name = name;
            this.city = city;

            this.screens.Add(screen);
        }
    }
}
