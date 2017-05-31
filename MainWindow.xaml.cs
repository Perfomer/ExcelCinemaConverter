using Microsoft.Win32;
using System;
using System.Windows;

using System.Data.Entity;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Infrastructure;
using System.Windows.Input;

namespace ExcelConverter
{
    public partial class MainWindow : Window
    {
        public const int START_ROW = 1, START_COLUMN = 5, FINISH_ROW = 8, FINISH_COLUMN = -1, MAX_ROWS_INTERVAL = 10;
        public const int LETTER_CHAR_START = 65;
        private const string RESULT_NAME = "_result.xls", FILE_CHOOSING_ERROR = "FileChoosingError";
        private const string CREATE_TABLE_EXPRESSION = "CREATE TABLE \"CinemaModels\" ( `Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `City` TEXT, `Cinema` TEXT, `CinemaCodeName` TEXT UNIQUE, `CityCodeName` TEXT, `Screens` TEXT )";

        private string mUnloadPath = "", mResultPath = "", mAskPath = "";
        private int mCurrentIndex = 0, mAssociated = 0;
        private bool mUnloadReady = false, mAskReady = false;

        private CinemaAsk mCurrentCinema;

        private EntityAskWorker mAsk;
        private EntityUnload mCurrentRow;
        private EntityUnloadWorker mUnload;

        private TableConverter mConverter;

        private List<SummaryCinema> mSummaryCinemas, mLinkedCinemas;
        
        private DatabaseManager mDatabaseManager;

        public MainWindow()
        {
            InitializeComponent();
            mDatabaseManager = new DatabaseManager();
            RefreshDataGrid();

            mSummaryCinemas = new List<SummaryCinema>();
            mLinkedCinemas = new List<SummaryCinema>();
        }







        // <<-- CONVERTER -->> \\

        private void SetFilmInfo()
        {
            L_FilmName.Content = mUnload.filmName;
            ActivateFilmInfo(true);
            ActivateCinemaInfo(false);
        }

        private void SetCinemaInfo(SummaryCinema summary)
        {
            SetCinemaInfo(summary.cityCodename, summary.cinemaCodename, summary.timeStart, summary.timeEnd, summary.screenNames);
        }

        private void SetCinemaInfo(string city, string cinema, string timeStart, string timeEnd, List<string> screens)
        {
            TB_City.Text = city;
            TB_CinemaName.Text = cinema;

            TB_TimeStart.Text = timeStart;
            TB_TimeEnd.Text = timeEnd;
            LB_ConverterScreens.ItemsSource = screens;
        }

        private void LoadCinemaInfo(int index)
        {
            bool linked = false;
            EntityUnload sourceRow;

            if (index < mUnload.rows.Count) sourceRow = mUnload.rows[index]; 
            else
            {
                sourceRow = mUnload.linkedEntities[index - mUnload.rows.Count];
                linked = true;
            }

            ActivateCinemaInfoForLinked();

            mCurrentRow = sourceRow;
            mCurrentIndex = index;

            L_City.Content = sourceRow.city;
            L_CinemaName.Content = sourceRow.cinema;

            TB_CurrentCinema.Text = (index + 1).ToString();
            TB_CountCinema.Text = GetRowsCount().ToString();
            LB_Dimensionalities.ItemsSource = null;

            if (mCurrentIndex < mSummaryCinemas.Count)
            {
                SetCinemaInfo(GetCurrentSummaryElement());
            }
            else if (linked)
            {
                if (mCurrentIndex < CalculateCurrentLength())
                {
                    SetCinemaInfo(GetCurrentSummaryElement());
                }
                else
                {
                    SummaryCinema cinema = new SummaryCinema(mUnload.linkedModels[index - mUnload.rows.Count]);
                    SetCinemaInfo(cinema);
                    mLinkedCinemas.Add(cinema);
                }
                
            }
            else
            {
                mConverter.InitCinema(sourceRow.city, sourceRow.cinema, TB_TimeStart.Text, TB_TimeEnd.Text);
                SetCinemaInfo(mConverter.tcCity, mConverter.tcCodeName, "00:00", "23:59", null);


                SummaryCinema summary = new SummaryCinema();
                summary.cityName = sourceRow.city;
                summary.cityCodename = TB_City.Text;

                summary.cinemaName = sourceRow.cinema;
                summary.cinemaCodename = TB_CinemaName.Text;

                summary.timeStart = TB_TimeStart.Text;
                summary.timeEnd = TB_TimeEnd.Text;

                mSummaryCinemas.Add(summary);

            }
            
            RefreshNavigationButtons();
            RefreshScreensButtons();
            RefreshScreensCount();
            RefreshCinemasCount();

        }
        
