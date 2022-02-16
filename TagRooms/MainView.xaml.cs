using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            DataContext = vm;
        }
        //public MainView(MainViewViewModel roomsData) : this()
        //{
        //    m_roomsData = roomsData;
        //    InitRoomListView();
        //}

        //private void InitRoomListView()
        //{
        //    roomsListView.
        //    this.roomsListView.Columns.Clear();

        //    // Create the columns of the roomsListView
        //    this.roomsListView.Columns.Add("Room Name");
        //    foreach (RoomTagType type in m_roomsData.RoomTagTypes)
        //    {
        //        this.roomsListView.Columns.Add(type.Name);
        //    }

        //    this.roomsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        //    this.roomsListView.FullRowSelect = true;

    }
}
