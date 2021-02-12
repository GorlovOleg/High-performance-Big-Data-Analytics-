using AFC.CommonUtilities.Common;
using AFC.DataAccess.Interface;
using AFC.DataAccess;
using AFC.ErrorLog;
using AFC.Models;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AFC.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Data;

namespace i4C.BAL.AFC
{
    public class CityStateCountryRepository : ICityStateCountryRepository
    {
        private readonly IDapperHelper _daper;
        private readonly IDbHelper _DbHelper;
        CityStateCountryDA CSCountryDA;
        Logging logging;


        private readonly IConfiguration _configuration;
        public readonly ILogger<Dashboard1Repository> _logger;

        public CityStateCountryRepository()
        {
        }

        public async Task<IList<CityStateCountries>> GetCityStateCountryList(string SearchValue)
        {
            IList<CityStateCountries> CSCountryList = new List<CityStateCountries>();

            var connectionString_ = new Startup(_configuration);
            var connectionstring__ = connectionString_.connectionString;

            //var parameters = new DynamicParameters();

            try
            {
                using (IDbConnection db = new SqlConnection(connectionstring__))
                {
                    //CSCountryList = await CSCountryDA.GetCityStateCountryList(SearchValue);

                    IList<CityStateCountries> CityStateCountryList = null;

                    var parameters = new DynamicParameters();
                    parameters.Add("@SearchValue", SearchValue, DbType.String, ParameterDirection.Input, 255);

                    return (IList<CityStateCountries>)await db.QueryAsync<CityStateCountries>(
                        "usp_GetCityStateCountryList",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    //return CityStateCountryList;
                }

            }
            catch (Exception ex)
            {
                logging.ExceptionLog(ex.Message.ToString(), SystemInfo.IP(), MethodInfo.GetCurrentMethod().ToString(), SystemInfo.Url());
...