        private void SaveFilmData()
        {
            SummaryCinema cinema = GetCurrentSummaryElement();

            cinema.cityCodename = TB_City.Text;
            cinema.cinemaCodename = TB_CinemaName.Text;
            cinema.timeStart = TB_TimeStart.Text;
            cinema.timeEnd = TB_TimeEnd.Text;
        }

        private void B_Skip_Click(object sender, RoutedEventArgs e) { Skip(); }

        private void B_PrevCinema_Click(object sender, RoutedEventArgs e) { Prev(); }

        private void B_NextCinema_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Prev()
        {
            SaveFilmData();
            LoadCinemaInfo(--mCurrentIndex);
        }

        private void Next()
        {
            SaveFilmData();
            LoadCinemaInfo(++mCurrentIndex);
        }

        private void Skip()
        {
            Next();

            if (mCurrentIndex >= mUnload.rows.Count && mCurrentIndex < GetRowsCount() - 1) Skip();
        }

        private void B_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            bool screensCount = LB_ConverterScreens.Items.Count == Int32.Parse(mCurrentRow.screensCount),
                city = !String.IsNullOrEmpty(TB_City.Text),
                cinemaName = !String.IsNullOrEmpty(TB_CinemaName.Text),
                timeStart = !String.IsNullOrEmpty(TB_TimeStart.Text),
                timeEnd = !String.IsNullOrEmpty(TB_TimeEnd.Text);

            if (screensCount && city && cinemaName && timeStart && timeEnd)
                SaveFilmData();
            else
            {
                GetActualSummaryList().Remove(GetCurrentSummaryElement());
            }

            List<List<string[]>> newCinemas = new List<List<string[]>>(), linkedCinemas = new List<List<string[]>>();

            foreach (SummaryCinema cinema in mSummaryCinemas) newCinemas.Add(mConverter.ConvertCinema(cinema));

            foreach (SummaryCinema cinema in mLinkedCinemas) linkedCinemas.Add(mConverter.ConvertCinema(cinema));

            List<List<string[]>> cinemas = new List<List<string[]>>(newCinemas);
            cinemas.AddRange(linkedCinemas);

            if (FileManager.SaveFile(cinemas, TB_Path_Result.Text))
            {
                mDatabaseManager.AddConvertationResults(mSummaryCinemas);
                RefreshUnload();
                B_PrevCinema.IsEnabled = false;
            }
        }

        private void B_SaveFilm_Click(object sender, RoutedEventArgs e)
        {
            string dates = "";
            if (mUnload.rows.Count != 0) dates = mUnload.rows[0].dates;
            else dates = mUnload.linkedEntities[0].dates;

            mConverter = new TableConverter(TB_FilmID.Text, TB_FilmName.Text, TB_FilmFormat.Text, dates);
            LoadCinemaInfo(mCurrentIndex);

            ActivateFilmInfo(false);
            ActivateCinemaInfoForLinked();
        }

        private void ActivateFilmInfo(bool activate)
        {
            TB_FilmName.IsEnabled = activate;
            TB_FilmID.IsEnabled = activate;
            B_SaveFilm.IsEnabled = activate;
            TB_FilmFormat.IsEnabled = activate;
        }

