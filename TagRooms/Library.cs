using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagRooms
{
    public class Library
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
        public static List<RoomTagType> GetRoomTagTypes(ExternalCommandData commandData)
        {
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(commandData.Application.ActiveUIDocument.Document);
            filteredElementCollector.OfClass(typeof(FamilySymbol));
            filteredElementCollector.OfCategory(BuiltInCategory.OST_RoomTags);
            var m_roomTagTypes = filteredElementCollector.Cast<RoomTagType>().ToList();
            return m_roomTagTypes;
        }
    }
}
