using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Microsoft.Xaml.Behaviors.Layout;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagRooms
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private Document _doc;
        public List<Level> Levels { get; } = new List<Level>();
        public List<RoomTagType> TagType { get; } = new List<RoomTagType>();
        public DelegateCommand AutoPlaceSpace { get; }
        public DelegateCommand PlaceSpace { get; }
        public DelegateCommand PlaceTagRoom { get; }
        public ObservableCollection<Room> Rooms { get; } = new ObservableCollection<Room>();

        private RevitTask revitTask;

        public string Name { get; set; }
        public double Number { get; set; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand OkCommand { get; }
        public object RoomList { get; }
        public Level SelectedLevels { get; set; }
        public RoomTagType SelectedTagType { get; set; }
        public Element SelectedRoom { get; set; }


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = commandData.Application.ActiveUIDocument.Document;
            Levels = Model.GetLevels(commandData);
            TagType = Model.GetRoomTagTypes(commandData);
            AutoPlaceSpace = new DelegateCommand(OnAutoPlaceSpace);
            PlaceSpace = new DelegateCommand(OnPlaceSpace);
            PlaceTagRoom = new DelegateCommand(OnPlaceTagRoom);
            List<Room> roomlist = Model.GetRooms(_doc);
            Rooms = Model.GetUniqueLevelOfRooms(_doc, roomlist);
            revitTask = new RevitTask();
            SaveCommand = new DelegateCommand(OnSaveCommand);
            OkCommand = new DelegateCommand(OnOkCommand);
        }
        private void OnOkCommand()
        {
            RaiseCloseRequest();
        }

        private async void OnSaveCommand()
        {
            RaiseHideRequest();
            string newName = Name;
            string newNumber = Number.ToString();
            Room room = SelectedRoom as Room;
            Rooms.Remove(room);                     
            await revitTask.Run(app =>
            {
                using (Transaction ts = new Transaction(_doc))
                {
                    try
                    {
                        ts.Start("SetName");
                        room.get_Parameter(BuiltInParameter.ROOM_NAME).Set(newName);
                        room.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(newNumber);
                        Rooms.Add(room);
                        ts.Commit();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            });

            RaiseShowRequest();
        }

        private async void OnPlaceSpace()
        {
            #region Ручное размещение помещений

            RaiseHideRequest();
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            ViewPlan view = doc.ActiveView as ViewPlan;
            Level level = view.GenLevel;
            await revitTask.Run(app =>
            {
                while (true)
                {
                    try
                    {
                        using (Transaction ts = new Transaction(doc, "Create Room"))
                        {
                            ts.Start();
                            XYZ xyz = uidoc.Selection.PickPoint("Select a point");
                            UV uv = new UV(xyz.X, xyz.Y);
                            Room room = doc.Create.NewRoom(level, uv);
                            Rooms.Add(room);
                            ts.Commit();
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {

                        break;

                    }

                };
            });
            TaskDialog.Show("Revit", "Помещения созданы");
            RaiseShowRequest();

            #endregion
        }

        private async void OnPlaceTagRoom()
        {
            RaiseHideRequest();
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            PlanTopology planTopology = doc.get_PlanTopology(SelectedLevels);

            await revitTask.Run(app =>
            {
                using (Transaction ts = new Transaction(doc, "Create RoomTag"))
                {
                    ts.Start();
                    foreach (ElementId eid in planTopology.GetRoomIds())
                    {
                        Room tmpRoom = doc.GetElement(eid) as Room;
                        if (doc.GetElement(tmpRoom.LevelId) != null && tmpRoom.Location != null)
                        {
                            LocationPoint locationPoint = tmpRoom.Location as LocationPoint;
                            UV point = new UV(locationPoint.Point.X, locationPoint.Point.Y);
                            RoomTag newTag = doc.Create.NewRoomTag(new LinkElementId(tmpRoom.Id), point, null);
                            newTag.RoomTagType = SelectedTagType;
                        }
                    }
                    ts.Commit();
                }
            });
            TaskDialog.Show("Revit", "Маркировка создана");
            RaiseShowRequest();
        }

        private async void OnAutoPlaceSpace()
        {
            #region Автоматическое создание помещений с маркой 
            RaiseHideRequest();
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<ElementId> roomsIds = new List<ElementId>();
            List<Room> roomlist = Model.GetRooms(_doc);
            FilteredElementCollector roomTags = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .WhereElementIsNotElementType();
            Element type = SelectedTagType;
            await revitTask.Run(app =>
            {
                try
                {
                    RaiseOutCloseRequest();
                    if (SelectedLevels != null)
                    {
                        if (SelectedTagType != null)
                        {
                            using (Transaction ts = new Transaction(doc, "rooms"))
                            {
                                ts.Start();
                                PlanTopology pt = doc.get_PlanTopology(SelectedLevels);
                                foreach (PlanCircuit pc in pt.Circuits)
                                {
                                    if (!pc.IsRoomLocated)
                                    {
                                        Room r = doc.Create.NewRoom(null, pc);
                                        Rooms.Add(r);
                                        roomsIds.Add(r.Id);
                                        foreach (RoomTag rt in roomTags)
                                        {
                                            if (roomsIds.Contains(rt.TaggedLocalRoomId))
                                            {
                                                rt.ChangeTypeId(type.Id);
                                            }
                                        }
                                    }
                                }
                                ts.Commit();
                                TaskDialog.Show("Revit", "Помещения созданы");
                            }
                        }
                        else
                        {
                            TaskDialog.Show("Ошибка", "Не выбран тип марки");
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Ошибка", "Не выбран уровень");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });
            RaiseShowRequest();
            #endregion
        }
        #region EventHandlers

        //свернуть
        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }
        //на передний план
        public event EventHandler OnRequestClose;
        private void RaiseOnCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OutRequestClose;
        private void RaiseOutCloseRequest()
        {
            OutRequestClose?.Invoke(this, EventArgs.Empty);
        }
        //закрыть
        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        //показать
        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
