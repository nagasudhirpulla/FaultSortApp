using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace FaultSortApp.FetchLogic
{
    public enum DataQuality
    {
        Good,
        Suspect,
        Garbage,
        Replaced
    };

    public enum DataQualityReason
    {
        None,
        PMUSync,
        PMUError
    };

    [Serializable]
    public class PMUDataStructure
    {
        public DateTime TimeStamp { get; set; }
        public float[] Value { get; set; }
        public DataQuality Quality { get; set; }
        public DataQualityReason QualityReason { get; set; }

        public PMUDataStructure(DateTime dateTime, float value1, float value2)
        {
            TimeStamp = dateTime;
            Value = new float[2] { value1, value2 };
        }

        public PMUDataStructure(DateTime dateTime, float value)
        {
            TimeStamp = dateTime;
            Value = new float[1] { value };
        }

        public PMUDataStructure(DateTime dateTime, float[] value, DataQuality quality = DataQuality.Good, DataQualityReason qualityReason = DataQualityReason.None)
        {
            TimeStamp = dateTime;
            Value = value;
            Quality = quality;
            QualityReason = qualityReason;
        }

        public PMUDataStructure()
        {
        }
    }    
}
