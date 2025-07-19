using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using wepApi.Classes;
using wepApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using System.Diagnostics;


namespace wepApi.Controllers
{
    [Route("api/[controller]/[action]")]

    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly FilesDbContext _context;
        public static string errorAsString = string.Empty;
        public MainController(FilesDbContext context)
        {
            _context = context;
            _context.Database.OpenConnection();
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            _context.Database.OpenConnection();
            Value val = new Value();
            string resOfFileCheck = new UploadHandler().UploadData(file);
            if (resOfFileCheck != "File is fine")
            {
                BadRequest(resOfFileCheck);
                return null;
            }
            List<Value> listToUpload = val.GetDataFromFile(file);
            if (listToUpload == null)
            {
                return BadRequest(errorAsString);
            }

            //get all files from File Table
            List<Models.File> filesFromDB = new List<Models.File>();
            filesFromDB = _context.Files.ToList();

            for (int i = 0; i < filesFromDB.Count; i++)
            {
                if (filesFromDB[i].FileName == file.FileName)
                {
                    Models.File fileToUpdate = filesFromDB[i];
                    Result resultToUpdate = new Result();
                    resultToUpdate = Result.GetResultFromValues(listToUpload);
                    resultToUpdate.FileID = filesFromDB[i].FileID;
                    for (int j = 0; j < listToUpload.Count; j++)
                    {
                        listToUpload[j].ValueFileID = filesFromDB[i].FileID;
                    }
                    UpdateDataInDatabase(fileToUpdate, listToUpload, resultToUpdate);
                    //update data

                    listToUpload = null;
                    val = null;
                    filesFromDB = null;
                    fileToUpdate = null;
                    resultToUpdate = null;
                    resOfFileCheck = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    return Ok("File was updated succesfully!");
                }
            }
            //add new file
            Models.File fileToAdd = new Models.File();
            fileToAdd.FileName = file.FileName;
            AddFileDataToDatabase(fileToAdd);

            filesFromDB = _context.Files.ToList();


            for (int i = 0; i < listToUpload.Count; i++)
            {
                listToUpload[i].ValueFileID = filesFromDB[filesFromDB.Count-1].FileID;
            }

            Result resultToUpload = new Result();
            resultToUpload = Result.GetResultFromValues(listToUpload);
            resultToUpload.FileID = filesFromDB[filesFromDB.Count-1].FileID;


            //add result to db
            AddResultDataToDatabase(resultToUpload);
            _context.SaveChanges();
            //add values to db
            AddValueDataToDatabase(listToUpload);
            listToUpload = null;
            val = null;
            filesFromDB = null;
            fileToAdd = null;
            resultToUpload = null;
            resOfFileCheck = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return Ok();
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        private void UpdateDataInDatabase(Models.File file, List<Value> valList, Result result)
        {
            try
            {
                //delete all data from vals table and replace them with new ones
                //reason why I do this is bcuz if previous file had more rows then current one it will flud db with outdated data
                _context.Values.Where(x => x.ValueFileID == file.FileID).ExecuteDelete();
                AddValueDataToDatabase(valList);
                var existingRes = _context.Set<Result>().FirstOrDefaultAsync(u => u.FileID == file.FileID) ?? throw new InvalidOperationException($"Result for file ID {file.FileID} not found");

                existingRes.Result.MinDateOnFirstLaunch = result.MinDateOnFirstLaunch;
                existingRes.Result.MaxDateOnFirstLaunch = result.MaxDateOnFirstLaunch;
                existingRes.Result.AvgExecTime = result.AvgExecTime;
                existingRes.Result.AvgIndicatorVal = result.AvgIndicatorVal;
                existingRes.Result.MinValIndicator = result.MinValIndicator;
                existingRes.Result.MaxValIndicator = result.MaxValIndicator;
                existingRes.Result.DeltaTimeInSec = result.DeltaTimeInSec;
                existingRes.Result.MedianByIndicator = result.MedianByIndicator;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
                return;
            }

        }
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<ActionResult<List<Value>>> GetLastTenValuesByFileName(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("File name must be provided");
                }

                // Находим все FileID, соответствующие имени файла
                var fileIds = await _context.Files
                    .Where(f => f.FileName == fileName)
                    .Select(f => f.FileID)
                    .ToListAsync();

                if (fileIds == null || !fileIds.Any())
                {
                    return NotFound($"No files found with name: {fileName}");
                }

                // Получаем последние 10 значений для найденных FileID
                var values = await _context.Values
                    .Where(v => fileIds.Contains(v.ValueFileID))
                    .OrderByDescending(v => v.ValueDateOfStart)
                    .Take(10)
                    .ToListAsync();

                return values;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<ActionResult<List<Result>>> GetFilteredResultsAsync(string? fileName = null, DateTime? firstOperationStartFrom = null,
                                                                              DateTime? firstOperationStartTo = null, double? averageIndicatorFrom = null,
                                                                              double? averageIndicatorTo = null, double? averageExecutionTimeFrom = null, 
                                                                              double? averageExecutionTimeTo = null)
        {
            try
            {
                var query = _context.Set<Result>().AsQueryable();

                if (!string.IsNullOrEmpty(fileName))
                {
                    query = query.Join(
                        _context.Set<Models.File>(),
                        r => r.FileID,
                        f => f.FileID,
                        (r, f) => new { Result = r, File = f }
                    )
                    .Where(j => j.File.FileName.Contains(fileName))
                    .Select(j => j.Result);
                }

                if (firstOperationStartFrom.HasValue != firstOperationStartTo.HasValue)
                {
                    return BadRequest("Both start and end dates for first operation range must be provided");
                }

                if (firstOperationStartFrom.HasValue && firstOperationStartTo.HasValue)
                {
                    query = query.Where(r =>
                        r.MinDateOnFirstLaunch >= firstOperationStartFrom.Value &&
                        r.MinDateOnFirstLaunch <= firstOperationStartTo.Value
                    );
                }

                if (averageIndicatorFrom.HasValue != averageIndicatorTo.HasValue)
                {
                    return BadRequest("Both min and max values for average indicator range must be provided");
                }

                if (averageIndicatorFrom.HasValue && averageIndicatorTo.HasValue)
                {
                    query = query.Where(r =>
                        r.AvgIndicatorVal >= averageIndicatorFrom.Value &&
                        r.AvgIndicatorVal <= averageIndicatorTo.Value
                    );
                }

                if (averageExecutionTimeFrom.HasValue != averageExecutionTimeTo.HasValue)
                {
                    return BadRequest("Both min and max values for average execution time range must be provided");
                }

                if (averageExecutionTimeFrom.HasValue && averageExecutionTimeTo.HasValue)
                {
                    query = query.Where(r =>
                        r.AvgExecTime >= averageExecutionTimeFrom.Value &&
                        r.AvgExecTime <= averageExecutionTimeTo.Value
                    );
                }

                var results = await query.ToListAsync();
                return results;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public void AddValueDataToDatabase(List<Value> valList)
        {
            try
            {
                foreach (Value val in valList)
                {
                    _context.Values.Add(val);
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
               BadRequest(ex.Message);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Ok();
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> AddResultDataToDatabase(Result val)
        {
            try
            {
                _context.Result.Add(val);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            return Ok();
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> AddFileDataToDatabase(Models.File val)
        {
            try
            {
                _context.Files.Add(val);
                _context.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Database error: {innerException}");
            }

        }
    }
}
