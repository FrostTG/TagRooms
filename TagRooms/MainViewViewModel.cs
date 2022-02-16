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
        public List<Level> Levels { get; } = new List<Level>();
        public List<RoomTagType> TagType { get; } = new List<RoomTagType>();
        public DelegateCommand AutoPlaceSpace { get; }
        public DelegateCommand PlaceSpace { get; }
        public DelegateCommand PlaceTagRoom { get; }
        public object RoomList { get; }
        public Level SelectedLevels { get; set; }
        public RoomTagType SelectedTagType { get; set; }


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            Levels = Library.GetLevels(commandData);
            TagType = Library.GetRoomTagTypes(commandData);
            AutoPlaceSpace = new DelegateCommand(OnAutoPlaceSpace);
            PlaceSpace = new DelegateCommand(OnPlaceSpace);
            PlaceTagRoom = new DelegateCommand(OnPlaceTagRoom);
        }

        private void OnPlaceSpace()
        {
            #region Ручное размещение помещений

            RaiseHideRequest();
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            ViewPlan view = doc.ActiveView as ViewPlan;
            Level level = view.GenLevel;
            List<Room> rooms = new List<Room>();
            while (true)
            {
                try
                {
                    XYZ xyz = uidoc.Selection.PickPoint("Select a point");
                    UV uv = new UV(xyz.X, xyz.Y);
                    using (Transaction ts = new Transaction(doc, "Create Room"))
                    {
                        ts.Start();
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

            }
            TaskDialog.Show("Revit", "Помещения созданы");
            RaiseShowRequest();
            #endregion
        }

        private void OnPlaceTagRoom()
        {
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
            using (Transaction ts = new Transaction(doc,"jhg"))
            {
                ts.Start();
                ts.GetStatus();
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
        }

        private void OnAutoPlaceSpace()
        {

            #region Автоматическое создание помещений с маркой            
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<Room> rooms = new List<Room>();
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
                            //List<RoomTag> roomTags = new FilteredElementCollector(doc)
                            //    .OfClass(typeof(RoomTag))
                            //    .OfClass(typeof(Level))
                            //    .
                            //    .ToList();
                            
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
