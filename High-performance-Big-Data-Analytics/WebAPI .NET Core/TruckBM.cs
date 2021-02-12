using AFC.DataAccess;
using AFC.DataAccess.Interface;
using AFC.ErrorLog;
using AFC.Models;
using AFC.Repositories;

using i4C.Models.CASA.SAMSUser;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using System.Data;

namespace AFC.BusinessManager
{
    public class TruckBM : ITruckRepository
    {
        private readonly IDapperHelper _daper;
        private readonly IDbHelper _DbHelper;
        TruckDA _TruckDA;
        Logging logging;
        public TruckBM(IDapperHelper daper, IDbHelper DbHelper)
        {
            _daper = daper;
            _DbHelper = DbHelper;
            _TruckDA = new TruckDA(_daper);
            logging = new Logging(_DbHelper);
        }


        //public async Task<IEnumerable<SAMSUser>> GetTrucks()
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<IEnumerable<SAMSUser>> GetTrucks()
        {
            IEnumerable<SAMSUser> getTrucks = null;

            //var connectionString_ = new Startup(_configuration); //---_configuration.GetConnectionString("ConnectionStrings:DefaultConnection"); 

            //var connectionString__ = connectionString_.connectionString3;
            //var text = Startup.connectionString;

            using (var sqlConnection = new SqlConnection(""))
            {
                await sqlConnection.OpenAsync();
                return await sqlConnection.QueryAsync<SAMSUser>(
                    "usp_GetSAMSUsers",
                    null,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public int ActiveInActiveTruckDetailsById(TruckStatusInput statusUpdate)
        {
            int result = 0;
            try
            {
                result = _TruckDA.ActiveInActiveTruckDetailsById(statusUpdate);
            }
            catch (Exception ex)
            {
                //logging.ExceptionLog(ex.Message.ToString(), SystemInfo.IP(), MethodInfo.GetCurrentMethod().ToString(), SystemInfo.Url());
            }
            return result;
        }

        public int DeleteTruckDetailsById(int TruckID)
        {
            int result = 0;
            try
            {
                result = _TruckDA.DeleteTruckDetailsById(TruckID);
            }
            catch (Exception ex)
            {
                //logging.ExceptionLog(ex.Message.ToString(), SystemInfo.IP(), MethodInfo.GetCurrentMethod().ToString(), SystemInfo.Url());
            }
            return result;
        }

        public async Task<Trucks> GetPostedTruckDetailsById(int TruckID)
        {
            Trucks PostedTruck = null;
            try
            {
                PostedTruck = await _TruckDA.GetPostedTruckDetailsById(TruckID);
            }
            catch (Exception ex)
            {
                //logging.ExceptionLog(ex.Message.ToString(), SystemInfo.IP(), MethodInfo.GetCurrentMethod().ToString(), SystemInfo.Url());
            }
            return PostedTruck;
        }

        public async Task<IList<Trucks>> GetTruckPostedList(TruckSearchInput SearchInput)
        {
            IList<Trucks> PostedTruckDetails = null;
            try
            {
                PostedTruckDetails = await _TruckDA.GetTruckPostedList(SearchInput);
            }
            catch (Exception ex)
            {
                //logging.ExceptionLog(ex.Message.ToString(), SystemInfo.IP(), MethodInfo.GetCurrentMethod().ToString(), SystemInfo.Url());
            }
            return PostedTruckDetails;
        }
        public async Task<IList<Trucks>> SearchPostedTruckList(TruckSearchInput SearchInput)
        {
            IList<Trucks> PostedTruckDetails = null;
            try
            {
                PostedTruckDetails = await _TruckDA.SearchPostedTruckList(SearchInput);
            }
            catch (Exception ex)
            {
                //logging.ExceptionLog(ex.Message.ToString(), SystemInfo.IP(), MethodInfo.GetCurrentMethod().ToString(), SystemInfo.Url());
            }
            return PostedTruckDetails;
        }
...