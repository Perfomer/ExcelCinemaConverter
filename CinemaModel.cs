using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExcelConverter
{
    class CinemaModel
    {
        private string cmCinemaName;
        private string cmCinemaCodeName;
        private string cmCity;
        private string cmScreens;
        private string cmCityCodeName;
        private string cmDimensionalities;
        
        public int Id { get; set; }

        public string Cinema
        {
            get { return cmCinemaName; }
            set
            {
                cmCinemaName = value;
                OnPropertyChanged("Cinema");
            }
        }

        public string CinemaCodeName
        {
            get { return cmCinemaCodeName; }
            set
            {
                cmCinemaCodeName = value;
                OnPropertyChanged("CinemaCodeName");
            }
        }

        public string CityCodeName
        {
            get { return cmCityCodeName; }
            set
            {
                cmCityCodeName = value;
                OnPropertyChanged("CityCodeName");
            }
        }
        public string City
        {
            get { return cmCity; }
            set
            {
                cmCity = value;
                OnPropertyChanged("City");
            }
        }
        public string Screens
        {
            get { return cmScreens; }
            set
            {
                cmScreens = value;
                OnPropertyChanged("Screens");
            }
        }
        public string Dimensionalities
        {
            get { return cmDimensionalities; }
            set
            {
                cmDimensionalities = value;
                OnPropertyChanged("Dimensionalities");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
