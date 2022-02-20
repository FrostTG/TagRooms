using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TagRooms
{
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private ExternalCommandData _commandData;
        Document _doc;
        private RevitTask revitTask;
        private string message;

        public MainView(ExternalCommandData commandData)
        {
            InitializeComponent();
            MainViewViewModel vm = new MainViewViewModel(commandData);
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            _doc = doc;
            vm.CloseRequest += (s, e) => this.Close();
            vm.HideRequest += (s, e) => this.Hide();
            vm.ShowRequest += (s, e) => this.Show();
            vm.OnRequestClose += (s, e) => this.Topmost = true;
            vm.OutRequestClose += (s, e) => this.Topmost = false;
            vm.RefreshRequest += Vm_RefreshRequest;
            DataContext = vm;

        }
        private void Vm_RefreshRequest(object sender, EventArgs e)
        {
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<Room> roomlist = Model.GetRooms(doc);
            AllRoomsView.ItemsSource = Model.GetUniqueLevelOfRooms(doc, roomlist);
        }
        private void EditTagComplete(object sender, SelectionChangedEventArgs args)
        {
            Room room = AllRoomsView.SelectedItem as Room;            
            txtName.Text = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
            txtNumber.Text = room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString();           
        }

        private void btnEditTag(object sender, RoutedEventArgs e)
        {
            txtName.IsEnabled = true;
            txtNumber.IsEnabled = true;
            combTagType.IsEnabled = true;
            SaveCommand.IsEnabled = true;
            NumberTag.IsEnabled = true;
            NameTag.IsEnabled = true;
            TagType.IsEnabled = true;
            txtLevel.IsEnabled = false;
            txtTagTypeMain.IsEnabled = false;
            AllRoomsView.IsEnabled = false;
            levelsComboBox.IsEnabled = false;
            cmbxTagType.IsEnabled = false;
            EditTag.IsEnabled = false;
            btnPlaceSpace.IsEnabled = false;
            btnCreateTag.IsEnabled = false;
            btnAutoPlaceSpace.IsEnabled = false;
            btnOk.IsEnabled = false;
            btnCancel.IsEnabled = false;
        }

        private void EditTagComplete(object sender, RoutedEventArgs e)
        {
            txtName.IsEnabled = false;
            txtNumber.IsEnabled = false;
            combTagType.IsEnabled = false;
            SaveCommand.IsEnabled = false;
            NumberTag.IsEnabled = false;
            NameTag.IsEnabled = false;
            TagType.IsEnabled = false;
            txtLevel.IsEnabled = true;
            txtTagTypeMain.IsEnabled = true;
            AllRoomsView.IsEnabled = true;
            levelsComboBox.IsEnabled = true;
            cmbxTagType.IsEnabled = true;
            EditTag.IsEnabled = true;
            btnPlaceSpace.IsEnabled = true;
            btnCreateTag.IsEnabled = true;
            btnAutoPlaceSpace.IsEnabled = true;
            btnOk.IsEnabled = true;
            btnCancel.IsEnabled = true;

           
            //Получаем значение текста для нового имени
            string newName = txtName.Text;
            string newNumber = txtNumber.Text;
            RoomTagType newRoomTag = cmbxTagType.SelectedItem as RoomTagType;
            //получаем помещение
            Room room = AllRoomsView.SelectedItem as Room;
            RoomTag roomTag = AllRoomsView.SelectedItem as RoomTag;
            //записываем новое имя
            try
            {
                using (Transaction t = new Transaction(_doc))
                {
                    t.Start("SetName");
                    room.get_Parameter(BuiltInParameter.ROOM_NAME).Set(newName);
                    room.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(newNumber);
                    roomTag.RoomTagType = newRoomTag;
                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }           

            //обновляем UI список
            AllRoomsView.Items.Refresh();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            AllRoomsView.Items.Clear();
            AllRoomsView.Items.Refresh();
        }
    }
}