        private void ActivateCinemaInfoForLinked()
        {
            bool linked = mCurrentIndex >= mUnload.rows.Count && mCurrentIndex < GetRowsCount() - 1;
            TB_City.IsEnabled = !linked;
            TB_CinemaName.IsEnabled = !linked;
            LB_ConverterScreens.IsEnabled = !linked;
            TB_TimeStart.IsEnabled = true;
            TB_TimeEnd.IsEnabled = true;
            TB_CurrentCinema.IsEnabled = !linked;
            B_SaveFile.IsEnabled = true;
            B_Skip.IsEnabled = linked;
        }

        private void ActivateCinemaInfo(bool activate)
        {
            TB_City.IsEnabled = activate;
            TB_CinemaName.IsEnabled = activate;
            LB_ConverterScreens.IsEnabled = activate;
            TB_TimeStart.IsEnabled = activate;
            TB_TimeEnd.IsEnabled = activate;
            TB_CurrentCinema.IsEnabled = activate;
            B_SaveFile.IsEnabled = activate;
        }

        private void TB_CurrentCinema_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int index = 0;
                if (Int32.TryParse(TB_CurrentCinema.Text, out index))
                {
                    if (index <= CalculateCurrentLength() && index > 0)
                    {
                        LoadCinemaInfo(index - 1);
                        //TB_CurrentCinema.Focus();
                    }
                    else
                    {
                        MessageBox.Show("Введённое число вне диапазона [1.." + CalculateCurrentLength().ToString() + "].\nНельзя переместиться по индексу на неинициализированный элемент.");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Введено НЕ число.");
                }
            }

        }

        private void LB_ConverterScreens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = LB_ConverterScreens.SelectedIndex;
            if (index >= 0) LB_Dimensionalities.ItemsSource = GetCurrentSummaryElement().screens[index].dimensionalities;
        }
        
        private List<SummaryCinema> GetActualSummaryList()
        {
            if (mCurrentIndex < mUnload.rows.Count) return mSummaryCinemas;
            else return mLinkedCinemas;
        }

        private SummaryCinema GetCurrentSummaryElement()
        {
            if (mCurrentIndex < mUnload.rows.Count)
                return mSummaryCinemas[mCurrentIndex];
            else
                return mLinkedCinemas[mCurrentIndex - mUnload.rows.Count];
        }

        private int CalculateCurrentLength()
        {
            return mSummaryCinemas.Count + mLinkedCinemas.Count;
        }

        private void RefreshNavigationButtons()
        {
            B_PrevCinema.IsEnabled = (mCurrentIndex > 0);
            B_NextCinema.IsEnabled = (mCurrentIndex != GetRowsCount() - 1) && (GetCurrentSummaryElement().screens.Count == Int32.Parse(mCurrentRow.screensCount));
            TB_CurrentCinema.IsEnabled = (mCurrentIndex > 2);

        }

        private void RefreshCinemasCount()
        {
            L_Progress.Content = "Инициализировано " + CalculateCurrentLength().ToString() + "/" + GetRowsCount().ToString() + " элементов";
        }

        private void RefreshScreensCount()
        {
            L_ScreensCount.Content = "[" + LB_ConverterScreens.Items.Count.ToString() + "/" + mCurrentRow.screensCount.ToString() + "]";
        }

        private void RefreshScreensButtons()
        {
            B_AddScreen.IsEnabled = mCurrentIndex < mUnload.rows.Count && (LB_ConverterScreens.Items.Count < Int32.Parse(mCurrentRow.screensCount));
            B_RemoveScreen.IsEnabled = mCurrentIndex < mUnload.rows.Count && (LB_ConverterScreens.Items.Count > 0);
        }
        
        private int GetRowsCount()
        {
            return mUnload.rows.Count + mUnload.linkedEntities.Count;
        }

        private void B_AddScreen_Click(object sender, RoutedEventArgs e)
        {
            AddScreenWindow window = new AddScreenWindow(mCurrentRow.dimensionalities);
            if (window.ShowDialog() == true)
            {
                LB_ConverterScreens.ItemsSource = null;

                SummaryCinema cinema = GetCurrentSummaryElement();
                Screen screen = new Screen(cinema.screens.Count, window.aswScreenName, window.aswDimensionalities);
                cinema.screens.Add(screen);
                cinema.screenNames.Add(window.aswScreenName);

                LB_ConverterScreens.ItemsSource = cinema.screenNames;

                RefreshScreensButtons();
                RefreshScreensCount();
                RefreshNavigationButtons();
            }
        }

        private void B_RemoveScreen_Click(object sender, RoutedEventArgs e)
        {
            SummaryCinema cinema = GetCurrentSummaryElement();
            List<Screen> screens = cinema.screens;
            Screen screen = screens[LB_ConverterScreens.SelectedIndex];

            screens.Remove(screen);
            cinema.screenNames.Remove(screen.name);

            LB_ConverterScreens.ItemsSource = null;
            LB_ConverterScreens.ItemsSource = cinema.screenNames;

            RefreshScreensButtons();
            RefreshScreensCount();
            RefreshNavigationButtons();

        }

        private void B_Path_Result_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SetTextBoxInfo(2, folderBrowserDialog.SelectedPath);
            }
        }

        // <<!-- CONVERTER -->> \\








        // <<-- ASSOCIATOR -->> \\

        private void B_Associate_Click(object sender, RoutedEventArgs e)
        {
            var item = LB_Codenames.SelectedItem;

            if (item != null)
            {
                if (String.IsNullOrEmpty(TB_AssociatorCity.Text))
                {
                    MessageBox.Show("Введите название города справа.");
                    return;
                }

                string selectedItem = LB_Codenames.SelectedItem.ToString();
                CinemaModel model = new CinemaModel();

                string screens = "";
                foreach (string screen in LB_AssociatorScreens.Items)
                {
                    screens += screen + " ";
                }
                screens = screens.Remove(screens.Length - 1, 1);


                string dimensionalities = "";
                foreach (List<string> screenDimensionality in mCurrentCinema.dimensionalities)
                {
                    foreach (string dimensionality in screenDimensionality)
                    {
                        dimensionalities += dimensionality + ",";
                    }
                    dimensionalities = dimensionalities.Remove(dimensionalities.Length - 1, 1);
                    dimensionalities += ".";
                }
                dimensionalities = dimensionalities.Remove(dimensionalities.Length - 1, 1);

                model.CinemaCodeName = selectedItem;
                model.Cinema = CB_PCinema.Text;
                model.City = CB_PCity.Text;
                model.CityCodeName = TB_AssociatorCity.Text;
                model.Screens = screens;
                model.Dimensionalities = dimensionalities;

                mDatabaseManager.AddItem(model);
                try { mDatabaseManager.Save(); }
                catch (DbUpdateException)
                {
                    MessageBox.Show("Кинотеатр \"" + model.CinemaCodeName + "\" уже есть в базе данных.");
                    return;
                }

                mAsk.cinemaCodenames.Remove((string)LB_Codenames.SelectedItem);
                L_PUnassociated.Content = mAsk.cinemaCodenames.Count.ToString();
                L_PAssociated.Content = (++mAssociated).ToString();

                RefreshCodenamesListBox();
            }
            else
            {
                MessageBox.Show("Не выбран элемент справа.");
            }
        }

        private void RefreshCodenamesListBox()
        {
            LB_Codenames.ItemsSource = null;
            LB_AssociatorDimensionalities.ItemsSource = null;

            string text = TB_Filter.Text;
            if (!String.IsNullOrEmpty(text)) LB_Codenames.ItemsSource = mAsk.Search(text);
            else LB_Codenames.ItemsSource = mAsk.cinemaCodenames;
        }

        private void RefreshDimensionalitiesListBox()
        {
            LB_AssociatorDimensionalities.ItemsSource = null;
            LB_AssociatorDimensionalities.ItemsSource = mCurrentCinema.dimensionalities[LB_AssociatorScreens.SelectedIndex];
        }

        private void LB_Codenames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox view = sender as ListBox;
            if (view.SelectedItem != null)
            {
                mCurrentCinema = mAsk.GetCinema(view.SelectedItem.ToString());
                string city = mCurrentCinema.city;
                if (!String.IsNullOrEmpty(city))
                {
                    TB_AssociatorCity.Foreground = Brushes.Gray;
                    TB_AssociatorCity.Text = city;
                }
                else
                {
                    TB_AssociatorCity.Foreground = Brushes.Black;
                    string[] parts = mCurrentCinema.name.Split('_');
                    int index = parts.Length >= 2 ? 1 : 0;
                    TB_AssociatorCity.Text = parts[index].ToUpper();
                }
                LB_AssociatorScreens.ItemsSource = mCurrentCinema.screens;
            }
            else
            {
                TB_AssociatorCity.Text = "";
                LB_AssociatorScreens.ItemsSource = null;
            }
        }

        private void LB_AssociatorScreens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_AssociatorScreens.SelectedIndex >= 0)
                LB_AssociatorDimensionalities.ItemsSource = mCurrentCinema.dimensionalities[LB_AssociatorScreens.SelectedIndex];
            else LB_AssociatorDimensionalities.ItemsSource = null;
        }

        private void B_AssociatorAddScreen_Click(object sender, RoutedEventArgs e)
        {
            AddDimensionalityToSelectedScreen();
        }

        private void B_AssociatorRemoveScreen_Click(object sender, RoutedEventArgs e)
        {
            int index = LB_AssociatorDimensionalities.SelectedIndex;
            if (index >= 0)
            {
                mCurrentCinema.dimensionalities[LB_AssociatorScreens.SelectedIndex].Remove(LB_AssociatorDimensionalities.Items[index].ToString());

                RefreshDimensionalitiesListBox();
            }

        }

        private void LB_AssociatorScreens_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddDimensionalityToSelectedScreen();
            }
        }

        private void AddDimensionalityToSelectedScreen()
        {
            string dimensionality = TB_Dimensionality.Text;
            if (String.IsNullOrEmpty(dimensionality))
            {
                MessageBox.Show("Введите формат.");
                return;
            }

            mCurrentCinema.dimensionalities[LB_AssociatorScreens.SelectedIndex].Add(dimensionality);

            RefreshDimensionalitiesListBox();
        }

        private void TB_Filter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) FilterCinemas();
        }

        private void B_Clear_Click(object sender, RoutedEventArgs e)
        {
            LB_Codenames.ItemsSource = mAsk.cinemaCodenames;
            TB_Filter.Text = "";
        }

        private void B_Search_Click(object sender, RoutedEventArgs e)
        {
            FilterCinemas();
        }

        private void FilterCinemas()
        {
            string text = TB_Filter.Text;
            if (!String.IsNullOrEmpty(text)) LB_Codenames.ItemsSource = mAsk.Search(text);
            else LB_Codenames.ItemsSource = mAsk.cinemaCodenames;
        }

        private void B_AddCity_Click(object sender, RoutedEventArgs e) { }

        private void B_AddCinema_Click(object sender, RoutedEventArgs e) { }
        
        private void SetAssociatorData()
        {
            LB_Codenames.ItemsSource = mAsk.cinemaCodenames;
            CB_PCity.ItemsSource = mUnload.cities;

            CB_PCity.IsEnabled = true;

            LB_Codenames.IsEnabled = true;
            LB_AssociatorScreens.IsEnabled = true;
            LB_AssociatorDimensionalities.IsEnabled = true;
            TB_Filter.IsEnabled = true;
            TB_Dimensionality.IsEnabled = true;
            B_AssociatorAddScreen.IsEnabled = true;
            TB_AssociatorCity.IsEnabled = true;
            B_Search.IsEnabled = true;
            B_Clear.IsEnabled = true;

            L_PUnassociated.Content = mAsk.cinemaCodenames.Count.ToString();
        }

        private void LB_AssociatorDimensionalities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = LB_AssociatorDimensionalities.SelectedIndex;
            if (index != -1 && LB_AssociatorDimensionalities.Items.Count > 1)
            {
                if (!mCurrentCinema.primalDimensionality.Equals(LB_AssociatorDimensionalities.Items[index].ToString()))
                {
                    B_AssociatorRemoveScreen.IsEnabled = true;
                    return;
                }
            }

            B_AssociatorRemoveScreen.IsEnabled = false;

        }

        private void CB_PCity_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CB_PCinema.IsEnabled = true;
            CB_PCinema.ItemsSource = mUnload.SearchCinemas((sender as ComboBox).SelectedItem.ToString());
        }

        private void CB_PCinema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            B_Associate.IsEnabled = (CB_PCity.SelectedIndex != -1);
        }


        // <<!- ASSOCIATOR -->> \\








        // <<-- DATABASE -->> \\

        private void B_Refresh_Click(object sender, RoutedEventArgs e) { RefreshDataGrid(); }
        
        private void B_Add_Click(object sender, RoutedEventArgs e) { }
        
        private void B_Delete_Click(object sender, RoutedEventArgs e)
        {
            CinemaModel item = (CinemaModel)DG_Linked.SelectedItem;
            mDatabaseManager.RemoveItem(item);
            mDatabaseManager.Save();
            RefreshDataGrid();
        }

        private void B_Edit_Click(object sender, RoutedEventArgs e) { }

        private void DG_Linked_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool activate = DG_Linked.SelectedItem != null;

            //B_Edit.IsEnabled = activate;
            B_Delete.IsEnabled = activate;
        }

        private void RefreshDataGrid()
        {
            List<CinemaModel> cinemas = mDatabaseManager.ToList();
            DG_Linked.ItemsSource = cinemas;
            mAssociated = cinemas.Count;
            L_PAssociated.Content = mAssociated;
        }

        // <<!- DATABASE -->> \\









        // <<-- GENERAL -->> \\
        private bool CheckReadiness() { return mAskReady && mUnloadReady; }

        private void B_Path_Unload_Click(object sender, RoutedEventArgs e)
        {
            string filepath = ChooseFile();
            if (!filepath.Equals(FILE_CHOOSING_ERROR))
            {

                SetTextBoxInfo(1, filepath);
                SetTextBoxInfo(2, GeneralMethods.CutPath(filepath, false));

                RefreshUnload();                

                B_Path_Result.IsEnabled = true;
            }
        }

        private void RefreshUnload()
        {
            mUnload = new EntityUnloadWorker(TB_Path_Unload.Text, mDatabaseManager.ToList());
            if (mUnload.state.Equals(ExcelWorker.STATE_ERROR)) return;

            SetFilmInfo();
            mUnloadReady = true;
            if (CheckReadiness()) SetAssociatorData();

            B_Path_Result.IsEnabled = true;
        }

        private void SetTextBoxInfo(int action, string path)
        {
            switch (action)
            {
                case 1:
                    mUnloadPath = path;
                    TB_Path_Unload.Text = path;
                    break;
                case 2:
                    mResultPath = path;
                    TB_Path_Result.Text = path;
                    break;
                case 3:
                    mAskPath = path;
                    TB_Path_Ask.Text = path;
                    break;
            }

        }

        private void B_Path_Ask_Click(object sender, RoutedEventArgs e)
        {
            string filepath = ChooseFile();
            if (!filepath.Equals(FILE_CHOOSING_ERROR))
            {
                mAsk = new EntityAskWorker(filepath, mDatabaseManager.ToList());
                if (mAsk.state.Equals(ExcelWorker.STATE_ERROR)) return;
                SetTextBoxInfo(3, filepath);

                mAskReady = true;
                if (CheckReadiness()) SetAssociatorData();
            }
        }

        private string ChooseFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Документы Excel (*.xls, *.xlsx)|*.xlsx;*.xls" };
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else return FILE_CHOOSING_ERROR;
        }

        // <<!- GENERAL -->> \\
    }
}