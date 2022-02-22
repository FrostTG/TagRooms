using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagRooms
{
    public class Model
    {
        public static List<Level> GetLevels(ExternalCommandData commandData)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();
            return levels;
        }
        public static List<ViewPlan> GetViews(ExternalCommandData commandData)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            List<ViewPlan> views = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan))
                .OfType<ViewPlan>()
                .ToList();
            return views;
        }
        public static List<RoomTagType> GetRoomTagTypes(ExternalCommandData commandData)
        {
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(commandData.Application.ActiveUIDocument.Document);
            filteredElementCollector.OfClass(typeof(FamilySymbol));
            filteredElementCollector.OfCategory(BuiltInCategory.OST_RoomTags);
            var m_roomTagTypes = filteredElementCollector.Cast<RoomTagType>().ToList();
            return m_roomTagTypes;
        }
        public static XYZ GetElemCenter(Element element)
        {
            BoundingBoxXYZ boxXYZ = element.get_BoundingBox(null);
            return (boxXYZ.Max + boxXYZ.Min) / 2;
        }
        public static List<Room> GetRooms(Document doc)
        {
            List<Room> roomlist = new FilteredElementCollector(doc)
                 .OfClass(typeof(SpatialElement))
                 .OfType<Room>()
                 .ToList();
            return roomlist;
        }

        public static ObservableCollection<Room> GetUniqueLevelOfRooms(Document document, List<Room> roomList)
        {
            ObservableCollection<Room> roomsList = new ObservableCollection<Room>();
            List<ElementId> RoomIdList = new List<ElementId>();
            foreach (Room room in roomList)
            {
                if (room.Area != 0)
                    RoomIdList.Add(room.Id);
            }
            List<ElementId> UniqueRoomlIdList = RoomIdList.Distinct().ToList();
            foreach (ElementId elemid in UniqueRoomlIdList)
            {
                if (elemid.IntegerValue > 0)
                    roomsList.Add(document.GetElement(elemid) as Room);
                continue;
            }            
            return roomsList;
        }
        public static Tuple<List<Room>, List<Level>> GetAllRooms(Document doc)
        {
            FilteredElementCollector newRoomFilter = new FilteredElementCollector(doc);
            ICollection<Element> allRooms = newRoomFilter.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements();

            List<Level> allRoomLevel = new List<Level>();

            List<string> levelNames = new List<string>();

            List<Room> allRoomsList = new List<Room>();

            Dictionary<string, List<Room>> allRoomsByLevelDict = new Dictionary<string, List<Room>>();

            foreach (Element roomEl in allRooms)
            {
                Room room = roomEl as Room;
                Level level = room.Level;

                if (allRoomsByLevelDict.ContainsKey(level.Name))
                    allRoomsByLevelDict[level.Name].Add(room);
                else
                {
                    List<Room> roomList = new List<Room>() { room };
                    allRoomsByLevelDict[level.Name] = roomList;
                }
                if (!levelNames.Contains(level.Name))
                {
                    levelNames.Add(level.Name);
                    allRoomLevel.Add(level);
                }
            }

            return Tuple.Create(allRoomsList, allRoomLevel);
        }


    }
}
