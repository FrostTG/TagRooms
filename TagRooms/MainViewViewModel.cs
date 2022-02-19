using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Microsoft.Xaml.Behaviors.Layout;
using Prism.Commands;
using System;
using System.Collections.Generic;
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
        public DelegateCommand EditTag { get; }
        public List<Room> Rooms { get; } = new List<Room>();

        private RevitTask revitTask;

        public object RoomList { get; }
        public Level SelectedLevels { get; set; }
        public RoomTagType SelectedTagType { get; set; }
        public List<Room> SelectedRooms { get; set; }


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = commandData.Application.ActiveUIDocument.Document;
            Levels = Model.GetLevels(commandData);
            TagType = Model.GetRoomTagTypes(commandData);
            AutoPlaceSpace = new DelegateCommand(OnAutoPlaceSpace);
            PlaceSpace = new DelegateCommand(OnPlaceSpace);
            PlaceTagRoom = new DelegateCommand(OnPlaceTagRoom);
            EditTag = new DelegateCommand(OnEditTag);
            List<Room> roomlist = Model.GetRooms(_doc);
            Rooms = Model.GetUniqueLevelOfRooms(_doc, roomlist);
            revitTask = new RevitTask();
        }

        private void OnEditTag()
        {
           
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
            List<Room> rooms = new List<Room>();            
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
                            rooms.Add(room);
                            #region
                            //PlanTopology planTopology = doc.get_PlanTopology(SelectedLevels);
                            //using (SubTransaction sts = new SubTransaction(doc))
                            //{
                            //    sts.Start();
                            //    foreach (ElementId eid in planTopology.GetRoomIds())
                            //    {
                            //        Room tmpRoom = doc.GetElement(eid) as Room;
                            //        if (doc.GetElement(tmpRoom.LevelId) != null && tmpRoom.Location != null)
                            //        {
                            //            LocationPoint locationPoint = tmpRoom.Location as LocationPoint;
                            //            UV point = new UV(locationPoint.Point.X, locationPoint.Point.Y);
                            //            RoomTag newTag = doc.Create.NewRoomTag(new LinkElementId(tmpRoom.Id), point, null);
                            //            newTag.RoomTagType = SelectedTagType;

                            //        }
                            //    }
                            //    sts.Commit();
                            #endregion

                            ts.Commit();
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
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
            #region
            //Library.GetRooms(_commandData);
            //Library.GetViews(_commandData);
            //using (SubTransaction ts1 = new SubTransaction(doc))
            //{
            //    ts1.Start();
            //    foreach (View view in Library.GetViews(_commandData))
            //    {
            //        foreach (ElementId roomid in Library.GetRooms(_commandData))
            //        {
            //            Element element = doc.GetElement(roomid);
            //            Room room = element as Room;
            //            XYZ roomcenter = Library.GetElemCenter(room);
            //            UV center = new UV(roomcenter.X, roomcenter.Y);
            //            RoomTag roomTag= doc.Create.NewRoomTag(new LinkElementId(roomid), center, view.Id);
            //            roomTag.RoomTagType = SelectedTagType;
            //        }
            //    }
            //    ts1.Commit();
            //}
            #endregion
            PlanTopology planTopology = doc.get_PlanTopology(SelectedLevels);

            await revitTask.Run(app =>
            {
                using (Transaction ts = new Transaction(doc, "jhg"))
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
            List<Room> rooms = new List<Room>();

            await revitTask.Run(app =>
            {
                try
                {

                    RaiseOutCloseRequest();
                    if (SelectedLevels != null)
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

                                    rooms.Add(r);
                                }
                            }
                            ts.Commit();
                            TaskDialog.Show("Revit", "Помещения созданы");
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
                RaiseOnCloseRequest();
            });           
            RaiseShowRequest();
            #endregion
        }
        #region EventHandlers

        public event EventHandler RefreshRequest;
        private void RaiseRefreshRequest()
        {
            RefreshRequest?.Invoke(this, EventArgs.Empty);
        }

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
