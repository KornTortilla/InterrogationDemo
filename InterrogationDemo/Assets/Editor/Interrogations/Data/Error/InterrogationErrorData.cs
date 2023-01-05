using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Error
{
    public class InterrogationErrorData
    {
        public Color Color { get; set; }

        public InterrogationErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32(
                (byte) Random.Range(200, 256),
                (byte) Random.Range(0, 51),
                (byte) Random.Range(0, 101),
                255
            );
        }
    }
}


