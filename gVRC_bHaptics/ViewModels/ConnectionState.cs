using System.ComponentModel;

namespace gVRC_bHaptics.ViewModels
{
    internal class ConnectionState
    {
        public string Name { get; set; }
        public bool Connected { get; set; }
        public string ConnectionText 
        { 
            get
            {
                if (Connected)
                {
                    return "Ready";

                }
                else
                {
                    return "Offline";
                }
            }
        }

        public ConnectionState(string name) { 
            Name = name;
        }

    }
}
