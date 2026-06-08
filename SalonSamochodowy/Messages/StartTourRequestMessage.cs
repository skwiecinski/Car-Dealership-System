using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SalonSamochodowy.Messages
{
    public class StartTourRequestMessage : RequestMessage<bool>
    {
        public string PageName { get; }

        public StartTourRequestMessage(string pageName)
        {
            PageName = pageName;
        }
    }
}
