namespace i4C.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using i4C.BAL;
    using i4C.DAL;
    using i4C.Models.IndicatorCategory;
    using i4C.Models.IndicatorSubCategory;
    using i4C.Models.Indicator;
    using i4C.Models.IndicatorSegment;
    using i4C.Models.DoctorIndicator;
    using i4C.Models.PracticeDoctors;
    using i4C.Models.IndicatorData;
    using i4C.Models.IndicatorPatient;
    using i4C.Models.IndicatorGraphicType;
    using i4C.ViewModels.IndicatorSegmentDataPatientDemographics;
    using i4C.Models.Demographics;
    using Microsoft.Extensions.Configuration;
    using i4C.Models.DoctorIndicator_IndicatorData_Indicator_IndicatorSegment;

    [Route("api/Dashboardi4C"), Produces("application/json"), EnableCors("AppPolicy")]
    public class Dashboardi4CController : Controller
    {
        private readonly IConfiguration _configuration;

        public readonly ILogger<Dashboardi4CController> _logger;
        public readonly C3_DASHBOARDDbContext _context;
        public Dashboardi4CRepository _dashboardi4C = new Dashboardi4CRepository();

        public Dashboardi4CController(IConfiguration configuration, C3_DASHBOARDDbContext context, ILogger<Dashboardi4CController> logger)
        {
            _logger = logger;
            _logger.LogInformation($"+++++++++++++++++++++ Dashboardi4CController.");
            _context = context;
            _configuration = configuration;
        }

        #region C3_DASHBOARDDbContext
        // GET api/Dashboardi4C/GetNewIndicatorDataById/1
        //[HttpGet, Route("GetNewIndicatorDataById/{id}")]
        //public async Task<IEnumerable<DoctorIndicator_IndicatorData_Indicator_IndicatorSegment>> GetNewIndicatorDataById(string id)
        //{
        //    //---var id = "1";
        //    return await _dashboardi4C.GetNewIndicatorDataById(id).ConfigureAwait(true);
        //}

        // GET api/DoctorSearch/GetPatientListByDoctorId/1
        //[HttpGet, Route("GetPaientListByDoctorId/{id}")]
        //public async Task<IEnumerable<DoctorIndicator_IndicatorData_Indicator_IndicatorSegment>> GetPatientListByDoctorId(string id)
        //{
        //    //---var id = "1";
        //    return await _doctorSearchBAL.GetPatientListByDoctorId(id).ConfigureAwait(true); 
        //}

        #endregion


        //-- 2.4.10 GetPatientListByIndicatorDataId
        //--- GET: api/Dashboardi4C/GetPatientListByIndicatorDataId/1
        [HttpGet, Route("GetPatientListByIndicatorDataId/{id}")]
        public async Task<IEnumerable<Demographics>> GetPatientListByIndicatorDataId(int id)
        {
            //---var data = await _officeBAL.GetAllOffice();
            var t = await this._dashboardi4C.GetPatientListByIndicatorDataId(id).ConfigureAwait(true);
            return t;
        }

        [HttpGet, Route("GetConfig_url_base1")]
        public async Task<IActionResult> GetConfig_url_base1()
        {
            try
            {
                _logger.LogInformation("...........url_base1 Called");

                //var client = BuildHttpClient();
                //var response = await client.GetAsync(ICBTAPIUrl + "/api/v1/user/test");
                //if (response.IsSuccessStatusCode)
                //{
                //var result = (TestResponse)await response.Content.ReadAsAsync(typeof(TestResponse));

                var connectionString_ = new Startup(_configuration);
                var connectionstring = connectionString_.connectionString1;

                var _url_base1 = connectionString_.url_base1;
                var _url_base2 = connectionString_.url_base2;

                return Ok(_url_base1);
                //}
                //else
                //{
                    //var errMsg = $"Test Failed";
                    //_logger.LogError(errMsg + ": (CbtController/Test)");
                    //return BadRequest(errMsg);
                //}

            }
            catch (Exception ex)
            {
                var errMsg = $"Test Failed";
                _logger.LogCritical(ex, errMsg + " (GetConfig_url_base1/test)");
                return BadRequest(errMsg);
            }
        }
...
