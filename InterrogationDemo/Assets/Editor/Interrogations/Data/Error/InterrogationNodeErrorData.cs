using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Error
{
    using Elements;

    public class InterrogationNodeErrorData
    {
        public InterrogationErrorData ErrorData { get; set; }
        public List<BaseNode> NodeList { get; set;}

        public InterrogationNodeErrorData()
        {
            ErrorData = new InterrogationErrorData();
            NodeList = new List<BaseNode>();
        }
    }
}


