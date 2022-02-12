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
        private PlanCircuit planCircuit;

        public List<Level> Levels { get; } = new List<Level>();
        public List<RoomTagType> TagType { get; } = new List<RoomTagType>();
        public DelegateCommand PlaceSpace { get; }
        public DelegateCommand PlaceTagRoom { get; }
        public Level SelectedLevels { get; set; }
        public RoomTagType SelectedTagType { get; set; }
        

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            Levels = Library.GetLevels(commandData);            
            TagType=Library.GetRoomTagTypes(commandData);
            PlaceSpace = new DelegateCommand(OnPlaceSpace);
            PlaceTagRoom = new DelegateCommand(OnPlaceTagRoom);
        }

        private void OnPlaceTagRoom()
        {
            throw new NotImplementedException();
        }

        private void OnPlaceSpace()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            View v = doc.ActiveView;
            Parameter viewPhase = v.get_Parameter(BuiltInParameter.VIEW_PHASE);
            string currentPhase = viewPhase.AsValueString();
            Element e=null;
            Phase phaseCreated = e.Document.GetElement(e.CreatedPhaseId);
            Room newShedueleRoom = doc.Create.NewRoom(phaseCreated);
            PlanTopology planTopology = doc.get_PlanTopology(SelectedLevels);
            foreach (PlanCircuit circuit in planTopology.Circuits)
            {
                if (circuit!=null)
                {
                    planCircuit = circuit;
                    break;
                }
            }
            Room newRoom2 = null;
            if (planCircuit!=null)
            {
                using (Transaction ts = new Transaction(doc, "Create Room"))
                {
                    if (ts.Start()==TransactionStatus.Started)
                    {
                        newRoom2 = doc.Create.NewRoom(newShedueleRoom, planCircuit);
                        if (newRoom2!=null)
                        {
                            TaskDialog.Show("Revit", "Room place in Plan Circuit successfully");
                        }
                        ts.Commit();
                    }
                }
            }
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
