using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Data.Json;

// Шаблон элемента пользовательского элемента управления задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234236

namespace Tasks_
{
    public sealed partial class PreferencesUserControl : UserControl
    {
        public PreferencesUserControl()
        {
            this.InitializeComponent();
        }

        private async void SaveSettings()
        {
            //зберігаємо налаштування вибору елемента в двох комбо
            string SavedSetting = "";
            ComboBoxItem fon = (ComboBoxItem)FonCombo.SelectedItem;
            ComboBoxItem cat = (ComboBoxItem)categoryCombo.SelectedItem;
            SavedSetting = "[{\"Fon\" : \"" + fon.Content.ToString() + "\"}, {\"CatFon\" : \"" + cat.Content.ToString() + "\"}]"; //строка з налаштуваннями
            
            //записуємо строку в файл
            try
            {
                StorageFolder current = ApplicationData.Current.LocalFolder;
                StorageFile file = await current.CreateFileAsync("Settings.txt", CreationCollisionOption.ReplaceExisting);  //створюємо файл? якшо є то заміняємо
                await FileIO.WriteTextAsync(file, SavedSetting);        //записуємо налаштування
            }
            catch
            { }//MessageShow("Error"); }
        }

        private async void OpenPicker(string f)
        {
            FileOpenPicker fop = new FileOpenPicker();
            fop.ViewMode = PickerViewMode.Thumbnail;
            fop.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fop.FileTypeFilter.Add(".png");
            fop.FileTypeFilter.Add(".jpg");
            fop.FileTypeFilter.Add(".bmp");
            fop.FileTypeFilter.Add(".jpeg");

            StorageFile file = await fop.PickSingleFileAsync();
            if (file != null)
            {
                if (f == "Fon")  //якщо ми клікнули фон то копіюємо вибрану картинку і іменем Fon.png
                {
                    StorageFile pic = await file.CopyAsync(ApplicationData.Current.LocalFolder, "Fon.png", NameCollisionOption.ReplaceExisting); //копіюємо картину
                }
                else if (f == "Category")
                {
                    StorageFile pic = await file.CopyAsync(ApplicationData.Current.LocalFolder, "CatFon.png", NameCollisionOption.ReplaceExisting); //копіюємо картину
                }
                //зберігаємо свій вибір товбто зобр якшо ми його вибрали
                SaveSettings();
            }
        }

        /// <summary>
        /// Клік на елементі фон комбо
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FonComBoTaped(object sender, TappedRoutedEventArgs e)
        {
            // якщо ми клікнули на пункт зображення відкриваємо файл пікер
            if (FonCombo.SelectedItem == pict)
                OpenPicker("Fon");
            else           //якщо ні то ми клікнули на колір і треба зберегти вибір
                SaveSettings();
        }

        /// <summary>
        /// Клік на елементі комбо
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CatComBoxTaped(object sender, TappedRoutedEventArgs e)
        {
            // якщо ми клікнули на пункт зображення відкриваємо файл пікер
            if (categoryCombo.SelectedItem == pic)
                OpenPicker("Category");
            else
                SaveSettings();
        }


        /// <summary>
        /// виконується пр загрузці вікна налаштувань
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile file;
            try
            {
                // Відкриваємо та читаємо Settings,txt
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("Settings.txt");
            }
            catch (FileNotFoundException)
            {
                file = null;
            }
            if (file != null)
            {
                var result = await FileIO.ReadTextAsync(file);
                // Перетворюємо прочитане в JsonArray
                var Settings = JsonArray.Parse(result);
                // Перетворюємо JSON в настройки
                CreateSettings(Settings);
            }
        }
        
        private void CreateSettings(JsonArray array)
        {
            foreach (var item in array)
            {
                var obj = item.GetObject();
                string fon="", cat="";
                foreach (var key in obj.Keys)
                {
                    IJsonValue val;
                    if (!obj.TryGetValue(key, out val))
                        continue;
                    switch (key)
                    {
                        case "Fon":                         //шукаємо Fon 
                            fon = val.GetString();   //і присвоюємо його зн. до fon
                            break;
                        case "CatFon":                         //шукаємо CatFon 
                            cat = val.GetString();   //і присвоюємо його зн. до cat
                            break;
                    }
                }

                foreach (ComboBoxItem cb in categoryCombo.Items)
                {
                    //якщо хоч 1 елемент має такий самий контент як в налашт то поставити то й ел. як вибраний
                    if (cb.Content.ToString() == cat)       
                        categoryCombo.SelectedItem = cb;
                }

                foreach (ComboBoxItem cb in FonCombo.Items)
                {
                    if (cb.Content.ToString() == fon)
                        FonCombo.SelectedItem = cb;
                }
            }
        }
    }
}
