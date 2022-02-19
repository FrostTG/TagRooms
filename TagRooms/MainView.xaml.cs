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
    }   
}
