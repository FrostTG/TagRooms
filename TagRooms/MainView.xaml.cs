using Autodesk.Revit.UI;
using System;
using System.Windows;

namespace TagRooms
{
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView(ExternalCommandData commandData)
        {
            InitializeComponent();
            MainViewViewModel vm = new MainViewViewModel(commandData);
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
            AllRoomsView.Items.Clear();                
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

        }
    }   
}
