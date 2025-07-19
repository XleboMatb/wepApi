namespace wepApi.Models
{
    public class Result
    {
        private int _resultID { get; set; }
        private int _fileID { get; set; }
        private int _deltaTimeInSec { get; set; }
        private DateTime _minDateOnFirstLaunch { get; set; }
        private DateTime _maxDateOnFirstLaunch { get; set; }
        private double _avgExecTime { get; set; }
        private double _avgIndicatorVal { get; set; }
        private double _medianByIndicator { get; set; }
        private double _minValIndicator { get; set; }
        private double _maxValIndicator { get; set; }

        public int ResultID { get { return _resultID; } set { _resultID = value; } }
        public int FileID { get { return _fileID; } set { _fileID = value; } }
        public int DeltaTimeInSec { get { return _deltaTimeInSec; } set { _deltaTimeInSec = value; } }
        public DateTime MinDateOnFirstLaunch { get { return _minDateOnFirstLaunch; } set { _minDateOnFirstLaunch = value; } }
        public DateTime MaxDateOnFirstLaunch { get { return _maxDateOnFirstLaunch; } set { _maxDateOnFirstLaunch = value; } }
        public double AvgExecTime { get { return _avgExecTime; } set { _avgExecTime = value; } }
        public double AvgIndicatorVal { get { return _avgIndicatorVal; } set { _avgIndicatorVal = value; } }
        public double MedianByIndicator { get { return _medianByIndicator; } set { _medianByIndicator = value; } }
        public double MinValIndicator { get { return _minValIndicator; } set { _minValIndicator = value; } }
        public double MaxValIndicator { get { return _maxValIndicator; } set { _maxValIndicator = value; } }

        public static Result GetResultFromValues(List<Value> values)
        {
            Result resultToReturn = new Result();
            resultToReturn.MinDateOnFirstLaunch = DateTime.MinValue;
            resultToReturn.FileID = values[0].ValueID;

            foreach (Value value in values)
            {
                if (value.ValueDateOfStart < resultToReturn.MinDateOnFirstLaunch | resultToReturn.MinDateOnFirstLaunch == DateTime.MinValue)
                {
                    resultToReturn.MinDateOnFirstLaunch = value.ValueDateOfStart;
                }
                if (value.ValueDateOfStart > resultToReturn.MaxDateOnFirstLaunch | resultToReturn.MaxDateOnFirstLaunch == DateTime.MinValue)
                {
                    resultToReturn.MaxDateOnFirstLaunch = value.ValueDateOfStart;
                }
                if (resultToReturn.MinValIndicator == 0 | resultToReturn.MinValIndicator > value.ValueDecimalVal)
                {
                    resultToReturn.MinValIndicator = value.ValueDecimalVal;
                }
                if (resultToReturn.MaxValIndicator == 0 | resultToReturn.MaxValIndicator < value.ValueDecimalVal)
                {
                    resultToReturn.MaxValIndicator = value.ValueDecimalVal;
                }
                resultToReturn.AvgExecTime += value.ValueExecTime;
                resultToReturn.AvgIndicatorVal += value.ValueDecimalVal;
            }
            resultToReturn.AvgExecTime = resultToReturn.AvgExecTime / values.Count;
            resultToReturn.AvgIndicatorVal = resultToReturn.AvgIndicatorVal / values.Count;
            resultToReturn.DeltaTimeInSec = (resultToReturn.MaxDateOnFirstLaunch - resultToReturn.MinDateOnFirstLaunch).Seconds;

            //median
            values = values.OrderBy(x => x.ValueDecimalVal).ToList();
            if (values.Count == 1)
            {
                resultToReturn.MedianByIndicator = values[values.Count - 1].ValueDecimalVal;
                return resultToReturn;
            }
            if (values.Count % 2 == 0)
            {
                resultToReturn.MedianByIndicator = values[(values.Count / 2) - 1].ValueDecimalVal;
                return resultToReturn;
            }
            resultToReturn.MedianByIndicator = (values[(int)(values.Count / 2) - 1].ValueDecimalVal + values[(int)(values.Count / 2)].ValueDecimalVal)/2;

            return resultToReturn;
        }
    }
}
