using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text;
using wepApi.Classes;
using wepApi.Controllers;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace wepApi.Models
{
    public class Value
    {
        private int _valueID { get; set; }
        private int _valueFileID { get; set; }
        private DateTime _valueDateOfStart { get; set; }
        private int _valueExecTime { get; set; }
        private double _valueDecVal { get; set; }

        public int ValueID { get { return _valueID; } set { _valueID = value; } }
        public int ValueFileID { get { return _valueFileID; } set { _valueFileID = value; } }
        public DateTime ValueDateOfStart { get { return _valueDateOfStart; } set { _valueDateOfStart = value; } }
        public int ValueExecTime { get { return _valueExecTime; } set { _valueExecTime = value; } }
        public double ValueDecimalVal { get { return _valueDecVal; } set { _valueDecVal = value; } }

        public List<Value> GetDataFromFile(IFormFile fileToGetDataFrom)
        {
            string fileContents = string.Empty;
            List<Value> valuesFromFile = new List<Value>();
            Value value;
            int dateTimeIndex = 0;
            int secondsIndex = 1;
            int valueInDecIndex = 2;
            int counter = 0;
            //getting datetime
            using (StreamReader stRead = new StreamReader(fileToGetDataFrom.OpenReadStream()))
            {
                while ((fileContents = stRead.ReadLine()) != null)
                {
                    value = new Value();
                    if (fileContents.EndsWith(';') | fileContents.StartsWith(';') | !fileContents.Contains(';') | fileContents.Count(c => c == ';') > 2)
                    {
                        MainController.errorAsString = ShowBadRequestWithMessage("Incorrect file layout! try file with correct layout");
                        return null;
                    }
                    string[] split = fileContents.Split(';');
                    DateTime utcDate = DateTime.ParseExact(split[dateTimeIndex], "yyyy-MM-dd HH-mm-ss.ffff", CultureInfo.InvariantCulture);

                    if (utcDate < new DateTime(2000, 1, 1, 0, 0, 0) | utcDate > DateTime.Now)
                    {
                        MainController.errorAsString = ShowBadRequestWithMessage($"Incorrect date type in row {valuesFromFile.Count}");
                        return null;
                    }
                    value.ValueDateOfStart = utcDate;
                    value.ValueExecTime = CheckIfStringIsInt(split[secondsIndex]);
                    if (value.ValueExecTime < 0)
                    {
                        MainController.errorAsString = ShowBadRequestWithMessage("Execution time cannot be less than 0 (<0) or typed incorrectly");
                        return null;
                    }
                    value.ValueDecimalVal = CheckIfStringIsDouble(split[valueInDecIndex]);
                    if (value.ValueExecTime < 0)
                    {
                        MainController.errorAsString = ShowBadRequestWithMessage("Indicator value cannot be less than 0 (<0) or typed incorrectly");
                        return null;
                    }

                    //добавить проверку на дату
                    valuesFromFile.Add(value);
                    counter++;

                    fileContents = string.Empty;
                    if (counter > 10000)
                    {
                        MainController.errorAsString = ShowBadRequestWithMessage("File cannot be proceeded! Above 10000 (10k) rows!");
                        return null;
                    }
                }

            }
            return valuesFromFile;
        }

        private int CheckIfStringIsInt(string stringToParse)
        {
            if(!int.TryParse(stringToParse, out int result))
            {
                return -1;
            }
            if (result < 0)
            {
                return -1;
            }
            return result;
        }
        private double CheckIfStringIsDouble(string stringToParse)
        {
            if (!double.TryParse(stringToParse, out double result))
            {
                return -1;
            }
            if (result < 0)
            {
                return -1;
            }
            return result;
        }

        private string ShowBadRequestWithMessage(string message)
        {
            return message;
        }
    }
}
