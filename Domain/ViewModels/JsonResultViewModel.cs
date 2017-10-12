using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Domain.ViewModels
{
    public class PanelStatus
    {
        public bool HasVoteUp { get; set; }
        public bool HasVoteDown { get; set; }
        public bool HasCollection { get; set; }
    }
}