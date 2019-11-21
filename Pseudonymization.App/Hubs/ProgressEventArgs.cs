using System;

namespace Pseudonymization.App.Hubs
{
    public class ProgressEventArgs : EventArgs
    {
        public int Percetange { get; set; }
    }
}