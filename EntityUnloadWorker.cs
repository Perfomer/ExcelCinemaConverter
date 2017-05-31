
using System;
using System.Collections;
using System.Collections.Generic;

namespace ExcelConverter
{
    class EntityUnloadWorker : ExcelWorker
    {
        private const int START_ROW = 1, START_COLUMN = 5, FINISH_ROW = 8, FINISH_COLUMN = -1;

        private HashSet<CinemaUnload> cinemas;
        private List<CinemaModel> database;
        
        public List<CinemaModel> linkedModels;
        public List<EntityUnload> rows, linkedEntities;
        public SortedSet<string> cities;

        public string filmName, id;

        public EntityUnloadWorker(string path, List<CinemaModel> database)
            :base(path)
        {
            cities = new SortedSet<string>();
            cinemas = new HashSet<CinemaUnload>();
            rows = new List<EntityUnload>();
            linkedModels = new List<CinemaModel>();
            linkedEntities = new List<EntityUnload>();

            this.database = database;

            filmName = GetCellValue(1, 0).Split('"')[1];
            Init();
        }

        private void Init()
        {
            for(int i = START_COLUMN, rowsInterval = 0, items = 0; items < size && rowsInterval < MAX_ROWS_INTERVAL; i++)
            {
                string cell = GetCellValue(1, i);

                if (String.IsNullOrEmpty(cell))
                {
                    rowsInterval++;
                    continue;
                }
                else rowsInterval = 0;
                
                if (Char.IsDigit(cell[0]))
                {
                    items++;
                    string city = GetCellValue(3, i), cinema = GetCellValue(4, i), dimensionality = GetCellValue(2, i);
                    cities.Add(city);

                    EntityUnload entity = new EntityUnload(city, cinema, dimensionality, GetCellValue(6, i), GetCellValue(5, i));

                    CinemaModel model = SearchModelInDataBase(city, cinema); //Ищем в БД сочетание город + кинотеатр
                    if (model != null)
                    {
                        linkedEntities.Add(entity);
                        linkedModels.Add(model); //Если нашли, то закидываем модель не в общий список, а в особенный только для БДшных
                        continue;
                    }

                    EntityUnload entityBuffer = SearchEntity(city, cinema); //Нашли такой же кинотеатр? Знач не будет добавлять новый
                    if (entityBuffer != null) 
                    {
                        entityBuffer.dimensionalities.Add(dimensionality); //Но добавим новый формат. Всё, некст строчка.
                        continue;
                    }

                    bool needToAdd = true;
                    foreach (String cinemaUnload in SearchCinemas(city))
                    {
                        if (cinemaUnload.Equals(cinema))
                        {
                            needToAdd = false;
                            break;
                        }
                    }
                    if (needToAdd) cinemas.Add(new CinemaUnload(city, cinema));

                    if (id == null) id = GetCellValue(1, i);

                    rows.Add(entity);
                }


            }
        }

        private CinemaModel SearchModelInDataBase(string city, string cinema)
        {
            foreach (CinemaModel model in database)
            {
                if (city.Equals(model.City) && cinema.Equals(model.Cinema)) return model;
            }

            return null;
        }

        public List<string> SearchCinemas(string city)
        {
            List<string> result = new List<string>();
            foreach (CinemaUnload cinema in cinemas)
            {
                if (cinema.city.Equals(city))
                    result.Add(cinema.cinema);
            }
            return result;
        }

        private EntityUnload SearchEntity(string city, string cinema)
        {
            foreach(EntityUnload unload in rows)
            {
                if (city.Equals(unload.city) && cinema.Equals(unload.cinema)) return unload;
            }

            return null;
        }

    }

    class Screen
    {
        public int id;
        public string name;
        public IList dimensionalities;

        public Screen(int id, string name, IList dimensionality)
        {
            Init(id, name);
            dimensionalities = dimensionality;
        }

        public Screen(int id, string name)
        {
            Init(id, name);
            dimensionalities = new List<string>();
        }

        private void Init(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    class CinemaUnload
    {
        public string city, cinema;

        public CinemaUnload(string city, string cinema)
        {    
            this.city = city;
            this.cinema = cinema;
        }
    }

    class EntityUnload
    {
        public string city, cinema, dates, screensCount;
        public List<string> dimensionalities;

        public EntityUnload(string city, string cinema, string dimensionality, string dates, string screensCount)
        {
            dimensionalities = new List<string>();
            this.city = city;
            this.cinema = cinema;
            dimensionalities.Add(dimensionality);
            this.dates = dates;
            this.screensCount = screensCount;
        }
    }
}
